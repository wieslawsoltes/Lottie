// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Media;

namespace Avalonia.Skia.Composition;

internal enum VisualCommand
{
    Start,
    Stop,
    Update,
    Redraw,
    Dispose
}

internal readonly record struct VisualPayload(
    VisualCommand VisualCommand,
    int? RepeatCount = null,
    Stretch? Stretch = null,
    StretchDirection? StretchDirection = null);
