using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Skia.Composition;
using SkiaSharp;

namespace EffectsDemo;

public partial class MainWindow : Window
{
    private SKRuntimeEffectUniforms? _uniforms;

    public MainWindow()
    {
        InitializeComponent();

        RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps;
        //RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps | RendererDebugOverlays.RenderTimeGraph | RendererDebugOverlays.LayoutTimeGraph;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        _uniforms?.Dispose();
        _uniforms = null;
    }

    private void ShaderAnimatedControl_OnDraw(object? sender, DrawEventArgs e)
    {
        if (e.ErrorText is not null)
        {
            return;
        }

        // TODO: Calculate target width and height based on the control's size.
        var targetWidth = (float)(e.IsShaderFillCanvas ? e.DestRect.Width : e.ShaderWidth);
        var targetHeight = (float)(e.IsShaderFillCanvas ? e.DestRect.Height : e.ShaderHeight);

        _uniforms ??= new SKRuntimeEffectUniforms(e.Effect);
        _uniforms["iTime"] = (float)e.EffectiveElapsed.TotalSeconds;
        _uniforms["iResolution"] = new[] { targetWidth, targetHeight, 0f };

        using var paint = new SKPaint();
        using var shader = e.Effect.ToShader(_uniforms);
        paint.Shader = shader;

        var rect = new SKRect(
            (float)e.DestRect.X, 
            (float)e.DestRect.Y, 
            (float)e.DestRect.Right, 
            (float)e.DestRect.Bottom);
        e.Canvas.DrawRect(rect, paint);
    }
}

