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

using AstrumApp.Services;

namespace AstrumApp.Apps.Authentication
{
    public partial class BioPage : Page
    {
        public BioPage()
        {
            InitializeComponent();
        }

        private async void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string bio = BioTextBox.Text;

            RegistrationButton.IsEnabled = false;

            AuthResult result = await App.Supabase.UserRegistration(name, bio);

            if (!result.Success)
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                MainAuthWindow.Instance.NavigateToReg();
                return;
            }

            App.Notifications.Show("Успешная регистрация", CardTypes.Success);
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToReg();
        }
    }
}
