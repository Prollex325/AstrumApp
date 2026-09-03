using AstrumApp.Interfaces;
using AstrumApp.Services;
using AstrumApp.Profile;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace AstrumApp.Apps.Settings.MenuControls
{
    public partial class ProfileEdit : UserControl, ICanClose
    {
        bool hasChanges = false;
        bool isSaved = false;
        
        UserProfileViewModel profile = App.Session.Profile;
        public ProfileEdit()
        {
            InitializeComponent();
            this.DataContext = profile;
            profile.PropertyChanged += Profile_PropertyChanged;
            SaveButton.IsEnabled = false;
        }

        private void EditAvatar_Click(object sender, RoutedEventArgs e)
        {
            App.Navigation.NavigatePage(NavigationPage.AvatarEdit);
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            profile.Save();
            isSaved = true;

            App.Navigation.Back();

            App.Notifications.Show("Настройки сохранены!", CardTypes.Success);
        }

        private void DiscardChanges_Click(object sender, RoutedEventArgs e)
        {
            profile.Cancel();

            App.Navigation.Back();

            if (hasChanges && !isSaved) App.Notifications.Show("Настройки отменены!", CardTypes.Error);
        }

        private void Profile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(profile.HasChanges))
            {
                hasChanges = profile.HasChanges;
                SaveButton.IsEnabled = hasChanges;
            }
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = profile.HasChanges;
        }

        public bool CanClose() 
        {
            if (!hasChanges || isSaved) return true;

            return MessageBox.Show("У вас есть несохраненные настройки. Изменения могут не примениться", MessageBoxButton.OKCancel, MessageBoxType.Warning) == MessageBoxResult.Yes;
        }
    }
}
