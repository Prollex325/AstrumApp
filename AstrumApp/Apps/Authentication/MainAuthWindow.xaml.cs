using AstrumApp.Apps.Settings;
using AstrumApp.Apps.Settings.MenuControls;
using AstrumApp.Apps.Settings.MenuControls.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrumApp.Apps.Authentication
{
    public enum NavigationPage
    {
        Login,
        Registration,
        SetAvatar,
        Bio,
        UserCard,

        Progress
    }
    public enum Directions
    {
        Left,
        Right
    }

    public partial class MainAuthWindow : Page
    {
        private readonly Dictionary<NavigationPage, UserControl> _pages = new();
        private static readonly Dictionary<NavigationPage, string> StepTitles = new()
            {
                { NavigationPage.Registration, "Давайте знакомиться!" },
                { NavigationPage.SetAvatar, "Покажите себя!" },
                { NavigationPage.Bio, "Пару слов о себе?" },
                { NavigationPage.UserCard, "Ну как вам результат?" }
            };

        public static MainAuthWindow Instance { get; private set; }
        public MainAuthWindow()
        {
            InitializeComponent();

            Instance = this;

            RegisterPages();

            StartCubeRotation(LeftCube, 30, 1);
            //StartCubeRotation(UpperCube);
            StartCubeRotation(RightCube, 20, -1);
            StartCubeFloating(LeftCubeMove, 3, -30);
            //StartCubeFloating(UpperCubeMove);
            StartCubeFloating(RightCubeMove, 5, -50);

            StepControl.InitializeSteps(_pages.Count - 2);

            //StepControl.StepsCount = _pages.Count - 2; 
            StepControl.Visibility = Visibility.Hidden;

            Loaded += (_, _) => StartLogoAnim();

            PageHost.Content = _pages[NavigationPage.Login];
        }

        public void SetStartPage(NavigationPage page)
        {
            NavigateToPage(page, Directions.Left);
        }

        public void StartLogoAnim()
        {
            var animation = new DoubleAnimation
            {
                From = -70,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                BeginTime = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
            };
            LogoTranslate.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void RegisterPages()
        {
            _pages[NavigationPage.Login] = new LoginPage();
            _pages[NavigationPage.Registration] = new RegistrationPage();
            _pages[NavigationPage.SetAvatar] = new SetAvatarPage();
            _pages[NavigationPage.Bio] = new BioPage();
            _pages[NavigationPage.UserCard] = new UserCardPage();
            _pages[NavigationPage.Progress] = new RegistrationProgressPage();
        }

        private void StartCubeRotation(Image cube, int speed, int factor)
        {
            if (cube.RenderTransform is not TransformGroup group)
                return;

            if (group.Children[0] is not RotateTransform rotate)
                return;

            double startAngle = rotate.Angle;

            var animation = new DoubleAnimation
            {
                From = startAngle,
                To = startAngle + factor * 360,
                Duration = TimeSpan.FromSeconds(speed),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void StartCubeFloating(TranslateTransform cubeMove, int speed, int dist)
        {
            if (cubeMove == null)
                return;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = dist, // Поднимается на 30 пикселей
                Duration = TimeSpan.FromSeconds(speed),

                EasingFunction = new SineEase
                {
                    EasingMode = EasingMode.EaseInOut
                },

                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            cubeMove.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        public void NavigateToPage(NavigationPage page, Directions direction)
        {
            if (_pages.TryGetValue(page, out var targetPage))
            {
                string directionString = direction.ToString();
                AnimatePage(targetPage, directionString);

                if (targetPage is RegistrationPage)
                {
                    StepControl.Visibility = Visibility.Visible;
                }
                else if (targetPage is LoginPage or RegistrationProgressPage)
                {
                    StepControl.Visibility = Visibility.Hidden;
                    StepControl.SetCurrentStep(0);
                } else if (targetPage is UserCardPage userCardPage)
                {
                    userCardPage.SetUserData();
                }

                if (!(targetPage is LoginPage or RegistrationProgressPage))
                {
                    if (directionString == "Left")
                    {
                        StepControl.NextStep(StepTitles.GetValueOrDefault(page, "Осталось совсем немного!"));
                    }
                    else if (directionString == "Right")
                    {
                        StepControl.BackStep(StepTitles.GetValueOrDefault(page, "Осталось совсем немного!"));
                    }
                }
            } 
            else 
            {
                throw new ArgumentException($"Page {page} is not registered.", nameof(page));
                }
        } 

        private async void AnimatePage(UserControl page, String direction)
        {
            int to = direction switch
            {
                "Left" => -60,
                "Right" => 60,
                _ => 0
            };
            int from = direction switch
            {
                "Left" => 60,
                "Right" => -60,
                _ => 0
            };
            var oldTransform = new TranslateTransform();

            PageHost.RenderTransform = oldTransform;

            // Уход текущей страницы
            var hideAnimation = new DoubleAnimation(0, to,
                TimeSpan.FromMilliseconds(150));

            hideAnimation.EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseIn
            };

            oldTransform.BeginAnimation(
                TranslateTransform.XProperty,
                hideAnimation);

            var fadeOut = new DoubleAnimation(1, 0,
                TimeSpan.FromMilliseconds(150));

            PageHost.BeginAnimation(
                UIElement.OpacityProperty,
                fadeOut);

            await Task.Delay(130);

            // Меняем страницу
            PageHost.Content = page;

            // Начальная позиция новой
            var newTransform = new TranslateTransform();

            PageHost.RenderTransform = newTransform;

            PageHost.Opacity = 0;

            // Появление новой страницы
            var showAnimation = new DoubleAnimation(from, 0,
                TimeSpan.FromMilliseconds(180));

            showAnimation.EasingFunction = new QuarticEase
            {
                EasingMode = EasingMode.EaseOut
            };

            var fadeIn = new DoubleAnimation(0, 1,
                TimeSpan.FromMilliseconds(180));

            newTransform.BeginAnimation(
                TranslateTransform.XProperty,
                showAnimation);

            PageHost.BeginAnimation(
                UIElement.OpacityProperty,
                fadeIn);
        }

        public void CloseAuth()
        {
            DoubleAnimation mainTranslateAnim = new()
            {
                From = 0,
                To = -70,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            DoubleAnimation mainOpacityAnim = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(280),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                }
            }; 
            
            mainOpacityAnim.Completed += (_, _) => {
                MainGrid.Visibility = Visibility.Collapsed;
                if (App.Current.MainWindow is MainWindow main) main.NavigateToMain();
            };

            MainTranslate.BeginAnimation(TranslateTransform.YProperty, mainTranslateAnim);

            MainGrid.BeginAnimation(UIElement.OpacityProperty, mainOpacityAnim); 
        }
    }
}
