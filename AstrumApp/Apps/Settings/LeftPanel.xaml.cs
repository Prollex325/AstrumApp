using AstrumApp.Services;
using System.Windows;
using System.Windows.Controls;
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

        public LeftPanel()
        {
            InitializeComponent();

            this.DataContext = App.Session.Profile;
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

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 0.7;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 1;
            SettingsItems.SelectedIndex = 0;
            App.Navigation.NavigateMenu(NavigationPage.Profile);
        }

        private void SettingsItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int newIndex = SettingsItems.SelectedIndex;

            if (newIndex < 0)
                return;

            Dispatcher.InvokeAsync(() =>
            {
                var newItem = SettingsItems.ItemContainerGenerator
                    .ContainerFromIndex(newIndex) as ListBoxItem;

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
                
                newItemIndicator?.Opacity = 1;

                previousIndex = newIndex;
            });
        }
    }
}
