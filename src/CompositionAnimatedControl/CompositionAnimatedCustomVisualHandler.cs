// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using SkiaSharp;

namespace Avalonia.Skia.Composition;

internal class CompositionAnimatedCustomVisualHandler : CompositionCustomVisualHandler
{
    private readonly Func<Size> _getSourceSize;
    private readonly Action<SKCanvas, Rect, TimeSpan, bool> _render;
    private readonly Action<TimeSpan, bool> _onPositionChanged;
    private readonly Action<TimeSpan> _onUpdate;
    private readonly Action _onStarted;
    private readonly Action _onStopped;
    private readonly Action _onDisposed;
    private readonly Func<TimeSpan, (TimeSpan effectiveElapsed, bool hasLooped)> _normalizeElapsed;

    private TimeSpan _animationElapsed;
    private TimeSpan? _lastServerTime;
    private bool _running;
    private Stretch? _stretch;
    private StretchDirection? _stretchDirection;
    private int _repeatCount;
    private int _completedLoops;
    private double _playbackRate = 1.0;

    public CompositionAnimatedCustomVisualHandler(
        Func<Size> getSourceSize,
        Action<SKCanvas, Rect, TimeSpan, bool> render,
        Action<TimeSpan, bool> onPositionChanged,
        Action<TimeSpan> onUpdate,
        Action onStarted,
        Action onStopped,
        Action onDisposed,
        Func<TimeSpan, (TimeSpan effectiveElapsed, bool hasLooped)> normalizeElapsed)
    {
        _getSourceSize = getSourceSize;
        _render = render;
        _onPositionChanged = onPositionChanged;
        _onUpdate = onUpdate;
        _onStarted = onStarted;
        _onStopped = onStopped;
        _onDisposed = onDisposed;
        _normalizeElapsed = normalizeElapsed;
    }

    public override void OnMessage(object message)
    {
        if (message is not VisualPayload msg)
        {
            return;
        }

        switch (msg)
        {
            case { VisualCommand: VisualCommand.Start, RepeatCount: { } rp, Stretch: { } st, StretchDirection: { } sd }:
                _running = true;
                _lastServerTime = null;
                _stretch = st;
                _stretchDirection = sd;
                _repeatCount = rp;
                _completedLoops = 0;
                _animationElapsed = TimeSpan.Zero;
                _playbackRate = msg.PlaybackRate ?? 1.0;
                _onStarted();
                _onPositionChanged(TimeSpan.Zero, _running);
                RegisterForNextAnimationFrameUpdate();
                break;

            case { VisualCommand: VisualCommand.Update, Stretch: { } st, StretchDirection: { } sd }:
                _stretch = st;
                _stretchDirection = sd;
                _playbackRate = msg.PlaybackRate ?? _playbackRate;
                Invalidate();
                RegisterForNextAnimationFrameUpdate();
                break;

            case { VisualCommand: VisualCommand.Seek, Position: { } pos, Stretch: { } st, StretchDirection: { } sd }:
                _stretch = st;
                _stretchDirection = sd;
                _animationElapsed = pos;
                _lastServerTime = null;
                _completedLoops = 0;
                Invalidate();
                _onPositionChanged(_animationElapsed, _running);
                RegisterForNextAnimationFrameUpdate();
                break;

            case { VisualCommand: VisualCommand.Pause }:
                _running = false;
                _onPositionChanged(_animationElapsed, _running);
                break;

            case { VisualCommand: VisualCommand.Resume, Stretch: { } st, StretchDirection: { } sd }:
                _running = true;
                _stretch = st;
                _stretchDirection = sd;
                _playbackRate = msg.PlaybackRate ?? _playbackRate;
                _lastServerTime = null;
                _onPositionChanged(_animationElapsed, _running);
                RegisterForNextAnimationFrameUpdate();
                break;

            case { VisualCommand: VisualCommand.Redraw }:
                Invalidate();
                break;

            case { VisualCommand: VisualCommand.Stop }:
                _running = false;
                _animationElapsed = TimeSpan.Zero;
                _completedLoops = 0;
                _onStopped();
                _onPositionChanged(_animationElapsed, _running);
                break;

            case { VisualCommand: VisualCommand.Dispose }:
                _running = false;
                _onDisposed();
                break;
        }
    }

    public override void OnAnimationFrameUpdate()
    {
        if (!_running)
            return;

        if (_repeatCount == 0 || (_repeatCount > 0 && _completedLoops >= _repeatCount))
        {
            _running = false;
            // do not reset _animationElapsed here to preserve last frame for pause/stop behavior
        }

        Invalidate();
        RegisterForNextAnimationFrameUpdate();
    }

    public override void OnRender(ImmediateDrawingContext context)
    {
        if (_stretch is not { } st || _stretchDirection is not { } sd)
        {
            return;
        }

            if (_running)
        {
            if (_lastServerTime.HasValue)
            {
                    var delta = CompositionNow - _lastServerTime.Value;
                    var scaled = TimeSpan.FromTicks((long)(delta.Ticks * _playbackRate));
                    _animationElapsed += scaled;
                    _onUpdate(scaled);
            }
            _lastServerTime = CompositionNow;
        }

        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null)
        {
            return;
        }

        var rb = GetRenderBounds();
        var viewPort = new Rect(rb.Size);
        var sourceSize = _getSourceSize();
        if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
        {
            return;
        }

        var scale = st.CalculateScaling(rb.Size, sourceSize, sd);
        var scaledSize = sourceSize * scale;
        var destRect = viewPort.CenterRect(new Rect(scaledSize)).Intersect(viewPort);

        var (effectiveElapsed, looped) = _normalizeElapsed(_animationElapsed);
        if (looped)
        {
            _completedLoops++;
        }

        _onPositionChanged(effectiveElapsed, _running);

        using var lease = leaseFeature.Lease();
        var canvas = lease?.SkCanvas;
        if (canvas is null)
        {
            return;
        }

        canvas.Save();
        try
        {
            canvas.ClipRect(new SKRect((float)destRect.X, (float)destRect.Y, (float)destRect.Right, (float)destRect.Bottom));
            _render(canvas, destRect, effectiveElapsed, _running);
        }
        finally
        {
            canvas.Restore();
        }
    }
}
