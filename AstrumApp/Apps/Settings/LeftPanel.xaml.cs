using AstrumApp.Apps.Settings.Navigation;
using Supabase.Postgrest;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AstrumApp.Apps.Settings
{
    public partial class LeftPanel : UserControl
    {
        int previousIndex = -1;
        private bool _isProfileMenuOpen;

        private Dictionary<NavigationPage, int> _pagesIndexes = new()
        {
            [NavigationPage.Profile] = 0,
            [NavigationPage.Security] = 1,
            // Уведомления = 2
            [NavigationPage.ChangelogPage] = 3,
            [NavigationPage.SystemInfo] = 4,
        };

        public LeftPanel()
        {
            InitializeComponent();

            Loaded += (s, e) => 
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SettingsItems.SelectedIndex = 0;
                }));
            };

            App.Navigation.NavigationNewMenu += SelectIndexFromPage;

            this.DataContext = App.Session.Profile;
        }

        public void SelectIndexFromPage(NavigationPage page)
        {
            SettingsItems.SelectedIndex = _pagesIndexes.GetValueOrDefault(page, 0);
        }
        
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.Profile);
        }
        private void SystemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.SystemInfo);
        }
        private void SecurityButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.Security);
        }
        private void ChangelogButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.ChangelogPage);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 0.7;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 1;

            SetProfileMenuOpen(!_isProfileMenuOpen);
        }

        private void SetProfileMenuOpen(bool open)
        {
            _isProfileMenuOpen = open;

            double currentHeight = ProfileMenuHost.ActualHeight;

            // Определяем необходимую высоту по содержимому
            ProfileInteractOptions.Measure(new Size(ProfileButton.ActualWidth, double.PositiveInfinity));

            double targetHeight = open
                ? ProfileInteractOptions.DesiredSize.Height
                : 0;

            // Останавливаем предыдущую анимацию
            ProfileMenuHost.BeginAnimation(FrameworkElement.HeightProperty, null);
            ProfileMenuHost.Height = currentHeight;

            var animation = new DoubleAnimation
            {
                From = currentHeight,
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                },
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (_, _) =>
            {
                if (_isProfileMenuOpen == open)
                    ProfileMenuHost.Height = open ? double.NaN : 0;
            };

            ProfileMenuHost.BeginAnimation(
                FrameworkElement.HeightProperty, animation);
        }

        private void SettingsItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int newIndex = SettingsItems.SelectedIndex;

            if (newIndex < 0)
                return;

            Dispatcher.InvokeAsync(() =>
            {
                var newItem = SettingsItems.ItemContainerGenerator.ContainerFromIndex(newIndex) as ListBoxItem;

                if (newItem == null)
                    return;

                TranslateTransform? lastItemTransform = null;
                FrameworkElement? lastItemIndicator = null;


                if (previousIndex > -1) 
                {
                    var lastItem = SettingsItems.ItemContainerGenerator
                        .ContainerFromIndex(previousIndex) as ListBoxItem;

                    if (lastItem != null)
                    {
                        lastItem.ApplyTemplate();

                        lastItemTransform = lastItem.Template.FindName("IndicatorTransform", lastItem) as TranslateTransform;
                        lastItemIndicator = lastItem.Template.FindName("SelectionIndicator", lastItem) as FrameworkElement;
                    }
                }

                newItem.ApplyTemplate();

                var newItemTransform = newItem.Template.FindName("IndicatorTransform", newItem) as TranslateTransform;
                var newItemIndicator = newItem.Template.FindName("SelectionIndicator", newItem) as FrameworkElement;

                if (newItemTransform == null)
                    return;

                double from = 0;
                double to = 0;

                // если идем вниз
                if (newIndex > previousIndex)
                {
                    from = -10;
                    to = 10;
                }

                // если идем вверх
                else if (newIndex < previousIndex)
                {
                    from = 10;
                    to = -10;
                }

                DoubleAnimation lastItemTransformAnim = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(180),
                    To = to,
                    From = 0
                };

                lastItemTransform?.BeginAnimation(TranslateTransform.YProperty, lastItemTransformAnim);

                DoubleAnimation newItemTransformAnim = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(180),
                    To = 0,
                    From = from
                };

                newItemTransform?.BeginAnimation(TranslateTransform.YProperty, newItemTransformAnim);

                previousIndex = newIndex;
            });
        }

        private void ProfileEdit_Option(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.ProfileEdit);
            SettingsItems.SelectedIndex = 0;
        }

        private void ProfileChange_Option(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.SwitchProfilePage);
            SettingsItems.SelectedIndex = 0;
        }

        public void MouseDownPreviewed()
        {
            if (!_isProfileMenuOpen)
                return;

            if (ProfileButton.IsMouseOver || ProfileMenuHost.IsMouseOver)
                return;

            SetProfileMenuOpen(false);
        }
    }
}
