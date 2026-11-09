using AstrumApp.Apps.Authentication;
using AstrumApp.Profile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace AstrumApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserCardOLD.xaml
    /// </summary>
    public partial class UserCardOLD : UserControl
    {
        public event Action? Clicked;
        public UserCardOLD()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (DataContext is UserCardTemplate profileTemplate)
                    PremiumMark.Visibility = profileTemplate.IsPremium ? Visibility.Visible : Visibility.Collapsed;

                if (DataContext is UserProfileViewModel profile)
                {
                    PremiumMark.Visibility = profile.IsPremium ? Visibility.Visible : Visibility.Collapsed;
                }
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 0.5;
            Clicked?.Invoke();
            ((Border)sender).Opacity = 1;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Border)sender).Opacity = 1;
        }
    }
}
