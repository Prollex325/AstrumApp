using AstrumApp.AuthManager;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Views;

public enum StartupStep
{
    Supabase,
    Session,
    Profile,
    Interface
}

public enum StartupStepStatus
{
    Pending,
    Active,
    Completed,
    Skipped,
    Failed
}

public sealed class StartupStepItem : INotifyPropertyChanged
{
    public StartupStep Id { get; }
    public string Title { get; }

    private StartupStepStatus _status = StartupStepStatus.Pending;

    public StartupStepStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText => Status switch
    {
        StartupStepStatus.Pending => "Ожидание",
        StartupStepStatus.Active => "Загрузка",
        StartupStepStatus.Completed => "Готово",
        StartupStepStatus.Skipped => "Пропущено",
        StartupStepStatus.Failed => "Ошибка",
        _ => ""
    };

    public StartupStepItem(StartupStep id, string title)
    {
        Id = id;
        Title = title;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class StartupView : UserControl
{
    private const double CycleSeconds = 2.5;
    private const double TravelStart = 0.16;
    private const double TravelEnd = 1.63;

    // Длительности финального перехода. Менять удобно здесь, а не в Storyboard.
    private const int SpiralFlightDurationMs = 1800; // Длительность полёта звезды
    private const int StarIgnitionDurationMs = 800; // Скорость разрастания звезды  980
    private const int RevealDurationMs = 900; // Скорость исчезновения белого экрана  500

    private readonly ObservableCollection<StartupStepItem> _steps =
    [
        new(StartupStep.Supabase, "Подключение к Supabase"),
        new(StartupStep.Session, "Проверка сессии"),
        new(StartupStep.Profile, "Загрузка профиля"),
        new(StartupStep.Interface, "Подготовка интерфейса")
    ];

    private readonly TextBlock[] _letters;
    private Storyboard? _loadingStoryboard;
    private bool _ready;
    private bool _outroStarted;
    private bool _flashCovered;

    public StartupView()
    {
        InitializeComponent();
        StepsList.ItemsSource = _steps;
        _letters = [LetterA, LetterS, LetterT, LetterR, LetterU, LetterM];

        Loaded += StartupView_Loaded;
        Unloaded += StartupView_Unloaded;
    }

    private void StartupView_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_ready && _loadingStoryboard is null)
            StartLoadingAnimation();
    }

    private void StartupView_Unloaded(object sender, RoutedEventArgs e)
    {
        StopLoadingAnimation();
    }

    // Эти методы вызывает внешний координатор запуска, а не сам StartupView.
    public void BeginStep(StartupStep step) => SetStepStatus(step, StartupStepStatus.Active);
    public void CompleteStep(StartupStep step) => SetStepStatus(step, StartupStepStatus.Completed);
    public void SkipStep(StartupStep step) => SetStepStatus(step, StartupStepStatus.Skipped);
    public void FailStep(StartupStep step) => SetStepStatus(step, StartupStepStatus.Failed);

    private void SetStepStatus(StartupStep step, StartupStepStatus status)
    {
        // Методы представления вызываются в UI-потоке WPF.
        _steps.First(item => item.Id == step).Status = status;
    }

