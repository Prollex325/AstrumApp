using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AstrumApp.Services;

namespace AstrumApp.Apps.Settings.MenuControls.Security
{
    public partial class PinSettingsPage : UserControl
    {
        public PinSettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Security.Pin.HasPin())
            {
                CreatePinButton.IsEnabled = false;
                ChangePinButton.IsEnabled = true;
                DeletePinButton.IsEnabled = true;
            }
            else
            {
                CreatePinButton.IsEnabled = true;
                ChangePinButton.IsEnabled = false;
                DeletePinButton.IsEnabled = false;
                
            }
        }

        private void CreatePinButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigatePage(NavigationPage.CreatePinPage);
        }

        private void ChangePinButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigatePage(NavigationPage.ChangePinPage);
        }

        private void DeletePinButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigatePage(NavigationPage.DeletePinPage);
        }
    }
}
