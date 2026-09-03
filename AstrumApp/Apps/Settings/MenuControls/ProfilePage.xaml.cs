using AstrumApp.Services;
using AstrumApp.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AstrumApp.Apps.Settings
{
    public partial class ProfilePage : UserControl
    {
        public ProfilePage()
        {
            InitializeComponent();

            this.DataContext = App.Session.Profile;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 0.5;
            App.Session.Profile.BeginEdit();
            App.Navigation.NavigatePage(NavigationPage.ProfileEdit);
            ((Border)sender).Opacity = 1;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 1;
        }
    }
}
