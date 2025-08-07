// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Avalonia.Skia.Composition;

public readonly struct NormalizeResult
{
    public TimeSpan EffectiveElapsed { get; }
    public bool HasLooped { get; }

    public NormalizeResult(TimeSpan effectiveElapsed, bool hasLooped)
    {
        EffectiveElapsed = effectiveElapsed;
        HasLooped = hasLooped;
    }

    public static NormalizeResult Create(TimeSpan effectiveElapsed, bool hasLooped)
        => new NormalizeResult(effectiveElapsed, hasLooped);
}