    public void ShowReady()
    {
        if (_ready) return;
        _ready = true;
        StopLoadingAnimation();
        Particle.Opacity = 0;
        StatusCaption.Text = "Инициализация завершена";
        ReadyNotice.Visibility = Visibility.Visible;
        ReadyNotice.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1,
            TimeSpan.FromMilliseconds(230)));
    }

    // Этап 1 финального перехода: звезда проходит по спирали и экран покрывается белым.
    // Следующую страницу НЕ переключаем здесь: это ответственность координатора.
    public async Task CoverWithFlashAsync()
    {
        if (!_ready)
            throw new InvalidOperationException("Сначала вызови ShowReady().");
        if (_outroStarted)
            throw new InvalidOperationException("Анимация завершения уже запущена.");

        _outroStarted = true;

        // Буквы гаснут параллельно с полётом. Само зажигание начинается,
        // когда логотип действительно долетел до центра окна.

        var rotation = new DoubleAnimation
        {
            From = 0,
            To = 180, // Два полных оборота
            Duration = TimeSpan.FromMilliseconds(SpiralFlightDurationMs),

            FillBehavior = FillBehavior.HoldEnd
        };

        StarRotation.BeginAnimation(
            RotateTransform.AngleProperty,
            rotation
        );

        await Task.WhenAll(
            FlyLogoAlongSpiralAsync(),
            AnimateOpacityAsync(LoadingContent, 0, 190));

        await IgniteStarAsync();

        // Звезда теперь перекрыта полностью белым FlashOverlay.
        // Координатор может безопасно подготовить под ним AuthView или MainView.
        StarIgnitionLayer.Visibility = Visibility.Collapsed;
        SceneRoot.Visibility = Visibility.Collapsed;
        _flashCovered = true;
    }

    // Этап 2: координатор уже подготовил нужный экран ПОД StartupView.
    public async Task RevealAsync()
    {
        if (!_flashCovered)
            throw new InvalidOperationException("Сначала дождись CoverWithFlashAsync().");

        await AnimateOpacityAsync(FlashOverlay, 0, RevealDurationMs);
        FlashOverlay.Visibility = Visibility.Collapsed;
    }

    // Упрощённый режим персонализации: без спирали и вспышки.
    // Сначала координатор показывает целевой экран ПОД StartupView.
    public async Task FadeOutAsync()
    {
        StopLoadingAnimation();
        await AnimateOpacityAsync(SceneRoot, 0, 300);
    }

    private async Task FlyLogoAlongSpiralAsync()
    {
        // Стартовая точка — реальный центр звезды в координатах SceneRoot.
        // Поэтому маршрут не зависит от фиксированного размера окна.
        Point start = CornerLogoHost.TranslatePoint(
            new Point(CornerLogoHost.ActualWidth / 2, CornerLogoHost.ActualHeight / 2),
            SceneRoot);

        double dx = SceneRoot.ActualWidth / 2 - start.X;
        double dy = SceneRoot.ActualHeight / 2 - start.Y;

        // Три кривые Безье: большой заход, закручивание, вход в центр.
        // Это стилизация под золотую спираль, а не математически точная спираль.
        Point p0 = new(0, 0);
        Point p1 = new(dx * .82, dy * 1.14);
        Point p2 = new(dx * 1.04, dy * .56);
        Point p3 = new(dx, dy);

        var spiral = new (Point From, Point C1, Point C2, Point To)[]
        {
            (p0, new(dx * .09, dy * .65), new(dx * .36, dy * 1.12), p1),
            (p1, new(dx * 1.35, dy * 1.12), new(dx * 1.30, dy * .54), p2),
            (p2, new(dx * .79, dy * .51), new(dx * .81, dy * .93), p3)
        };

        const int samples = 72;
        const int durationMs = SpiralFlightDurationMs;
        var x = new DoubleAnimationUsingKeyFrames();
        var y = new DoubleAnimationUsingKeyFrames();

        for (int i = 0; i <= samples; i++)
        {
            double progress = (double)i / samples;
            double eased = progress * progress * (3 - 2 * progress); // smoothstep

            Point position;
            if (eased <= .48)
                position = Bezier(spiral[0], eased / .48);
            else if (eased <= .79)
                position = Bezier(spiral[1], (eased - .48) / .31);
            else
                position = Bezier(spiral[2], (eased - .79) / .21);

            KeyTime time = KeyTime.FromTimeSpan(
                TimeSpan.FromMilliseconds(durationMs * progress));
            x.KeyFrames.Add(new LinearDoubleKeyFrame(position.X, time));
            y.KeyFrames.Add(new LinearDoubleKeyFrame(position.Y, time));
        }

        var scaleX = new DoubleAnimationUsingKeyFrames();
        scaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1, At(0)));
        scaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1.05,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.67))));
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame(3.2,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });

        var scaleY = new DoubleAnimationUsingKeyFrames();
        scaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1, At(0)));
        scaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1.05,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.67))));
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame(3.2,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });

        var storyboard = new Storyboard
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs))
        };
        Storyboard.SetTarget(x, CornerLogoHost);
        Storyboard.SetTargetProperty(x, new PropertyPath(
            "(UIElement.RenderTransform).(TranslateTransform.X)"));
        Storyboard.SetTarget(y, CornerLogoHost);
        Storyboard.SetTargetProperty(y, new PropertyPath(
            "(UIElement.RenderTransform).(TranslateTransform.Y)"));
        Storyboard.SetTarget(scaleX, LogoArtwork);
        Storyboard.SetTargetProperty(scaleX, new PropertyPath(
            "(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
        Storyboard.SetTarget(scaleY, LogoArtwork);
        Storyboard.SetTargetProperty(scaleY, new PropertyPath(
            "(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

        storyboard.Children.Add(x);
        storyboard.Children.Add(y);
        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        storyboard.Completed += (_, _) => completion.TrySetResult();

        storyboard.Begin(this, isControllable: true);
        await completion.Task;
    }

    // Эффект «зажигания звезды»: несколько мягких градиентов растут
    // из одной точки, лучи кратко вспыхивают, и только в финале
    // весь экран становится непрозрачно-белым.
    private Task IgniteStarAsync()
    {
        StarIgnitionLayer.Visibility = Visibility.Visible;
        FlashOverlay.Opacity = 0;
        FlashOverlay.Visibility = Visibility.Visible;

        const int t = StarIgnitionDurationMs;
        var storyboard = new Storyboard
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(t)),
            FillBehavior = FillBehavior.HoldEnd
        };

        // Внешнее свечение раскрывается чуть раньше внутреннего.
        AddGlowScale(storyboard, StarOuterHalo,
            (0, .12), (220, .72), (t, 5.8));
        AddGlowOpacity(storyboard, StarOuterHalo,
            (0, 0), (140, .85), (480, 1), (t, 1));

        // Белое ядро заполняет центр, голубая периферия остаётся мягкой.
        AddGlowScale(storyboard, StarInnerGlow,
            (0, .08), (240, .78), (t, 6.5));
        AddGlowOpacity(storyboard, StarInnerGlow,
            (0, 0), (160, 1), (t, 1));

        // Быстрая искра поверх логотипа.
        AddGlowScale(storyboard, StarCore,
            (0, .1), (180, 1.0), (t, 15));
        AddGlowOpacity(storyboard, StarCore,
            (0, 0), (115, 1), (t, 1));

        // Перекрестные лучи вспыхивают и рассеиваются.
        AddGlowOpacity(storyboard, StarRayA,
            (0, 0), (130, .95), (420, .68), (660, 0), (t, 0));
        AddGlowOpacity(storyboard, StarRayB,
            (0, 0), (160, .75), (420, .50), (670, 0), (t, 0));

        // Звезда исчезает, словно растворяясь в ярком свете.
        AddGlowOpacity(storyboard, LogoArtwork,
            (0, 1), (130, 1), (350, 0), (t, 0));

        // Полная белизна включается лишь в конце расширения светового пятна.
        // Это позволяет затем сменить содержимое под StartupView без скачка.
        AddGlowOpacity(storyboard, FlashOverlay,
            (0, 0), (570, 0), (770, .28), (t, 1));

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        storyboard.Completed += (_, _) =>
        {
            // Сначала фиксируем базовое значение Opacity=1, потом снимаем
            // анимационные значения Storyboard. Иначе RevealAsync() мог бы
            // начать исчезновение с базового нуля и резко обрубить белый экран.
            FlashOverlay.Opacity = 1;
            storyboard.Remove(this);
            completion.TrySetResult();
        };

        storyboard.Begin(this, isControllable: true);
        return completion.Task;
    }

    // Вспомогательные методы создают ключевые кадры сразу для X и Y.
    private static void AddGlowScale(Storyboard storyboard, FrameworkElement element,
        params (int Ms, double Scale)[] keys)
    {
        foreach (string axis in new[] { "ScaleX", "ScaleY" })
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            foreach (var (ms, scale) in keys)
            {
                animation.KeyFrames.Add(new EasingDoubleKeyFrame(scale,
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ms)))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                });
            }

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation,
                new PropertyPath($"(UIElement.RenderTransform).(ScaleTransform.{axis})"));
            storyboard.Children.Add(animation);
        }
    }

    private static void AddGlowOpacity(Storyboard storyboard, UIElement element,
        params (int Ms, double Opacity)[] keys)
    {
        var animation = new DoubleAnimationUsingKeyFrames();
        foreach (var (ms, opacity) in keys)
        {
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(opacity,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ms)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
        }

        Storyboard.SetTarget(animation, element);
        Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(animation);
    }

    private static Point Bezier((Point From, Point C1, Point C2, Point To) part, double t)
    {
        double u = 1 - t;
        double a = u * u * u;
        double b = 3 * u * u * t;
        double c = 3 * u * t * t;
        double d = t * t * t;
        return new Point(
            a * part.From.X + b * part.C1.X + c * part.C2.X + d * part.To.X,
            a * part.From.Y + b * part.C1.Y + c * part.C2.Y + d * part.To.Y);
    }

    private static Task AnimateOpacityAsync(UIElement element, double to, int milliseconds)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var animation = new DoubleAnimation(element.Opacity, to,
            TimeSpan.FromMilliseconds(milliseconds))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
            FillBehavior = FillBehavior.Stop
        };

        animation.Completed += (_, _) =>
        {
            element.BeginAnimation(UIElement.OpacityProperty, null);
            element.Opacity = to;
            completion.TrySetResult();
        };
        element.BeginAnimation(UIElement.OpacityProperty, animation);
        return completion.Task;
    }

    private void StartLoadingAnimation()
    {
        // Вычисляем центры букв в фактических WPF-координатах.
        // Поэтому кружок следует под буквами и при другом размере шрифта.
        var centers = _letters.Select(letter =>
            letter.TranslatePoint(new Point(letter.ActualWidth / 2, 0), TitleLetters).X
            - Particle.Width / 2).ToArray();

        if (centers.Length == 0 || TitleLetters.ActualWidth <= 0)
            return;

        double first = centers[0];
        double last = centers[^1];
        double distance = Math.Max(1, last - first);

        var storyboard = new Storyboard
        {
            Duration = new Duration(TimeSpan.FromSeconds(CycleSeconds)),
            RepeatBehavior = RepeatBehavior.Forever
        };

        // Кружок проходит под словом, гаснет и незаметно возвращается к началу.
        var move = new DoubleAnimationUsingKeyFrames();
        move.KeyFrames.Add(new DiscreteDoubleKeyFrame(first, At(0)));
        move.KeyFrames.Add(new DiscreteDoubleKeyFrame(first, At(TravelStart)));
        move.KeyFrames.Add(new LinearDoubleKeyFrame(last, At(TravelEnd)));
        move.KeyFrames.Add(new DiscreteDoubleKeyFrame(last, At(1.95)));
        move.KeyFrames.Add(new DiscreteDoubleKeyFrame(first, At(2.0)));
        move.KeyFrames.Add(new DiscreteDoubleKeyFrame(first, At(CycleSeconds)));
        Storyboard.SetTarget(move, Particle);
        Storyboard.SetTargetProperty(move,
            new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
        storyboard.Children.Add(move);

        var dotOpacity = new DoubleAnimationUsingKeyFrames();
        dotOpacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, At(0)));
        dotOpacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, At(1.70)));
        dotOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, At(1.91)));
        dotOpacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, At(2.06)));
        dotOpacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, At(2.26)));
        dotOpacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, At(CycleSeconds)));
        Storyboard.SetTarget(dotOpacity, Particle);
        Storyboard.SetTargetProperty(dotOpacity, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(dotOpacity);

        // Буква подпрыгивает именно в момент прохождения кружка под ней.
        for (int i = 0; i < _letters.Length; i++)
        {
            double hit = TravelStart + (centers[i] - first) / distance
                * (TravelEnd - TravelStart);

            var jump = new DoubleAnimationUsingKeyFrames();
            jump.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, At(0)));
            jump.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, At(Math.Max(0, hit - 0.06))));
            jump.KeyFrames.Add(new EasingDoubleKeyFrame(-11, At(hit + 0.10))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            jump.KeyFrames.Add(new EasingDoubleKeyFrame(0, At(hit + 0.29))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            jump.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, At(CycleSeconds)));

            Storyboard.SetTarget(jump, _letters[i]);
            Storyboard.SetTargetProperty(jump,
                new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(jump);
        }

        _loadingStoryboard = storyboard;
        storyboard.Begin(this, isControllable: true);
    }


    public async Task RevealIntoStarAsync()
    {
        double width = FlashOverlay.ActualWidth;
        double height = FlashOverlay.ActualHeight;

        if (width <= 0 || height <= 0)
            return;

        // Радиус, достаточный для покрытия всего экрана
        double diagonal = Math.Sqrt(width * width + height * height);
        double radius = diagonal / 2 / 0.78 * 1.05;

        // Белая середина с мягкими прозрачными краями
        var mask = new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            GradientOrigin = new Point(0.5, 0.5),

            RadiusX = radius / width,
            RadiusY = radius / height,

            GradientStops = new GradientStopCollection
        {
            new GradientStop(Colors.White, 0),
            new GradientStop(Colors.White, 0.78),
            new GradientStop(Color.FromArgb(0, 255, 255, 255), 1)
        }
        };

        FlashOverlay.Opacity = 1;
        FlashOverlay.Visibility = Visibility.Visible;
        FlashOverlay.OpacityMask = mask;

        const int durationMs = 850;

        // Сжимаем белый свет к центру экрана
        await Task.WhenAll(
            AnimateRadiusAsync(
                mask,
                RadialGradientBrush.RadiusXProperty,
                mask.RadiusX,
                0.001,
                durationMs
            ),
            AnimateRadiusAsync(
                mask,
                RadialGradientBrush.RadiusYProperty,
                mask.RadiusY,
                0.001,
                durationMs
            )
        );

        FlashOverlay.Visibility = Visibility.Collapsed;
        FlashOverlay.OpacityMask = null;
    }


    private static Task AnimateRadiusAsync(
        RadialGradientBrush brush,
        DependencyProperty property,
        double from,
        double to,
        int milliseconds)
    {
        var completion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(milliseconds),
            EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseIn
            },
            FillBehavior = FillBehavior.Stop
        };

        animation.Completed += (_, _) =>
        {
            brush.BeginAnimation(property, null);
            brush.SetValue(property, to);
            completion.TrySetResult();
        };

        brush.BeginAnimation(property, animation);

        return completion.Task;
    }



    private void StopLoadingAnimation()
    {
        if (_loadingStoryboard is null) return;
        _loadingStoryboard.Stop(this);
        _loadingStoryboard = null;
    }

    private static KeyTime At(double seconds) =>
        KeyTime.FromTimeSpan(TimeSpan.FromSeconds(seconds));
}
