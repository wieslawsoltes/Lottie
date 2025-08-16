# Lottie

[![CI](https://github.com/wieslawsoltes/Avalonia.Skia.Lottie/actions/workflows/build.yml/badge.svg)](https://github.com/wieslawsoltes/Avalonia.Skia.Lottie/actions/workflows/build.yml)

[![NuGet](https://img.shields.io/nuget/v/Lottie.svg)](https://www.nuget.org/packages/Lottie)
[![NuGet](https://img.shields.io/nuget/dt/Lottie.svg)](https://www.nuget.org/packages/Lottie)

[![NuGet](https://img.shields.io/nuget/v/AnimationControl.svg)](https://www.nuget.org/packages/AnimationControl)
[![NuGet](https://img.shields.io/nuget/v/CompositionAnimatedControl.svg)](https://www.nuget.org/packages/CompositionAnimatedControl)
[![NuGet](https://img.shields.io/nuget/v/ShaderAnimatedControl.svg)](https://www.nuget.org/packages/ShaderAnimatedControl)

A collection of animation controls for Avalonia including Lottie animation player and reusable base controls for custom animations.

## Controls

### Lottie Control

A lottie animation player control for Avalonia.

**Installation:**
```
dotnet add package Lottie
```

**Basic Usage:**

```xaml
<Lottie Path="/Assets/LottieLogo1.json" />
```

### AnimationControl

A reusable base class for creating custom animation controls in Avalonia. This control provides a simple animation frame loop that you can override to create custom animations.

**Installation:**
```
dotnet add package AnimationControl
```

**Basic Usage:**

```csharp
public class MyAnimationControl : AnimationControl
{
    protected override void OnAnimationFrame(TimeSpan now, TimeSpan last)
    {
        // Your animation logic here
        // The control will automatically invalidate and request the next frame
        var deltaTime = now - last;
        // Update your animation state based on deltaTime
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        // Your custom rendering logic here
    }
}
```

**Key Features:**
- Automatic animation frame loop when attached to visual tree
- Override `OnAnimationFrame(TimeSpan now, TimeSpan last)` for animation logic
- Automatic visual invalidation after each frame
- Lightweight base for simple custom animations

### CompositionAnimatedControl

A reusable base control for Skia rendering with optional animation using Avalonia's composition layer.
It is override-first: subclass it and override a small set of virtual methods.

**Installation:**
```
dotnet add package CompositionAnimatedControl
```

### Mode 1: Animated rendering (time-based or per-frame update)

You can animate by normalizing elapsed time into a loop, or by updating your state every frame and rendering using that state.

Example 1: simple time-based animation (used in demo tabs)

```csharp
public sealed class SimpleTimelineControl : CompositionAnimatedControl
{
    protected override Size OnGetSourceSize() => new Size(300, 200);
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
}
```

Example 2: bouncing ball with per-frame update and stretch-respecting drawing.

```csharp
public sealed class BouncingBallControl : CompositionAnimatedControl
{
    private const float LogicalWidth = 300f;
    private const float LogicalHeight = 200f;
    private const float Radius = 18f;
    private SKPoint _position = new(40, 40);
    private SKPoint _velocity = new(120f, 90f);

    protected override Size OnGetSourceSize() => new Size(LogicalWidth, LogicalHeight);
    protected override NormalizeResult OnNormalizeElapsed(TimeSpan ts) => new NormalizeResult(ts, false);
    protected override void OnUpdate(TimeSpan delta)
    {
        var dt = (float)delta.TotalSeconds;
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
        using var ball = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true };
        canvas.DrawCircle(cx, cy, r, ball);
    }
}

// Start/Stop animation from code-behind or view-model command
// myControl.Start();
// myControl.Stop();
```

Key virtuals:
- `OnGetSourceSize`: logical content size used for layout and stretch.
- `OnNormalizeElapsed`: returns a `NormalizeResult` which maps elapsed time to a single loop and indicates loop completion (enables repeat counting).
- `OnRender`: draw into the provided `SKCanvas` within `rect`.
- `OnUpdate`: per-frame updates driven by the composition scheduler.
- `Start()`/`Stop()` control the animation loop; `RepeatCount` sets the number of loops (use `CompositionAnimatedControl.Infinity` for endless).

Events:
- `Started`, `Stopped`, `Disposed`
- `Update(TimeSpan delta)`

### Mode 2: Static rendering with invalidation

For static content, omit `Start()` and call `Redraw()` when you need to refresh after state changes.

```csharp
public sealed class StaticRedrawControl : CompositionAnimatedControl
{
    private SKColor _color = SKColors.DarkSlateGray;
    protected override Size OnGetSourceSize() => new Size(200, 100);
    protected override NormalizeResult OnNormalizeElapsed(TimeSpan ts) => new NormalizeResult(TimeSpan.Zero, false);
    protected override void OnRender(SKCanvas canvas, Rect rect, TimeSpan _, bool __)
    {
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint { Color = _color, IsAntialias = true };
        canvas.DrawRect((float)rect.X + 20, (float)rect.Y + 20, (float)rect.Width - 40, (float)rect.Height - 40, paint);
    }
    public void Toggle()
    {
        _color = _color == SKColors.DarkSlateGray ? SKColors.OrangeRed : SKColors.DarkSlateGray;
        Redraw();
    }
}
```

Notes:
- `Redraw()` issues a single render pass without starting the animation scheduler.
- `Stretch` and `StretchDirection` are supported in both modes.

### ShaderAnimatedControl

A reusable control for rendering animated Skia shaders in Avalonia. This control loads and executes SKSL (Skia Shading Language) shaders with animation support.

**Installation:**
```
dotnet add package ShaderAnimatedControl
```

**Basic Usage:**

```xaml
<ShaderAnimatedControl 
    ShaderUri="/Assets/MyShader.sksl"
    ShaderWidth="512"
    ShaderHeight="512"
    IsShaderFillCanvas="False" />
```

```csharp
// Handle the Draw event for custom shader parameter binding
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        myShaderControl.Draw += OnShaderDraw;
    }

    private void OnShaderDraw(object? sender, DrawEventArgs e)
    {
        if (e.Effect == null || e.ErrorText != null)
            return;

        // Create shader with custom parameters
        var time = (float)e.EffectiveElapsed.TotalSeconds;
        var resolution = new SKPoint((float)e.ShaderWidth, (float)e.ShaderHeight);
        
        using var shader = e.Effect.ToShader(
            new SKRuntimeEffectUniforms(e.Effect)
            {
                ["iTime"] = time,
                ["iResolution"] = resolution
            });
        
        using var paint = new SKPaint { Shader = shader };
        e.Canvas.DrawRect(e.DestRect, paint);
    }
}
```

**Key Features:**
- Load SKSL shaders from assets or URIs
- Automatic animation loop with time-based parameters
- Configurable shader dimensions
- Fill canvas or maintain aspect ratio modes
- Draw event for custom shader parameter binding
- Built on top of CompositionAnimatedControl

**Properties:**
- `ShaderUri`: URI to the SKSL shader file
- `ShaderWidth`/`ShaderHeight`: Logical shader dimensions
- `IsShaderFillCanvas`: Whether to fill the entire canvas or maintain aspect ratio

## Links

- [Avalonia](https://avaloniaui.net/)
- [Skottie - Lottie Animation Player](https://skia.org/docs/user/modules/skottie/)
- [Skottie source code](https://skia.org/docs/user/modules/skottie/)
- [SkiaSharp.Skottie](https://www.nuget.org/packages/SkiaSharp.Skottie)

## License

Lottie is licensed under the [MIT license](LICENSE.TXT).
