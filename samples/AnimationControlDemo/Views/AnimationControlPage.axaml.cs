using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AnimationControlDemo.Views;

public partial class AnimationControlPage : UserControl
{
    public AnimationControlPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
