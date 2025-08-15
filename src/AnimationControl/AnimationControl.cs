// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;

namespace Avalonia.Controls;

public abstract class AnimationControl : Control
{
    private TimeSpan _last;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _last = TimeSpan.Zero;
        RequestNextFrame();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _last = TimeSpan.Zero;
    }

    private void RequestNextFrame()
    {
        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan now)
    {
        if (_last != TimeSpan.Zero)
        {
            OnAnimationFrame(now, _last);
            InvalidateVisual();
        }
        _last = now;
        RequestNextFrame();
    }

    protected abstract void OnAnimationFrame(TimeSpan now, TimeSpan last);
}
