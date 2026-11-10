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
    public partial class RegistrationProgressPage : UserControl
    {
        public RegistrationProgressPage()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                _ = SavingData();
            };
            ProgressPageControl.IsSuccessfulResult += isSuccessful =>
            {
                if (isSuccessful)
                {
                    MainAuthWindow.Instance.CloseAuth();
                }
                else
                {
                    App.Notifications.Show("Ошибка регистрации", CardTypes.Error);
                    MainAuthWindow.Instance.NavigateToPage(NavigationPage.Registration, Directions.Right);
                }
            };
        }

    public async Task SavingData()
        {
            ProgressPageControl.SetStatus("Создаём аккаунт...");
            AuthResult result = await App.Supabase.Registration();
            if (result.Success)
            {
                ProgressPageControl.SetStatus("Загружаем аватарку...");
                string newPath = await App.Supabase.FirstUploadAvatarAsync(AppPaths.AvatarPathTemp);
                ProgressPageControl.SetStatus("Сохраняем аватарку...");
                bool avatarSaved = await App.Supabase.UpdateAvatarPathAsync(newPath);

                if (avatarSaved)
                {
                    ProgressPageControl.ShowSuccess();
                    FrozenImage.DeleteFile(AppPaths.AvatarPathTemp);
                }
                else
                    ProgressPageControl.ShowError();
            }
            else
            {
                if (result.Error != null) ProgressPageControl.ShowError(result.Error);
                else ProgressPageControl.ShowError();
            }
        }
    }
}
