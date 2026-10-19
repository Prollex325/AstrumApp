using AstrumApp.Apps.Authentication;
using AstrumApp.Profile;
using AstrumApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AstrumApp.Session
{
    public class UserSession
    {
        public string? UserId { get; private set; }
        public string? Email { get; private set; }

        public UserProfileViewModel Profile { get; private set; }

        public void LoadFromSupabase(Supabase.Gotrue.User user, ImageSource avatarImage)
        {
            UserId = user.Id;
            Email = user.Email;

            long registeredAt = user.UserMetadata.TryGetValue("registered_at", out var regAt)
                && long.TryParse(regAt?.ToString(), out long regAtRes)
                ? regAtRes
                : 0;

            long updatedAt = user.UserMetadata.TryGetValue("registered_at", out var updAt)
                && long.TryParse(updAt?.ToString(), out long updAtRes)
                ? updAtRes
                : 0;

            string name, bio, email;

            if (user.UserMetadata.TryGetValue("display_name", out var nameRaw))
                name = nameRaw?.ToString() ?? "";
            else name = "";

            if (user.UserMetadata.TryGetValue("bio", out var bioRaw))
                bio = bioRaw?.ToString() ?? "";
            else bio = "";

            if (Email != null)
                email = Email;
            else email = "";

            Profile = new UserProfileViewModel(name, bio, email, avatarImage)
            {
                RegisteredAt = StringFormatService.FormatTimestamp(registeredAt),
                UpdatedAt = StringFormatService.FormatTimestamp(updatedAt),
            };
        }

        public void UpdateSupabase()
        {
            Debug.WriteLine($"Updating Supabase with: Email={Profile.Email}, Username={Profile.Username}, Bio={Profile.Bio}");
            //App.Supabase.UpdateData(Profile.Email, Profile.Username, Profile.Bio, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public async Task UpdateAvatar()
        {
            try
            {
                string s = await App.Supabase.UploadAvatar(AppPaths.AvatarPath);
                Debug.WriteLine($"Avatar upload result: {s}");
            } catch (Exception ex)
            {
                Debug.WriteLine($"Avatar upload failed: {ex.Message}");
                App.Notifications.Show("ex.Message", CardTypes.Error);
                return;
            }
        }

        public void Clear()
        {
            UserId = null;
            Email = null;
            //Profile.Clear();
        }
    }
}
