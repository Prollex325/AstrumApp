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

namespace AstrumApp.Apps.Settings.MenuControls.ProfileOptions
{

    public partial class SwitchProfilePage : UserControl
    {   
        public SwitchProfilePage()
        {
            InitializeComponent();
        }

        private void GetLocalAccounts(object sender, RoutedEventArgs e)
        {
            ObservableCollection<AccountCard> _items = App.AccountManager.LoadAllCacheProfiles(true);
            AccountsList.ItemsSource = _items;

            if (_items.Count == 3)
            {
                AddAccountButton.IsEnabled = false;
                Hint.Visibility = Visibility.Visible;
            }

            RememberChoice.IsChecked = App.AccountManager.RememberChoice;
        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            _ = App.AccountManager.SwitchAccount(null);
        }

        private void SwitchAccount(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is AccountCard card)
                {
                    if (!card.IsActive)
                    {
                        _ = App.AccountManager.SwitchAccount(card.ID);
                    }
                }
            }
        }

        private void ChangeRememberChoice(object sender, RoutedEventArgs e)
        {
            App.AccountManager.RememberChoice = RememberChoice.IsChecked ?? false;
        }

        private void LeaveAccount(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?\nТекущая сессия будет удалена.", MessageBoxButton.YesNo, MessageBoxType.Warning) == MessageBoxResult.Yes)
            {
                _ = App.AccountManager.DeleteCurrentSession();
            }
        }
    }
}
