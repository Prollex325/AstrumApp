using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AstrumApp.Apps.Settings.MenuControls.ProfileOptions
{

    public class ProfileItemTEST
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ImageSource Avatar { get; set; } = FrozenImage.CreateFromUri(AppPaths.DefaultAvatarPath)!;
        public bool IsActive { get; set; } = false;
    }

    public partial class SwitchProfilePage : UserControl
    {
        public ObservableCollection<ProfileItemTEST> Items { get; set; }
        
        public SwitchProfilePage()
        {
            InitializeComponent();
            var profile = App.Session.Profile;

            Items = new ObservableCollection<ProfileItemTEST>
            {
                new ProfileItemTEST
                {
                    Username = profile.Username,
                    Email = profile.Email,
                    Avatar = profile.Avatar,
                    IsActive = true
                },
                new ProfileItemTEST
                {
                    Username = "vitek",
                    Email = "uryadnovv@mail.ru",
                },

                new ProfileItemTEST
                {
                    Username = "TEST",
                    Email = "TEST@test.ts",
                },
            };


            AccountsList.ItemsSource = Items;
        }

        private void LeaveAccount(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("", MessageBoxButton.YesNo, MessageBoxType.Warning) == MessageBoxResult.Yes)
            {
                
            }
        }
    }
}
