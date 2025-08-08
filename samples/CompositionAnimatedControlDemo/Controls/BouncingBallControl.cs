// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Skia.Composition;
using SkiaSharp;

namespace CompositionAnimatedControlDemo;

public sealed class BouncingBallControl : CompositionAnimatedControl
{
    private const float LogicalWidth = 300f;
    private const float LogicalHeight = 200f;
    private const float Radius = 18f;
    private SKPoint _position = new(LogicalWidth / 2f, LogicalHeight / 2f);
    private SKPoint _velocity = new(120f, 90f);

    protected override Size OnGetSourceSize() => new(LogicalWidth, LogicalHeight);
    protected override NormalizeResult OnNormalizeElapsed(TimeSpan elapsed) => new(elapsed, false);
    protected override void OnUpdate(TimeSpan delta)
    {
        var dt = (float)delta.TotalSeconds;
        if (dt <= 0) return;
        _position.X += _velocity.X * dt;
        _position.Y += _velocity.Y * dt;

        if (_position.X - Radius < 0) { _position.X = Radius; _velocity.X = Math.Abs(_velocity.X); }
        else if (_position.X + Radius > LogicalWidth) { _position.X = LogicalWidth - Radius; _velocity.X = -Math.Abs(_velocity.X); }

        if (_position.Y - Radius < 0) { _position.Y = Radius; _velocity.Y = Math.Abs(_velocity.Y); }
        else if (_position.Y + Radius > LogicalHeight) { _position.Y = LogicalHeight - Radius; _velocity.Y = -Math.Abs(_velocity.Y); }
    }
    protected override void OnRender(SKCanvas canvas, Rect rect, TimeSpan _, bool __)
    {
        canvas.Clear(SKColors.White);
        var scaleX = (float)(rect.Width / LogicalWidth);
        var scaleY = (float)(rect.Height / LogicalHeight);
        var cx = (float)rect.X + _position.X * scaleX;
        var cy = (float)rect.Y + _position.Y * scaleY;
        var r = Radius * Math.Min(scaleX, scaleY);
        using var ball = new SKPaint();
        ball.Color = SKColors.CornflowerBlue;
        ball.IsAntialias = true;
        canvas.DrawCircle(cx, cy, r, ball);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Start();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Stop();
    }
}
