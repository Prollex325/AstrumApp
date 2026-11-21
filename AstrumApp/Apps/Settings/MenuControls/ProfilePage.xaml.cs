using AstrumApp.Apps.Settings.Navigation;
using AstrumApp.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace AstrumApp.Apps.Settings
{
    // Сменить профиль
    // Выйти из аккаунта
    // Удалить аккаунт
    public partial class ProfilePage : UserControl
    {
        public ProfilePage()
        {
            InitializeComponent();

            UserCardControl.DataContext = App.Session.Profile;

            UserCardControl.Clicked += () =>
            {
                App.Navigation.NavigatePage(NavigationPage.ProfileEdit);
            };
        }
    }
}
