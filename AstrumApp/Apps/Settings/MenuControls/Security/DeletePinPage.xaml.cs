using AstrumApp.Services;
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

namespace AstrumApp.Apps.Settings.MenuControls.Security
{
    public partial class DeletePinPage : UserControl
    {
        public DeletePinPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;

                PasswordBox.Focus();
                PasswordBox_PasswordChanged(s, e);
            };
        }

        private void RemovePin_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Security.Pin.HasPin())
            {
                App.Notifications.Show("PIN не установлен", CardTypes.Error);
            } else if (MessageBox.Show("Вы уверены, что хотите удалить PIN?", MessageBoxButton.OKCancel, MessageBoxType.Warning) == MessageBoxResult.Yes)
            {
                App.Security.Pin.Remove(PasswordBox.Password); 

                App.Notifications.Show("PIN удален", CardTypes.Success);
                App.Security.Save();

                App.Navigation.NavigateMenu(NavigationPage.Security);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.Length == 4)
            {
                ConfirmPasswordBox.Focus();
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password && PasswordBox.Password.Length == 4 && 
                ConfirmPasswordBox.Password.Length == 4)
            {
                App.Notifications.Show("PIN не совпадает", CardTypes.Error);
            }
            else
            {
                RemovePinButton.IsEnabled = PasswordBox.Password.Length == 4 && ConfirmPasswordBox.Password.Length == 4 &&
                    PasswordBox.Password == ConfirmPasswordBox.Password;
            }
        }
    }
}
