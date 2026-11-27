using AstrumApp.Apps.Settings.Navigation;
using System.Windows.Controls;
using System.Windows.Input;

namespace AstrumApp.Apps.Settings
{
    public partial class MainSettingsPage : UserControl
    {
        public MainSettingsPage()
        {
            InitializeComponent();

            App.Navigation.Initialize(MenuPageHost, NavigationPage.Profile);
        }
        
        public void OpenStartPage(NavigationPage startPage = NavigationPage.None)
        {
            if (startPage is not NavigationPage.None) App.Navigation.NavigateMenu(startPage);
        }

        private void CloseSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //App.Navigation.NavigateMenu(NavigationPage.NotFoundPage);
            if (App.Current.MainWindow is MainWindow window) window.CloseSettings();
        }

        private void Settings_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.XButton1:
                    App.Navigation.Back();
                    //e.Handled = true;
                    break;

                case MouseButton.XButton2:
                    App.Navigation.Forward();
                    //e.Handled = true;
                    break;
            }

            LeftPanelControl.MouseDownPreviewed();
        }
    }
}
