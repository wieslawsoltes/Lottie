using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CompositionAnimatedControlDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnToggle(object? sender, RoutedEventArgs e)
    {
        if (this.FindControl<StaticRedrawControl>("StaticControl") is { } staticControl)
        {
            staticControl.Toggle();
        }
    }

    private void OnGravityChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(Slider.Value)) return;
        if (this.FindControl<PhysicsCollisionsControl>("PhysicsControl") is { } physics &&
            this.FindControl<Slider>("GravitySlider") is { } slider)
        {
            physics.SetGravity((float)slider.Value);
        }
    }

    private void OnRestart(object? sender, RoutedEventArgs e)
    {
        if (this.FindControl<PhysicsCollisionsControl>("PhysicsControl") is { } physics)
        {
            physics.Restart();
        }
    }
}


