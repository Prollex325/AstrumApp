using AstrumApp.Apps.Authentication;
using AstrumApp.Apps.Settings;
using AstrumApp.Services;
using AstrumApp.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
//TODO : консоль разработчика

//TODO : изменение пароля (с защитой через пин) <-- Может быть
//TODO : возможно устройства <-- Пока не надо
//TODO : придумать как закрывать окно в приложении и подумать над ядром <-- Пока не надо
//TODO : выход из аккаунта и удаление аккаунта
//TODO : управление аккаунтами
namespace AstrumApp
{
    public partial class MainWindow : Window
    {
        private DesktopWidget? widget;

        private bool _startupStarted;

        private StartupCoordinator? coordinator;
        private MainAuthWindow authWindow;
        private MainSettingsPage settingsPage;

        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (s, e) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    UseLayoutRounding = false;
                    UseLayoutRounding = true;

                }, DispatcherPriority.Background); // чтоб мыла не было
            };

            Loaded += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //widget = new DesktopWidget();
                    //widget.Show();
                }));
            };

            coordinator = new(StartupView, WelcomeView);

            authWindow = new MainAuthWindow();
            settingsPage = new MainSettingsPage();

            ContentRendered += MainWindow_ContentRendered;
        }

        private async void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            if (_startupStarted) return;
            _startupStarted = true;

            try
            {
                // Даём загрузочному экрану немного времени появиться
                await Task.Delay(200);

                if (coordinator == null) return;

                await coordinator.StartUpScenario();

            }
            catch (Exception ex)
            {
                // Здесь обработка ошибки запуска
            }
        }
        public void CloseSettings()
        {
            MainFrame.Visibility = Visibility.Collapsed;
            MainFrame.Content = null;
        }
        public void OpenSettings()
        {
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(settingsPage);
            settingsPage.ToStartWin();
        }

        public void NavigateToMain()
        {
            AuthFrame.Visibility = Visibility.Collapsed;
        }

        public void NavigateToAuth(Apps.Authentication.NavigationPage page, string err = "")
        {
            if (!string.IsNullOrEmpty(err))
            {
                App.Notifications.Show(err, CardTypes.Error);
            }
            authWindow.SetStartPage(page);
            AuthFrame.Navigate(authWindow);
        }

        public void NavigateToWelcome()
        {
            WelcomeView.Visibility = Visibility.Visible;
        }

        public void CloseStartupView()
        {
            StartupView.Visibility = Visibility.Collapsed;
        }

        public void CloseWelcomeView()
        {
            WelcomeView.Visibility = Visibility.Collapsed;

            App.SettingsService.Settings.OnboardingVersion = 1;
            _ = App.SettingsService.SaveAsync();
        }

        public void ShowBlackoutMask()
        {
            BlackoutMask.Opacity = 0;
            BlackoutMask.Visibility = Visibility.Visible;
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            BlackoutMask.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void HideBlackoutMask()
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0.5,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            animation.Completed += (s, e) =>
            {
                BlackoutMask.Visibility = Visibility.Collapsed;
            };
            BlackoutMask.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void Unlock()
        {
            var lockScreenTransform = LockScreenControl.RenderTransform as TranslateTransform;

            DoubleAnimation animation = new DoubleAnimation
            {
                To = -800,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };
            lockScreenTransform?.BeginAnimation(TranslateTransform.YProperty, animation);

            DesktopControl.Visibility = Visibility.Visible;
            LeftPanelControl.Visibility = Visibility.Visible;
            RightPanelControl.Visibility = Visibility.Visible;

            RightPanel_Moving();
        }

        private void RightPanel_Moving()
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = -20,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuadraticEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };
            var transform = RightPanelControl.RenderTransform as TranslateTransform;
            transform?.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            /*if (App.Core.CurrentProcessName != "SettingsApp") return;*/

            switch (e.ChangedButton)
            {
                case MouseButton.XButton1:
                    App.Navigation.Back();
                    e.Handled = true;
                    break;

                case MouseButton.XButton2:
                    //App.Navigation.Forward();
                    //e.Handled = true;
                    break;
            }
        }

        private void UnLoad(object sender, RoutedEventArgs e)
        {
            widget?.Close();
        }
    }
}
