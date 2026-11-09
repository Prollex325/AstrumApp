using AstrumApp.Apps.Settings;
using AstrumApp.Services;
using AstrumApp.AuthManager;
using System;
using System.Collections.Generic;
using System.Net.Mail;
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

namespace AstrumApp.Apps.Authentication
{
    public partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            InitializeComponent();
            EmailTextBox.RaiseError("");
            PasswordBox.RaiseError("");
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Text.Trim();

            EmailTextBox.RaiseError(""); 
            PasswordBox.RaiseError(""); 

            if (string.IsNullOrEmpty(email))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                EmailTextBox.RaiseError("*Введите почту");
                return;
            }

            if (!StringFormatService.IsValidEmail(email))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                EmailTextBox.RaiseError("*Неверный формат почты");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                PasswordBox.RaiseError("*Введите пароль");
                return;
            }

            AuthButton.IsEnabled = false;

            try
            {
                AuthResult result = await App.Supabase.Login(email, password);

                if (!result.Success)
                {
                    App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                    switch (result.ErrorCode)
                    {
                        case "invalid_credentials":
                            if (result.Error != null)
                            {
                                EmailTextBox.RaiseError(result.Error);
                                PasswordBox.RaiseError(result.Error);
                            }
                            break;

                        case "email_not_confirmed":
                            if (result.Error != null)
                            {
                                EmailTextBox.RaiseError(result.Error);
                            }
                            break;

                        case "user_not_found":
                            if (result.Error != null)
                            {
                                EmailTextBox.RaiseError(result.Error);
                            }
                            break;

                        default:
                            if (result.Error != null)
                            {
                                EmailTextBox.RaiseError(result.Error);
                            }
                            break;
                    }
                    return;
                }

                EmailTextBox.RaiseError("");
                PasswordBox.RaiseError("");

                App.Notifications.Show("Авторизация успешна", CardTypes.Success);

                MainAuthWindow.Instance.CloseAuth();
            }
            finally
            {
                AuthButton.IsEnabled = true;
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Registration, Directions.Left);
        }

        private void EmailBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !StringFormatService.IsAllowedEmailChar(c));
        }
    }
}
