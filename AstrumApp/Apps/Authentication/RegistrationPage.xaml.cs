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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net.Mail;

namespace AstrumApp.Apps.Authentication
{
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Text.Trim();

            if (!IsValidEmail(email))
            {
                EmailTextBox.RaiseError("*Неверный формат почты");
                return;
            }

            if (password.Length < 6)
            {
                PasswordBox.RaiseError("*Пароль должен содержать минимум 6 символов");
                return;
            }

            _ = EmailTextBox.RaiseSuccess("");

            App.Supabase.SaveUserData(email, password);

            MainAuthWindow.Instance?.NavigateToBio();
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

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToLogin();
        }
    }
}
