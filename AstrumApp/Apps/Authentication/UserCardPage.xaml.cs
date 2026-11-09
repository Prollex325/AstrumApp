using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.IO;

namespace AstrumApp.Apps.Authentication
{
    public partial class UserCardPage : UserControl
    {
        public UserCardPage()
        {
            InitializeComponent();
        }

        public void SetUserData()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string stringNow = StringFormatService.FormatTimestamp(now);
            string path = File.Exists(AppPaths.AvatarPathTemp) ? AppPaths.AvatarPathTemp : AppPaths.DefaultAvatarPath;

            ImageSource? avatarImage = null;
            try
            {
                avatarImage = FrozenImage.CreateFromUri(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load avatar image from path: {path}. Exception: {ex.Message}");
            }
            UserCardControl.DataContext = new UserCardTemplate(
                username: (string)(App.Supabase.GetData("username") ?? string.Empty),
                bio: (string)(App.Supabase.GetData("bio") ?? string.Empty),
                email: (string)(App.Supabase.GetData("email") ?? string.Empty),
                avatar: avatarImage,
                registeredAt: stringNow,
                updatedAt: stringNow,
                isPremium: false
            );
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.SetAvatar, Directions.Right);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            MainAuthWindow.Instance.NavigateToPage(NavigationPage.Progress, Directions.Left);
        }
    }

    public class UserCardTemplate
    {
        public string Username { get; private set; } = string.Empty;
        public string Bio { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public ImageSource Avatar
        {
            get => _avatar ?? DefaultAvatar;
            private set
            {
                _avatar = value;
            }
        }
        private static ImageSource DefaultAvatar = FrozenImage.CreateFromUri(AppPaths.DefaultAvatarPath) ?? throw new InvalidOperationException("Failed to create default avatar.");
        private ImageSource? _avatar = DefaultAvatar;

        public string RegisteredAt { get; private set; } = string.Empty;
        public string UpdatedAt { get; private set; } = string.Empty;
        public bool IsPremium { get; private set; } = false;

        public UserCardTemplate(string username = "", string bio = "", string email = "", ImageSource? avatar = null,
            string registeredAt = "", string updatedAt = "", bool isPremium = false)
        {
            Username = username;
            Bio = bio;
            Email = email;
            _avatar = avatar;
            RegisteredAt = registeredAt;
            UpdatedAt = updatedAt;
            RegisteredAt = registeredAt;
            UpdatedAt = updatedAt;
            IsPremium = IsPremium;
        }
    }
}
