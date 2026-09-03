using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
    public partial class CreatePinPage : UserControl
    {
        public CreatePinPage()
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

        private void CreatePin_Click(object sender, RoutedEventArgs e)
        {
            if (App.Security.Pin.HasPin())
            {
                App.Notifications.Show("PIN уже установлен", CardTypes.Error);
            }
            else
            {
                App.Security.Pin.Set(PasswordBox.Password);

                App.Notifications.Show("PIN установлен", CardTypes.Success);
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
                CreatePinButton.IsEnabled = PasswordBox.Password.Length == 4 && ConfirmPasswordBox.Password.Length == 4 && 
                    PasswordBox.Password == ConfirmPasswordBox.Password;
            }
        }
    }
}
