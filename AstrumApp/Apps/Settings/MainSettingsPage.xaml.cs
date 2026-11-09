using AstrumApp.Services;
using System.Windows.Controls;

namespace AstrumApp.Apps.Settings
{
    public partial class MainSettingsPage : UserControl
    {
        public MainSettingsPage()
        {
            InitializeComponent();

            App.Navigation.Initialize(MenuPageHost);
            App.Navigation.NavigateMenu(NavigationPage.Profile);
        }

        public void ToStartWin()
        {
            NavigationPage startWin = NavigationPage.Profile;
            App.Navigation.NavigateMenu(startWin);
        }

        private void CloseSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Navigation.NavigateMenu(NavigationPage.NotFoundPage);
            if (App.Current.MainWindow is MainWindow window) window.CloseSettings();
        }
    }
}
