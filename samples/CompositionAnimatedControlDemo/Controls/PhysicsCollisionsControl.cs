// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Skia.Composition;
using SkiaSharp;

namespace CompositionAnimatedControlDemo;

public sealed class PhysicsCollisionsControl : CompositionAnimatedControl
{
    private const float LogicalWidth = 12f;
    private const float LogicalHeight = 8f;

    private float _accumulator;
    private float _gravityY = -9.81f;
    private readonly PhysicsSimulation _simulation;

    public PhysicsCollisionsControl()
    {
        _simulation = new PhysicsSimulation(LogicalWidth, LogicalHeight, _gravityY);

        Update += DoStep;

        // Fallback to ensure animation starts when attached in different lifecycles
        AttachedToVisualTree += (_, _) => Start();
        DetachedFromVisualTree += (_, _) => Stop();
    }

    protected override Size OnGetSourceSize() => new(LogicalWidth, LogicalHeight);

    protected override NormalizeResult OnNormalizeElapsed(TimeSpan ts) => new(ts, false);

    protected override void OnUpdate(TimeSpan delta) => DoStep(delta);

    protected override void OnRender(SKCanvas canvas, Rect rect, TimeSpan _, bool __)
    {
        canvas.Clear(SKColors.White);
        var scaleX = (float)(rect.Width / LogicalWidth);
        var scaleY = (float)(rect.Height / LogicalHeight);
        var s = MathF.Min(scaleX, scaleY);

        var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = SKColors.SteelBlue;
        _simulation.ForEachBall((pos, radius) =>
        {
            var cx = (float)rect.X + pos.X * scaleX;
            var cy = (float)rect.Y + (LogicalHeight - pos.Y) * scaleY;
            canvas.DrawCircle(cx, cy, radius * s, paint);
        });
    }

    private void DoStep(TimeSpan delta)
    {
        const float step = 1f / 240f;
        var dt = (float)delta.TotalSeconds;
        _accumulator += dt;
        while (_accumulator >= step)
        {
            _simulation.Tick(step);
            _accumulator -= step;
        }
    }

    public void SetGravity(float gravityY)
    {
        _gravityY = gravityY;
        _simulation.SetGravity(_gravityY);
    }

    public void Restart()
    {
        _accumulator = 0f;
        _simulation.Restart();
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
