using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AstrumApp.Services;

namespace AstrumApp.Apps.Authentication
{

    public partial class SelectProfilePage : UserControl
    {   
        public SelectProfilePage()
        {
            InitializeComponent();
        }

        private void GetLocalAccounts(object sender, RoutedEventArgs e)
        {
            AccountsList.ItemsSource = App.AccountManager.LoadAllCacheProfiles(false);
        }

        private void SelectProfile(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is AccountCard card)
                {
                    if (!card.IsActive)
                    {
                        _ = App.AccountManager.SelectAccount(card.ID);
                    }
                }
            }
        }

        private void ToRegisterPage(object sender, RoutedEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Registration, Directions.Left);
        }

        private void ToLoginPage(object sender, RoutedEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Login, Directions.Left);
        }
    }
}
