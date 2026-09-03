using AstrumApp.Apps.Authentication;
using AstrumApp.Apps.Settings;
using AstrumApp.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AstrumApp
{
    public partial class MainWindow : Window
    {

        private DesktopWidget? widget;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //widget = new DesktopWidget();
                    //widget.Show();
                }));
                //DesktopControl.Visibility = Visibility.Visible;
                //LeftPanelControl.Visibility = Visibility.Visible;
                //RightPanelControl.Visibility = Visibility.Visible;


                //RightPanel_Moving();

                Dispatcher.BeginInvoke(() =>
                {
                    UseLayoutRounding = false;
                    UseLayoutRounding = true;
                }, DispatcherPriority.Render); // чтоб мыла не было
            };

            //MainFrame.Navigate(new LockScreen());
            //MainFrame.Navigate(new MainAuthWindow()); // todo очень важно сделать позже если supabase ответит ошибкой после биографией отправлять на страницу регистрации!!!
            //MainFrame.Navigate(new MainSettingsPage());
            opensets();
        }

        public void opensets()
        {
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new MainSettingsPage());
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

        public void LockScreen_Lifting()
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
