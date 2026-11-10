using AstrumApp.Security.PasswordWindowDir;
using AstrumApp.Services;
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

namespace AstrumApp.Apps.Settings.MenuControls.Security
{
    public partial class ChangePasswordPage : UserControl
    {
        public ChangePasswordPage()
        {
            InitializeComponent();
            Loaded += LoadedWin;
        }

        private void LoadedWin(object sender, RoutedEventArgs e)
        {
            PasswordDialogResult result = PasswordDialog.Show(string.Empty);

            switch (result)
            {
                case PasswordDialogResult.Cancel:
                    App.Navigation.Back();
                    break;
                case PasswordDialogResult.Success:
                    break;
                case PasswordDialogResult.None:
                    App.Navigation.Back();
                    break;
                default:
                    App.Notifications.Show("Неизвестная ошибка", CardTypes.Error);
                    App.Navigation.Back();
                    break;

            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NewPasswordBox.RaiseError("");
            ConfirmPasswordBox.RaiseError("");

            if (NewPasswordBox.Text == ConfirmPasswordBox.Text)
            {
                string newPassword = NewPasswordBox.Text;
                _ = ChangePasswordAsync(newPassword);
            } else
            {
                App.Notifications.Show("Пароли не совпадают", CardTypes.Success);
                NewPasswordBox.RaiseError("Пароли не совпадают");
                ConfirmPasswordBox.RaiseError("Пароли не совпадают");
            }
        }

        private async Task ChangePasswordAsync(string newPassword)
        {
            bool result = await App.Supabase.UpdateUserPassword(newPassword);

            if (result) 
            {
                App.Notifications.Show("Пароль изменен", CardTypes.Success);
            } else
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
            }

            App.Navigation.Back();
        }
    }
}
