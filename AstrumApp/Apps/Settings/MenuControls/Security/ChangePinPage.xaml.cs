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
    public partial class ChangePinPage : UserControl
    {
        public ChangePinPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                OldPasswordBox.Password = string.Empty;
                NewPasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;

                OldPasswordBox.Focus();
                PasswordBox_PasswordChanged(s, e);
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (OldPasswordBox.Password.Length == 4)
            {
                NewPasswordBox.Focus();
            }

            if (NewPasswordBox.Password.Length == 4 && OldPasswordBox.Password.Length == 4)
            {
                ConfirmPasswordBox.Focus();
            }

            if (ConfirmPasswordBox.Password.Length == 4 && NewPasswordBox.Password.Length == 4 && NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                App.Notifications.Show("PIN не совпадает", CardTypes.Error);
            }

            ChangePinButton.IsEnabled = OldPasswordBox.Password.Length == 4 && NewPasswordBox.Password.Length == 4 && 
                ConfirmPasswordBox.Password.Length == 4 && NewPasswordBox.Password == ConfirmPasswordBox.Password;
        }

        private void ChangePin_Click(object sender, RoutedEventArgs e)
        {
            if (OldPasswordBox.Password == NewPasswordBox.Password)
            {
                App.Notifications.Show("PIN совпадает с текущим", CardTypes.Error);
                return;
            }

            bool result = App.Security.Pin.Change(OldPasswordBox.Password, NewPasswordBox.Password);
            if (result)
            {
                App.Notifications.Show("PIN успешно изменен", CardTypes.Success);

                App.Security.Save();

                App.Navigation.NavigateMenu(NavigationPage.Security);
            } else
            {
                App.Notifications.Show("Старый PIN неверный", CardTypes.Error);
            }
        }
    }
}
