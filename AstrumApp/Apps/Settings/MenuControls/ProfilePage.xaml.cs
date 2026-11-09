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

            UserCardControl.DataContext = App.Session.Profile;

            UserCardControl.Clicked += () =>
            {
                App.Session.Profile.BeginEdit();
                App.Navigation.NavigatePage(NavigationPage.ProfileEdit);
            };
        }
    }
}
