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
    }
}
