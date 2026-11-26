using AstrumApp.Apps.Authentication;
using AstrumApp.Apps.Settings;
using AstrumApp.Controls;
using AstrumApp.Services;
using AstrumApp.Services.Updates;
using AstrumApp.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
//TODO : консоль разработчика

//TODO : возможно устройства <-- Пока не надо
//TODO : придумать как закрывать окно в приложении и подумать над ядром <-- Пока не надо


//TODO : автообновления


//TODO : опустить сервис навигации до уровня приложения настроек
namespace AstrumApp
{
    public partial class MainWindow : Window
    {
        private DesktopWidget? widget;

        private bool _startupStarted;

        private StartupCoordinator? coordinator;
        private MainAuthWindow? authWindow;
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

            Loaded += UpdateTestOnLoaded;

            Loaded += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //widget = new DesktopWidget();
                    //widget.Show();
                }));
                MainWindow_ContentRendered(new(), new());
            };

            LockScreenControl.Unlocked += Unlock;
            LockScreenControl.Locked += CloseSettings;

            LockScreenControl.Lock();

            coordinator = new(StartupView, WelcomeView);

            authWindow = new MainAuthWindow();
            settingsPage = new MainSettingsPage();

            //ContentRendered += MainWindow_ContentRendered;

            //CloseStartupView();
            //Unlock(); // уведомление обновление доступно или скачано
            //OpenSettings();
        }

        private async void UpdateTestOnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UpdateTestOnLoaded;
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

                bool result = await coordinator.StartUpScenario();
                StartupView.StopAnims();

                if (!result)
                {
                    Close();
                }

            }
            catch (Exception ex)
            {
                // Здесь обработка ошибки запуска
            }
        }
        public void CloseSettings()
        {
            //MainFrame.Visibility = Visibility.Collapsed;
            MainFrame.Content = null;
        }
        public void OpenSettings()
        {
            //MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(settingsPage);
            settingsPage.OpenStartPage();
        }

        public void NavigateToMain(bool needsAuthFrameClearing = false)
        {
            if (needsAuthFrameClearing)
            {
                AuthFrame.Content = null;
                authWindow = null;
            }
        }

        public void NavigateToAuth(Apps.Authentication.NavigationPage page, string err = "")
        {
            if (!string.IsNullOrEmpty(err))
                App.Notifications.Show(err, CardTypes.Error);

            authWindow = new MainAuthWindow();

            AuthFrame.Content = authWindow;

            AuthFrame.Opacity = 0;
            AuthFrame.Visibility = Visibility.Visible;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                }
            };

            animation.Completed += (s, e) =>
            {
                MainFrame.Content = null;
                CloseSettings();
                Lock();
            };

            AuthFrame.BeginAnimation(UIElement.OpacityProperty, animation);


            authWindow.SetStartPage(page);
        }
        public void CloseAuth()
        {
            authWindow?.CloseAuth();
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
            CheckForUpdates();
            LockScreenControl.Visibility = Visibility.Collapsed;

            DesktopControl.Visibility = Visibility.Visible;
            LeftPanelControl.Visibility = Visibility.Visible;
            RightPanelControl.Visibility = Visibility.Visible;

            RightPanel_Moving();
        }

        public void Lock()
        {
            LockScreenControl.Visibility = Visibility.Visible;
            LockScreenControl.Lock();
        }

        private async void CheckForUpdates()
        {
            await App.Updates.CheckAsync();
            UpdateStatus status = App.Updates.Status;

            if (status is UpdateStatus.Available) App.Notifications.Show("Обновление доступно", CardTypes.Success);
            else if (status is UpdateStatus.ReadyToInstall) App.Notifications.Show("Обновление скачано", CardTypes.Success);
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                Debug.WriteLine($"Captured: {Mouse.Captured}");
                Debug.WriteLine($"DirectlyOver: {Mouse.DirectlyOver}");

                System.Diagnostics.Debugger.Break();
                e.Handled = true;
            }
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

        private void UnLoad(object sender, RoutedEventArgs e)
        {
            widget?.Close();
        }
    }
}
