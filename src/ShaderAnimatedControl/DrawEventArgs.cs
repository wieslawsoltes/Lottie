using System;
using SkiaSharp;

namespace Avalonia.Skia.Composition;

public class DrawEventArgs : EventArgs
{
    internal DrawEventArgs(
        SKCanvas canvas, 
        Rect destRect, 
        TimeSpan effectiveElapsed, 
        bool isShaderFillCanvas, 
        SKRuntimeEffect effect,
        double shaderWidth,
        double shaderHeight,
        string? errorText)
    {
        Canvas = canvas;
        DestRect = destRect;
        EffectiveElapsed = effectiveElapsed;
        IsShaderFillCanvas = isShaderFillCanvas;
        Effect = effect;
        ShaderWidth = shaderWidth;
        ShaderHeight = shaderHeight;
        ErrorText = errorText;
    }
    
    public SKCanvas Canvas { get; }
    
    public Rect DestRect { get; }
    
    public TimeSpan EffectiveElapsed { get; }
    
    public bool IsShaderFillCanvas { get; }
    
    public SKRuntimeEffect Effect { get; }
    
    public double ShaderWidth { get; }
    
    public double ShaderHeight { get; }
    
    public string? ErrorText { get; }
}
