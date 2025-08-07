// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Skia.Composition;
using SkiaSharp;

namespace CompositionAnimatedControlDemo;

public sealed class SimpleTimelineControl : CompositionAnimatedControl
{
    protected override Size OnGetSourceSize() => new(300, 200);

    protected override NormalizeResult OnNormalizeElapsed(TimeSpan elapsed)
    {
        var loop = TimeSpan.FromSeconds(2);
        if (loop <= TimeSpan.Zero) return new NormalizeResult(elapsed, false);
        var remainder = TimeSpan.FromTicks(elapsed.Ticks % loop.Ticks);
        var looped = elapsed >= loop;
        return new NormalizeResult(remainder, looped);
    }

    protected override void OnRender(SKCanvas canvas, Rect rect, TimeSpan t, bool running)
    {
        canvas.Clear(SKColors.White);
        var progress = (float)(t.TotalSeconds / 2.0);
        var x = (float)(rect.X + rect.Width * progress);
        using var paint = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true };
        canvas.DrawCircle(x, (float)rect.Center.Y, 20, paint);
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
