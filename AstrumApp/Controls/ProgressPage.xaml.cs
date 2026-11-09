using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AstrumApp.Controls
{
    public partial class ProgressPage : UserControl
    {
        private bool _finished;

        public bool IsSuccessful { get; private set; }

        public event Action<bool>? IsSuccessfulResult;

        public ProgressPage()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                PlayAppearance();
                if (!_finished)
                    StartLoadingAnimation();
            };
        }

        private void PlayAppearance()
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            WindowBody.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(350))
                { EasingFunction = ease });

            AppearScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(350))
                { EasingFunction = ease });

            AppearScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(350))
                { EasingFunction = ease });
        }

        private void StartLoadingAnimation()
        {
            OuterRotation.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(0, 360, TimeSpan.FromSeconds(7))
                {
                    RepeatBehavior = RepeatBehavior.Forever
                    // Без EasingFunction: равномерная скорость вращения
                });

            InnerRotation.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(0, -360, TimeSpan.FromSeconds(12))
                {
                    RepeatBehavior = RepeatBehavior.Forever
                });

            var pulse = new DoubleAnimation(0.30, 0.72, TimeSpan.FromSeconds(1.8))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            LoadingHalo.BeginAnimation(OpacityProperty, pulse);
            StatusDot.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0.35, 1, TimeSpan.FromSeconds(0.7))
                {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                });
        }

        private void StopLoadingAnimation()
        {
            OuterRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            InnerRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            LoadingHalo.BeginAnimation(OpacityProperty, null);
            StatusDot.BeginAnimation(OpacityProperty, null);
        }

        // Можно вызывать между async-этапами регистрации, меняя подпись.
        public void SetStatus(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetStatus(message));
                return;
            }

            if (!_finished)
                StatusText.Text = message;
        }

        public void ShowSuccess(string message = "Профиль создан. Всё готово к работе!")
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ShowSuccess(message));
                return;
            }

            if (_finished) return;
            _finished = true;
            IsSuccessful = true;

            TitleText.Text = "Добро пожаловать!";
            MessageText.Text = message;
            ContinueButton.Content = "Продолжить  →";
            ShowResult(false);
        }

        public void ShowError(string message = "Не удалось создать аккаунт. Попробуйте ещё раз.")
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ShowError(message));
                return;
            }

            if (_finished) return;
            _finished = true;
            IsSuccessful = false;

            TitleText.Text = "Что-то пошло не так";
            MessageText.Text = message;
            ContinueButton.Content = "Закрыть";
            ShowResult(true);
        }

        private void ShowResult(bool isError)
        {
            StopLoadingAnimation();

            LoadingVisual.Visibility = Visibility.Collapsed;
            LoadingStatus.Visibility = Visibility.Collapsed;
            WaitingNote.Visibility = Visibility.Collapsed;
            ResultVisual.Visibility = Visibility.Visible;

            if (isError)
            {
                ResultHalo.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#587C333C"));
                ResultCircle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B252D"));
                ResultCircle.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BD6979"));
                ResultIcon.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC0C9"));
                ResultIcon.Data = Geometry.Parse("M 10,10 L 44,44 M 44,10 L 10,44");
            }

            // Мягкое появление итоговой иконки
            var ease = new BackEase
            {
                EasingMode = EasingMode.EaseOut,
                Amplitude = 0.28
            };

            ResultScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0.75, 1, TimeSpan.FromMilliseconds(520))
                { EasingFunction = ease });
            ResultScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                new DoubleAnimation(0.75, 1, TimeSpan.FromMilliseconds(520))
                { EasingFunction = ease });

            ContinueButton.Visibility = Visibility.Visible;
            ContinueButton.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(450))
                { BeginTime = TimeSpan.FromMilliseconds(190) });
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            IsSuccessfulResult?.Invoke(IsSuccessful);
        }    
    }
}
