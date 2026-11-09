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
using AstrumApp.AuthManager;

namespace AstrumApp.Apps.Authentication
{
    public partial class BioPage : UserControl
    {
        public BioPage()
        {
            InitializeComponent();

            NameTextBox.RaiseError("");
            BioTextBox.RaiseError("");
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string bio = BioTextBox.Text.Trim();

            NameTextBox.RaiseError(""); 
            BioTextBox.RaiseError("");  

            if (string.IsNullOrEmpty(name))
            {
                App.Notifications.Show("Поле с именем пустое", CardTypes.Error);
                NameTextBox.RaiseError("*Введите имя");
                return;
            }

            if (name.Length < 2 || name.Length > 16)
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                NameTextBox.RaiseError("*Длина имени должна быть от 2 до 16 символов");
                return;
            }

            if (name.Length > 160)
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                BioTextBox.RaiseError("*Длина биографии должна быть до 160 символов");
                return;
            }

            if (!StringFormatService.IsValidUsername(name))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                NameTextBox.RaiseError("*Имя содержит запрещенные символы");
                return;
            }

            if (!StringFormatService.IsValidBio(bio))
            {
                App.Notifications.Show("Произошла ошибка", CardTypes.Error);
                BioTextBox.RaiseError("*Биография содержит запрещенные символы");
                return;
            }

            App.Supabase.SetData("display_name", name);
            App.Supabase.SetData("username", name);
            App.Supabase.SetData("bio", bio);

            MainAuthWindow.Instance?.NavigateToPage(NavigationPage.SetAvatar, Directions.Left);
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Registration, Directions.Right);
        }
    }
}
