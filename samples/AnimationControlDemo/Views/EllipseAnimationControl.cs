using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AnimationControlDemo.Views;

public class EllipseAnimationControl : AnimationControl
{
    private double _position;
    private double _velocity = 120;

    protected override void OnAnimationFrame(TimeSpan now, TimeSpan last)
    {
        var delta = now - last;
        _position += _velocity * delta.TotalSeconds;
        var limit = Math.Max(0, Bounds.Width - 40);
        if (_position > limit || _position < 0)
        {
            _velocity = -_velocity;
            _position = _position < 0 ? 0 : ( _position > limit ? limit : _position );
        }
    }

    public override void Render(DrawingContext context)
    {
        var center = new Point(20 + _position, Bounds.Height / 2);
        context.DrawEllipse(Brushes.DarkRed, null, center, 20, 20);
    }
}
