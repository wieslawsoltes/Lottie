using System;
using System.IO;
using Avalonia.Interactivity;
using Avalonia.Platform;
using SkiaSharp;

namespace Avalonia.Skia.Composition;

public class ShaderAnimatedControl : CompositionAnimatedControl
{
    public static readonly StyledProperty<Uri?> ShaderUriProperty =
        AvaloniaProperty.Register<ShaderAnimatedControl, Uri?>(nameof(ShaderUri));

    public static readonly StyledProperty<double> ShaderWidthProperty =
        AvaloniaProperty.Register<ShaderAnimatedControl, double>(nameof(ShaderWidth), defaultValue: 512);

    public static readonly StyledProperty<double> ShaderHeightProperty =
        AvaloniaProperty.Register<ShaderAnimatedControl, double>(nameof(ShaderHeight), defaultValue: 512);

    public static readonly StyledProperty<bool> IsShaderFillCanvasProperty =
        AvaloniaProperty.Register<ShaderAnimatedControl, bool>(nameof(IsShaderFillCanvas));
    
    private Uri? _shaderUri;
    private double _shaderWidth;
    private double _shaderHeight;
    private bool _isShaderFillCanvas;
    private Rect _bounds;
    private SKRuntimeEffect? _effect;
    private string? _errorText;

    public event EventHandler<DrawEventArgs>? Draw;

    public Uri? ShaderUri
    {
        get => GetValue(ShaderUriProperty);
        set => SetValue(ShaderUriProperty, value);
    }

    public double ShaderWidth
    {
        get => GetValue(ShaderWidthProperty);
        set => SetValue(ShaderWidthProperty, value);
    }

    public double ShaderHeight
    {
        get => GetValue(ShaderHeightProperty);
        set => SetValue(ShaderHeightProperty, value);
    }

    public bool IsShaderFillCanvas
    {
        get => GetValue(IsShaderFillCanvasProperty);
        set => SetValue(IsShaderFillCanvasProperty, value);
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
        DisposeEffect();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(ShaderUri):
            case nameof(ShaderWidth):
            case nameof(ShaderHeight):
            case nameof(IsShaderFillCanvas):
            case nameof(Bounds):
            {
                _shaderUri = ShaderUri;
                _shaderWidth = ShaderWidth;
                _shaderHeight = ShaderHeight;
                _isShaderFillCanvas = IsShaderFillCanvas;
                _bounds = Bounds;
                DisposeEffect();
                Redraw();
                break;
            }
        }
    }

    protected override Size OnGetSourceSize()
    {
        // TODO: Use stretch mode to determine the size.
        if (!_isShaderFillCanvas)
        {
            return new Size(_shaderWidth, _shaderHeight);
        }

        var size = _bounds.Size;
        return size is { Width: > 0, Height: > 0 } 
            ? size 
            : new Size(_shaderWidth, _shaderHeight);
    }

    protected override NormalizeResult OnNormalizeElapsed(TimeSpan elapsed) => new(elapsed, false);

    protected override void OnRender(SKCanvas canvas, Rect destRect, TimeSpan effectiveElapsed, bool isRunning)
    {
        EnsureEffect();
        Render(canvas, destRect, effectiveElapsed);
    }

    private void Render(SKCanvas canvas, Rect destRect, TimeSpan effectiveElapsed)
    {
        if (_effect is null)
        {
            return;
        }

        Draw?.Invoke(
            this, 
            new DrawEventArgs(
                canvas, 
                destRect, 
                effectiveElapsed, 
                _isShaderFillCanvas, 
                _effect,
                _shaderWidth,
                _shaderHeight,
                _errorText));
    }

    private void EnsureEffect()
    {
        if (_effect is not null || _shaderUri is null)
        {
            return;
        }

        using var stream = AssetLoader.Open(_shaderUri);
        using var txt = new StreamReader(stream);
        var shaderCode = txt.ReadToEnd();

        // SkiaSharp 3 API
        _effect = SKRuntimeEffect.CreateShader(shaderCode, out var errorText);
        _errorText = _effect == null ? errorText : null;
    }

    private void DisposeEffect()
    {
        _effect?.Dispose();
        _effect = null;
    }
}
