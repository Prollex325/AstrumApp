using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;
using System.Windows.Media.Animation;
using AstrumApp.Services;
using System.Diagnostics;

namespace AstrumApp.Controls
{
    public partial class LockScreen : UserControl
    {
        DispatcherTimer clock = new DispatcherTimer();
        private Point _startMouse;
        private double _startY;
        private bool _drag;
        private bool firstLoad = true;
        private int _prevLength = 0;
        private int _curLength = 0;

        public LockScreen()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                this.Focus(); // даём фокус UserControl
            };

            clock.Tick += Clock_Tick;
            clock.Interval = TimeSpan.FromSeconds(1);
            clock.Start();

            DataContext = App.Session.Profile;
        }

        // не трогаем
        private void WallPaper_MediaOpened(object sender, RoutedEventArgs e)
        {
            //WallPaperVideo.Volume = 0;

            Clock.Visibility = Visibility.Visible;
            Date.Visibility = Visibility.Visible;
        }
        private void WallPaper_MediaEnded(object sender, RoutedEventArgs e)
        {
            //WallPaperVideo.Position = TimeSpan.Zero;
            //WallPaperVideo.Play();
        }
        private void Main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _drag = true;
            _startMouse = e.GetPosition(null);
            _startY = ScreenTransform.Y;

            ScreenTransform.BeginAnimation(TranslateTransform.YProperty, null);
            ImageBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, null);

            Mouse.Capture((UIElement)sender);
        }
        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_drag) return;

            var pos = e.GetPosition(null);
            double delta = pos.Y - _startMouse.Y;

            double newY = _startY + delta;

            if (newY > 0)
                newY = 0;

            ScreenTransform.Y = newY;

            double progress = -newY / 300;
            ImageBlurEffect.Radius = Math.Clamp(progress * 20, 0, 20);
        }
        private void Main_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _drag = false;
            Mouse.Capture(null);
            int move_to = 0;

            if (ScreenTransform.Y < -200)
            {
                //ShowGreeting();
                _ = ShowGreetingTitle();
                move_to = -300;
            }
            if (move_to == 0)
            {
                DoubleAnimation blur_anim = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };
                ImageBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, blur_anim);
            }
            DoubleAnimation move_anim = new DoubleAnimation
            {
                To = move_to,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            ScreenTransform.BeginAnimation(TranslateTransform.YProperty, move_anim);
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            DoubleAnimation move_anim = new DoubleAnimation
            {
                To = -300,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            DoubleAnimation blur_anim = new DoubleAnimation
            {
                To = 20,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            ScreenTransform.BeginAnimation(TranslateTransform.YProperty, move_anim);
            ImageBlurEffect.BeginAnimation(BlurEffect.RadiusProperty, blur_anim);

            _ = ShowGreetingTitle();
        }
        private void Clock_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            Clock.Text = now.ToString("HH:mm:ss");
            if (now.Second == 0 || firstLoad)
            {
                Date.Text = now.ToString("dddd, dd MMMM");
                firstLoad = false;
            }
        }

        private void Password_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void PasswordInput_Changed(object sender, RoutedEventArgs e)
        {
            _curLength = PasswordInput.Password.Length;
            UpdateUIDots(_curLength > _prevLength);
            _prevLength = _curLength;

            if (PasswordInput.Password.Length == 4) {
                if (App.Security.Pin.Verify(PasswordInput.Password))
                {
                    _ = LiftLockScreenAsync(50);
                } else
                {
                    _ = IncorrectPassword();
                }
            }
        }

        private async Task ShowGreetingTitle()
        {
            if (!App.Security.Pin.HasPin())
            {
                _ = LiftLockScreenAsync(0);
                return;
            }
            DoubleAnimation opacity_anim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            await Task.Delay(150);

            GreetingBox.BeginAnimation(OpacityProperty, opacity_anim);

            clock.Stop();
            PasswordInput.Focus();
        }

        private async Task LiftLockScreenAsync(int delay)
        {
            await Task.Delay(delay);

            App.LiftLockScreen();
        }

        private async Task IncorrectPassword()
        {
            ShakePin();

            Dot1.Fill = Brushes.Red;
            Dot2.Fill = Brushes.Red;
            Dot3.Fill = Brushes.Red;
            Dot4.Fill = Brushes.Red;

            await Task.Delay(150);

            PasswordInput.Password = String.Empty;
            Dot1.Fill = Brushes.Gray;
            Dot2.Fill = Brushes.Gray;
            Dot3.Fill = Brushes.Gray;
            Dot4.Fill = Brushes.Gray;
        }

        private void UpdateUIDots(bool isCharAdded) // isCharAdded - добавлена цифра или удалена для решения какая анимация проиграется
        {
            int length = PasswordInput.Password.Length;
            Color targetColor = isCharAdded ? Colors.White : Colors.Gray;
            Color curColor = isCharAdded ? Colors.Gray : Colors.White;

            if (isCharAdded)
            {
                if (length == 1) DotAnimation(Dot1, targetColor, curColor);
                if (length == 2) DotAnimation(Dot2, targetColor, curColor);
                if (length == 3) DotAnimation(Dot3, targetColor, curColor);
                if (length == 4) DotAnimation(Dot4, targetColor, curColor);
            }
            else
            {
                if (length == 0) DotAnimation(Dot1, targetColor, curColor);
                if (length == 1) DotAnimation(Dot2, targetColor, curColor);
                if (length == 2) DotAnimation(Dot3, targetColor, curColor);
                if (length == 3) DotAnimation(Dot4, targetColor, curColor);
            }
        }

        private void DotAnimation(object obj, Color targetColor, Color curColor)
        {
            if (obj is Ellipse dot)
            {
                var brush = new SolidColorBrush(curColor);
                dot.Fill = brush;

                var fillAnim = new ColorAnimation
                {
                    To = targetColor,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };

                brush.BeginAnimation(SolidColorBrush.ColorProperty, fillAnim);
            }
        }

        private void ShakePin()
        {
            var animation = new DoubleAnimationUsingKeyFrames();

            animation.KeyFrames.Add(
                new LinearDoubleKeyFrame(-8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));

            animation.KeyFrames.Add(
                new LinearDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(50))));

            animation.KeyFrames.Add(
                new LinearDoubleKeyFrame(-4, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));

            animation.KeyFrames.Add(
                new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(140))));

            PinDotsTransform.BeginAnimation(
                TranslateTransform.XProperty,
                animation);
        }

        // до сюда

        /*
        private void ShowGreeting_Delay(object? sender, EventArgs e)
        {
            delayTimer.Stop();

            if (App.Security.Pin.HasPin())
            {
                ShowGreeting();
            } else
            {
                delayTimer.Tick += LockScreenLift_Delay;
                delayTimer.Interval = TimeSpan.FromMilliseconds(50);
                delayTimer.Start();
            }
            
        }
        private void ShowGreeting()
        {
            DoubleAnimation opacity_anim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            GreetingBox.BeginAnimation(OpacityProperty, opacity_anim);
            clock.Stop();
            PasswordInput.Focus();
        }
        private void LockScreenLift_Delay(object? sender, EventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;
            main.LockScreen_Lifting();
        }
        private void PasswordInput_Changed(object sender, RoutedEventArgs e)
        {
            
        }*/
    }
}
