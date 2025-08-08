// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Skia.Composition;
using SkiaSharp;

namespace CompositionAnimatedControlDemo;

public sealed class StaticRedrawControl : CompositionAnimatedControl
{
    private SKColor _color = SKColors.DarkSlateGray;

    protected override Size OnGetSourceSize() => new(300, 200);
    protected override NormalizeResult OnNormalizeElapsed(TimeSpan ts) => new(TimeSpan.Zero, false);
    protected override void OnRender(SKCanvas canvas, Rect rect, TimeSpan _, bool __)
    {
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint();
        paint.Color = _color;
        paint.IsAntialias = true;
        canvas.DrawRect((float)rect.X + 20, (float)rect.Y + 20, (float)rect.Width - 40, (float)rect.Height - 40, paint);
    }

    public void Toggle()
    {
        _color = _color == SKColors.DarkSlateGray ? SKColors.OrangeRed : SKColors.DarkSlateGray;
        Redraw();
    }
}
