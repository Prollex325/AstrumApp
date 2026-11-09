using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Views;

public partial class WelcomeView : UserControl
{
    private const int FlightDurationMs = 1050;
    private Task? _introTask;

    // Для предпросмотра анимация начнётся сама.
    // Если WelcomeView находится под белым слоем StartupView, установи false
    // и вызови PlayIntroAsync() только после RevealAsync().
    public bool AutoPlayIntro { get; set; } = true;

    public event EventHandler? StartRegistrationRequested;
    public event EventHandler? SignInRequested;

    public WelcomeView()
    {
        InitializeComponent();

        // Первый кадр: только большая звезда по центру на тёмном фоне.
        BrandHeader.Opacity = 0;
        HeaderLogoPath.Opacity = 0;
        FirstLaunchLabel.Opacity = 0;
        WelcomeContent.Opacity = 0;
        DecorationHost.Opacity = 0;
        WelcomeFooter.Opacity = 0;

        StartButton.IsEnabled = false;
        SignInButton.IsEnabled = false;

        Loaded += WelcomeView_Loaded;
    }

    private async void WelcomeView_Loaded(object sender, RoutedEventArgs e)
    {
        if (AutoPlayIntro)
            await PlayIntroAsync();
    }

    /// <summary>
    /// Перемещает звезду из центра WelcomeView к логотипу в левом верхнем углу.
    /// Можно вызывать вручную после того, как исчез белый слой StartupView.
    /// Повторные вызовы не запускают вторую анимацию.
    /// </summary>
    public Task PlayIntroAsync()
    {
        if (_introTask is not null)
            return _introTask;

        // Вызови после добавления WelcomeView в визуальное дерево:
        // до первого Layout размеры ещё не известны.
        if (!IsLoaded || WelcomeRoot.ActualWidth <= 0 || HeaderLogoPath.ActualWidth <= 0)
            return Task.CompletedTask;

        _introTask = RunIntroAsync();
        return _introTask;
    }

    private async Task RunIntroAsync()
    {
        Point startCenter = new(
            WelcomeRoot.ActualWidth / 2,
            WelcomeRoot.ActualHeight / 2);

        Point targetCenter = HeaderLogoPath.TranslatePoint(
            new Point(HeaderLogoPath.ActualWidth / 2, HeaderLogoPath.ActualHeight / 2),
            WelcomeRoot);

        // FlyingStar всегда выровнена по центру контейнера,
        // даже до запуска анимации, пока экран закрыт белой вспышкой.

        double dx = targetCenter.X - startCenter.X;
        double dy = targetCenter.Y - startCenter.Y;
        double targetScale = HeaderLogoPath.ActualWidth / 74.0;

        // Плавная дуга вместо движения строго по прямой.
        // Координаты относительно центра звезды, поэтому работают при любом размере окна.
        var path = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, 0) };
        figure.Segments.Add(new BezierSegment(
            new Point(dx * 0.10, -Math.Min(90, WelcomeRoot.ActualHeight * 0.14)),
            new Point(dx * 0.82, dy * 0.48),
            new Point(dx, dy),
            true));
        path.Figures.Add(figure);

        TimeSpan flight = TimeSpan.FromMilliseconds(FlightDurationMs);
        var x = new DoubleAnimationUsingPath
        {
            PathGeometry = path,
            Source = PathAnimationSource.X,
            Duration = flight,
            AccelerationRatio = 0.10,
            DecelerationRatio = 0.28
        };
        var y = new DoubleAnimationUsingPath
        {
            PathGeometry = path,
            Source = PathAnimationSource.Y,
            Duration = flight,
            AccelerationRatio = 0.10,
            DecelerationRatio = 0.28
        };

        var completed = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        y.Completed += (_, _) => completed.TrySetResult(true);

        // Анимируем сами трансформации напрямую, без Storyboard.SetTarget.
        FlyingStarShift.BeginAnimation(TranslateTransform.XProperty, x);
        FlyingStarShift.BeginAnimation(TranslateTransform.YProperty, y);

        var scaleEase = new CubicEase { EasingMode = EasingMode.EaseInOut };
        FlyingStarScale.BeginAnimation(ScaleTransform.ScaleXProperty,
            new DoubleAnimation(1.35, targetScale, flight) { EasingFunction = scaleEase });
        FlyingStarScale.BeginAnimation(ScaleTransform.ScaleYProperty,
            new DoubleAnimation(1.35, targetScale, flight)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
        FlyingStarRotation.BeginAnimation(RotateTransform.AngleProperty,
            new DoubleAnimation(-12, 0, flight)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });

        // Элементы интерфейса начинают появляться по мере полёта звезды.
        FadeIn(WelcomeContent, delayMs: 280, durationMs: 520);
        FadeIn(DecorationHost, delayMs: 450, durationMs: 580);
        FadeIn(FirstLaunchLabel, delayMs: 650, durationMs: 320);
        FadeIn(WelcomeFooter, delayMs: 660, durationMs: 330);
        FadeIn(BrandHeader, delayMs: 760, durationMs: 290);

        await completed.Task;

        // Незаметно подменяем летящую звезду постоянным логотипом в шапке.
        HeaderLogoPath.Opacity = 1;
        FlyingStar.Visibility = Visibility.Collapsed;

        StartButton.IsEnabled = true;
        SignInButton.IsEnabled = true;
    }

    private static void FadeIn(UIElement element, int delayMs, int durationMs)
    {
        element.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
    }

    private void LogoUpGo()
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = -70,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
        };
        BrandHeaderShift.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    private void WelcomeRootOpacity()
    {
        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        animation.Completed += (_, _) => { if (App.Current.MainWindow is MainWindow window) window.CloseWelcomeView(); };

        WelcomeRoot.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        LogoUpGo();
        WelcomeRootOpacity();
        StartRegistrationRequested?.Invoke(this, EventArgs.Empty);
    }

    private void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        LogoUpGo();
        WelcomeRootOpacity();
        SignInRequested?.Invoke(this, EventArgs.Empty);
    }
}
