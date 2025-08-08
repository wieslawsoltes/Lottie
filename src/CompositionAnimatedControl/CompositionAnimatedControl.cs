// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using SkiaSharp;

namespace Avalonia.Skia.Composition;

public abstract class CompositionAnimatedControl : Control
{
    private CompositionCustomVisual? _customVisual;

    public const int Infinity = -1;

    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<CompositionAnimatedControl, Stretch>(nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<CompositionAnimatedControl, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    public static readonly StyledProperty<int> RepeatCountProperty =
        AvaloniaProperty.Register<CompositionAnimatedControl, int>(nameof(RepeatCount), Infinity);

    public static readonly StyledProperty<double> PlaybackRateProperty =
        AvaloniaProperty.Register<CompositionAnimatedControl, double>(nameof(PlaybackRate), 1.0);

    public static readonly StyledProperty<TimeSpan> PositionProperty =
        AvaloniaProperty.Register<CompositionAnimatedControl, TimeSpan>(nameof(Position), TimeSpan.Zero);

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public int RepeatCount
    {
        get => GetValue(RepeatCountProperty);
        set => SetValue(RepeatCountProperty, value);
    }

    public double PlaybackRate
    {
        get => GetValue(PlaybackRateProperty);
        set => SetValue(PlaybackRateProperty, value);
    }

    public TimeSpan Position
    {
        get => GetValue(PositionProperty);
        private set => SetValue(PositionProperty, value);
    }

    public event Action<TimeSpan>? Update;
    public event Action? Started;
    public event Action? Stopped;
    public event Action? Disposed;

    protected override void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        base.OnLoaded(routedEventArgs);

        var elemVisual = ElementComposition.GetElementVisual(this);
        var compositor = elemVisual?.Compositor;
        if (compositor is null)
        {
            return;
        }

        var handler = new CompositionAnimatedCustomVisualHandler(
            () => OnGetSourceSize(),
            (canvas, destRect, elapsed, isRunning) => OnRender(canvas, destRect, elapsed, isRunning),
            (pos, running) => Dispatcher.UIThread.Post(() => Position = pos),
            delta => { OnUpdate(delta); Update?.Invoke(delta); },
            () => { OnStarted(); Started?.Invoke(); },
            () => { OnStopped(); Stopped?.Invoke(); },
            () => { OnDisposed(); Disposed?.Invoke(); },
            ts =>
            {
                var r = OnNormalizeElapsed(ts);
                return (r.EffectiveElapsed, r.HasLooped);
            });

        _customVisual = compositor.CreateCustomVisual(handler);
        ElementComposition.SetElementChildVisual(this, _customVisual);

        _customVisual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);
        _customVisual.SendHandlerMessage(new VisualPayload(VisualCommand.Update, null, Stretch, StretchDirection, PlaybackRate));

        LayoutUpdated += OnLayoutUpdated;
    }

    protected override void OnUnloaded(RoutedEventArgs routedEventArgs)
    {
        base.OnUnloaded(routedEventArgs);
        LayoutUpdated -= OnLayoutUpdated;
        Stop();
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Dispose));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(Stretch):
            case nameof(StretchDirection):
                _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Update, null, Stretch, StretchDirection, PlaybackRate));
                break;
            case nameof(RepeatCount):
                Stop();
                Start();
                break;
            case nameof(PlaybackRate):
                _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Update, null, Stretch, StretchDirection, PlaybackRate));
                break;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var sourceSize = OnGetSourceSize();
        return Stretch.CalculateSize(availableSize, sourceSize, StretchDirection);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var sourceSize = OnGetSourceSize();
        return Stretch.CalculateSize(finalSize, sourceSize);
    }

    public void Start()
    {
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Start, RepeatCount, Stretch, StretchDirection, PlaybackRate));
    }

    public void Stop()
    {
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Stop));
    }

    public void Pause()
    {
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Pause));
    }

    public void Resume()
    {
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Resume, RepeatCount, Stretch, StretchDirection, PlaybackRate));
    }

    public void Seek(TimeSpan position)
    {
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Seek, null, Stretch, StretchDirection, PlaybackRate, position));
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (_customVisual is null)
        {
            return;
        }

        _customVisual.Size = new Vector2((float)Bounds.Size.Width, (float)Bounds.Size.Height);
        _customVisual.SendHandlerMessage(new VisualPayload(VisualCommand.Update, null, Stretch, StretchDirection, PlaybackRate));
    }

    public void Redraw()
    {
        _customVisual?.SendHandlerMessage(new VisualPayload(VisualCommand.Redraw));
    }

    // Virtual hooks for subclass overrides (optional). If implemented, they run before delegates/events.
    protected virtual Size OnGetSourceSize() => default;
    protected virtual NormalizeResult OnNormalizeElapsed(TimeSpan elapsed) => new NormalizeResult(elapsed, false);
    protected virtual void OnRender(SKCanvas canvas, Rect destRect, TimeSpan effectiveElapsed, bool isRunning) { }
    protected virtual void OnUpdate(TimeSpan delta) { }
    protected virtual void OnStarted() { }
    protected virtual void OnStopped() { }
    protected virtual void OnDisposed() { }
}


