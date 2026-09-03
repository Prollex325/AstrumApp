using AstrumApp.Apps.Settings;
using AstrumApp.Services;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AstrumApp.Apps.Authentication
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Text.Trim();

            if (!IsValidEmail(email))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                EmailTextBox.RaiseError("*Неверный формат почты");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                PasswordBox.RaiseError("*Введите пароль");
                return;
            }

            AuthButton.IsEnabled = false;

            try
            {
                AuthResult result = await App.Supabase.UserLogin(email, password);

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
                MainWindow mw = App.Current.MainWindow as MainWindow;
                mw?.MainFrame.Visibility = Visibility.Collapsed;
            }
            finally
            {
                AuthButton.IsEnabled = true;
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToReg();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
