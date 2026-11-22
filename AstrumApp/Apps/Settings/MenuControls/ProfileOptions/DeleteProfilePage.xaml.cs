using AstrumApp.AuthManager;
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

namespace AstrumApp.Apps.Settings.MenuControls.ProfileOptions
{
    public partial class DeleteProfilePage : UserControl
    {
        public DeleteProfilePage()
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

        private async void ConfirmDeleting(object sender, RoutedEventArgs e)
        {
            DeleteButton.IsEnabled = false;
            NewPasswordBox.RaiseError("");
            ConfirmPasswordBox.RaiseError("");

            if (NewPasswordBox.Text == ConfirmPasswordBox.Text)
            {

                if (MessageBox.Show("Вы собираетесь навсегда удалить аккаунт.\nЭто действие невозможно отменить.",
                    MessageBoxButton.YesNo, MessageBoxType.Destructive) == MessageBoxResult.Yes)
                {
                    string newPassword = NewPasswordBox.Text;

                    AuthResult result = await App.Supabase.DeleteAccountAsync(newPassword);

                    if (!result.Success) App.Notifications.Show(result.Error ?? "Неизвестная ошибка", CardTypes.Error);

                    if (result.Success)
                    {
                        App.AccountManager.DeleteAccount();
                    }
                }
            }
            else
            {
                App.Notifications.Show("Пароли не совпадают", CardTypes.Error);
                NewPasswordBox.RaiseError("Пароли не совпадают");
                ConfirmPasswordBox.RaiseError("Пароли не совпадают");
            }
            DeleteButton.IsEnabled = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.Back();
        }
    }
}
