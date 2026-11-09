using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AstrumApp.Apps.Authentication
{
    public partial class RegistrationPage : UserControl
    {
        public RegistrationPage()
        {
            InitializeComponent();

            EmailTextBox.RaiseError("");
            PasswordBox.RaiseError("");
            ConfirmPasswordBox.RaiseError("");
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Text.Trim();
            string confirmPassword = ConfirmPasswordBox.Text.Trim();

            EmailTextBox.RaiseError("");
            PasswordBox.RaiseError("");
            ConfirmPasswordBox.RaiseError("");

            if (string.IsNullOrEmpty(email))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                EmailTextBox.RaiseError("*Введите почту");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                PasswordBox.RaiseError("*Введите пароль");
                return;
            }

            if (!StringFormatService.IsValidEmail(email))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                EmailTextBox.RaiseError("*Неверный формат почты");
                return;
            }

            if (password.Length < 6)
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                PasswordBox.RaiseError("*Пароль должен содержать минимум 6 символов");
                return;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                ConfirmPasswordBox.RaiseError("*Подтвердите пароль");
                return;
            }

            if (password != confirmPassword)
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                PasswordBox.RaiseError("*Пароли не совпадают");
                ConfirmPasswordBox.RaiseError("*Пароли не совпадают");
                return;
            }

            _ = EmailTextBox.RaiseSuccess("");

            App.Supabase.SetData("email", email);
            App.Supabase.SetData("password", password);

            MainAuthWindow.Instance?.NavigateToPage(NavigationPage.Bio, Directions.Left);
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Login, Directions.Right);
        }

        private void EmailBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => !StringFormatService.IsAllowedEmailChar(c));
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Login, Directions.Right);
        }
    }
}
