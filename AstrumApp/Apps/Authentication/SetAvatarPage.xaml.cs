using AstrumApp.AuthManager;
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

namespace AstrumApp.Apps.Authentication
{
    public partial class SetAvatarPage : UserControl
    {
        public SetAvatarPage()
        {
            InitializeComponent();

            string? localAvatar = AppPaths.AvatarPathTemp;

            AvatarPicker.AvatarSaved += image =>
            {
                if (image == null)
                {
                    if (MessageBox.Show("Вы не выбрали аватарку.\nХотите продолжить без неё?", MessageBoxButton.YesNo, MessageBoxType.Warning) == MessageBoxResult.No)
                    {
                        return;
                    } else
                    {
                        FrozenImage.DeleteFile(localAvatar);
                    }
                } else
                {
                    localAvatar = FrozenImage.SaveToFile(image, AppPaths.AvatarPathTemp);
                }
                MainAuthWindow.Instance?.NavigateToPage(NavigationPage.UserCard, Directions.Left);
            };

            AvatarPicker.EditCancelled += () =>
            {
                MainAuthWindow.Instance?.NavigateToPage(NavigationPage.UserCard, Directions.Left);
            };

            AvatarPicker.EditBack += () =>
            {
                MainAuthWindow.Instance?.NavigateToPage(NavigationPage.Bio, Directions.Right);
            };
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Registration, Directions.Right);
        }
    }
}
