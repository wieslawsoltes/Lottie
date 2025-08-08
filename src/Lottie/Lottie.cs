using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Skia.Composition;
using SkiaSharp;

namespace Avalonia.Skia.Lottie;

/// <summary>
/// Lottie animation player control.
/// </summary>
public class Lottie : CompositionAnimatedControl
{
    private SkiaSharp.Skottie.Animation? _animation;
    private readonly Uri _baseUri;
    private string? _preloadPath;

    /// <summary>
    /// Infinite number of repeats.
    /// </summary>
    public new const int Infinity = -1;

    /// <summary>
    /// Defines the <see cref="Path"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> PathProperty =
        AvaloniaProperty.Register<Lottie, string?>(nameof(Path));

    public static readonly DirectProperty<Lottie, double> PositionSecondsProperty =
        AvaloniaProperty.RegisterDirect<Lottie, double>(
            nameof(PositionSeconds),
            o => o.PositionSeconds,
            (o, v) => o.PositionSeconds = v);

    public static readonly DirectProperty<Lottie, double> DurationSecondsProperty =
        AvaloniaProperty.RegisterDirect<Lottie, double>(
            nameof(DurationSeconds),
            o => o.DurationSeconds);

    /// <summary>
    /// Gets or sets the Lottie animation path.
    /// </summary>
    [Content]
    public string? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public double PositionSeconds
    {
        get => Position.TotalSeconds;
        set => SeekSeconds(value);
    }

    public double DurationSeconds => _animation?.Duration.TotalSeconds ?? 0.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Lottie"/> class.
    /// </summary>
    /// <param name="baseUri">The base URL for the XAML context.</param>
    public Lottie(Uri baseUri)
    {
        _baseUri = baseUri;
        WireHandlers();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Lottie"/> class.
    /// </summary>
    /// <param name="serviceProvider">The XAML service provider.</param>
    public Lottie(IServiceProvider serviceProvider)
    {
        _baseUri = serviceProvider.GetContextBaseUri();
        WireHandlers();
    }

    private void WireHandlers() { }

        /// <summary>
        /// Starts or resumes playback.
        /// </summary>
        public void Play() => Start();

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public void Pause() => base.Pause();

        /// <summary>
        /// Seek to a specific absolute time position.
        /// </summary>
        public void Seek(TimeSpan position) => base.Seek(position);

        /// <summary>
        /// Seek to a specific position given in seconds.
        /// </summary>
        public void SeekSeconds(double seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }
            base.Seek(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Seek by normalized progress in range [0, 1].
        /// </summary>
        public void SeekProgress(double progress)
        {
            if (_animation is null)
            {
                return;
            }
            if (progress < 0) progress = 0;
            if (progress > 1) progress = 1;
            var target = TimeSpan.FromTicks((long)(_animation.Duration.Ticks * progress));
            base.Seek(target);
        }

    protected override Size OnGetSourceSize()
        => _animation is { } an ? new Size(an.Size.Width, an.Size.Height) : default;

    protected override NormalizeResult OnNormalizeElapsed(TimeSpan elapsed)
    {
        if (_animation is null)
            return new NormalizeResult(TimeSpan.Zero, false);
        var duration = _animation.Duration;
        if (duration <= TimeSpan.Zero)
            return new NormalizeResult(elapsed, false);
        if (elapsed < duration)
            return new NormalizeResult(elapsed, false);
        var loops = (int)(elapsed.Ticks / duration.Ticks);
        var remainder = TimeSpan.FromTicks(elapsed.Ticks % duration.Ticks);
        return new NormalizeResult(remainder, loops > 0);
    }

    protected override void OnRender(SKCanvas canvas, Rect destRect, TimeSpan effectiveElapsed, bool isRunning)
    {
        var animation = _animation;
        if (animation is null)
        {
            return;
        }
        var t = effectiveElapsed;
        var d = animation.Duration;
        if (d > TimeSpan.Zero && t >= d)
        {
            t = d - TimeSpan.FromTicks(1);
        }
        animation.SeekFrameTime(t.TotalSeconds);
        var dst = new SKRect(
            (float)destRect.X,
            (float)destRect.Y,
            (float)destRect.Right,
            (float)destRect.Bottom);
        canvas.Save();
        animation.Render(canvas, dst);
        canvas.Restore();
    }

    /// <inheritdoc/>
    protected override void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        base.OnLoaded(routedEventArgs);

        if (_preloadPath is null)
        {
            return;
        }

        Load(_preloadPath);
        Start();
        _preloadPath = null;
    }

    protected override void OnUnloaded(RoutedEventArgs routedEventArgs)
    {
        base.OnUnloaded(routedEventArgs);
        Stop();
        DisposeImpl();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(Position):
            {
                var oldS = change.GetOldValue<TimeSpan>().TotalSeconds;
                var newS = change.GetNewValue<TimeSpan>().TotalSeconds;
                RaisePropertyChanged(PositionSecondsProperty, oldS, newS);
                break;
            }
            case nameof(Path):
            {
                var path = change.GetNewValue<string?>();

                if (_preloadPath is null && _animation is null && VisualRoot == null)
                {
                    _preloadPath = path;
                    return;
                }

                Load(path);
                break;
            }
        }
    }

    private SkiaSharp.Skottie.Animation? Load(Stream stream)
    {
        using var managedStream = new SKManagedStream(stream);
        if (SkiaSharp.Skottie.Animation.TryCreate(managedStream, out var animation))
        {
            animation.Seek(0);

            Logger
                .TryGet(LogEventLevel.Information, LogArea.Control)?
                .Log(this, $"Version: {animation.Version} Duration: {animation.Duration} Fps:{animation.Fps} InPoint: {animation.InPoint} OutPoint: {animation.OutPoint}");
        }
        else
        {
            Logger
                .TryGet(LogEventLevel.Warning, LogArea.Control)?
                .Log(this, "Failed to load animation.");
        }

        return animation;
    }

    private SkiaSharp.Skottie.Animation? Load(string path, Uri? baseUri)
    {
        var uri = path.StartsWith("/")
            ? new Uri(path, UriKind.Relative)
            : new Uri(path, UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri && uri.IsFile)
        {
            using var fileStream = File.OpenRead(uri.LocalPath);
            return Load(fileStream);
        }

        using var assetStream = AssetLoader.Open(uri, baseUri);

        if (assetStream is null)
        {
            return default;
        }

        return Load(assetStream);
    }

    private void Load(string? path)
    {
        Stop();

        if (path is null)
        {
            DisposeImpl();
            return;
        }

        DisposeImpl();

        try
        {
            var oldDurationSeconds = DurationSeconds;
            _animation = Load(path, _baseUri);

            if (_animation is null)
            {
                return;
            }

            InvalidateArrange();
            InvalidateMeasure();

            RaisePropertyChanged(DurationSecondsProperty, oldDurationSeconds, DurationSeconds);
            Start();
        }
        catch (Exception e)
        {
            Logger
                .TryGet(LogEventLevel.Warning, LogArea.Control)?
                .Log(this, "Failed to load animation: " + e);
            _animation = null;
        }
    }

    private void DisposeImpl()
    {
        _animation?.Dispose();
        _animation = null;
    }
}
