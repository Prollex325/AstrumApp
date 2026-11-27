using AstrumApp.Profile;
using AstrumApp.Services;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AuthUser = Supabase.Gotrue.User;
using SupabaseClient = Supabase.Client;

namespace AstrumApp.AuthManager
{
    public partial class SupabaseService
    {
        public async Task ApplyUserAsync(User user)
        {
            _avatarStorage = new AvatarStorageService(Client);

            await ApplyUser(user);
        }

        private async Task ApplyUser(AuthUser user)
        {
            if (_avatarStorage == null || user == null) return;

            var metadata = user.UserMetadata;
            var appMetadata = user.AppMetadata;

            string userId = user.Id ?? string.Empty;
            string name = metadata?.GetValueOrDefault("username")?.ToString() ?? string.Empty;
            string bio = metadata?.GetValueOrDefault("bio")?.ToString() ?? string.Empty;
            string email = user.Email ?? string.Empty;
            string profileAvatarPath = metadata?.GetValueOrDefault("avatar_path")?.ToString() ?? string.Empty;

            long ReadTimestamp(string key) => long.TryParse(metadata?.GetValueOrDefault(key)?.ToString(), out var value) ? value : 0;

            string registeredAt = StringFormatService.FormatTimestamp(ReadTimestamp("registered_at"));
            string updatedAt = StringFormatService.FormatTimestamp(ReadTimestamp("updated_at"));

            string isPremiumString = appMetadata?.GetValueOrDefault("is_premium")?.ToString() ?? "0";

            bool isPremium = isPremiumString == "1";

            App.Session.SetIdentity(userId, profileAvatarPath);

            ImageSource? avatar = null;
            if (!string.IsNullOrEmpty(profileAvatarPath))
            {
                avatar = await _avatarStorage.DownloadAvatarAsync(profileAvatarPath);
            } else
            {
                
            }
            /*if (!string.IsNullOrEmpty(profileAvatarPath))
            {
                avatar = await _avatarStorage.DownloadAvatarAsync(profileAvatarPath);
                if (avatar != null)
                {
                    BitmapSource? localAvatar = FrozenImage.CreateFromUri(AppPaths.AvatarPath);

                    if (avatar is BitmapSource bitmap)
                    {
                        if (!File.Exists(AppPaths.AvatarPath) || !ReferenceEquals(localAvatar, bitmap))
                        {
                            //FrozenImage.SaveToFile(bitmap, AppPaths.AvatarPath);
                        }
                    }
                }
            }*/

            try
            {
                App.AccountManager.SaveCache(userId, name, email, avatar);
            }
            catch (Exception) { }

            try
            {
                App.AccountManager.SaveActiveID(userId);
            }
            catch (Exception) { }

            App.Session.Profile.Load(name, bio, email, avatar, registeredAt, updatedAt, isPremium);
        }

        public async Task UpdateUserData(UserProfileViewModel profile, long timestamp, string? avatarPath)
        {
            if (Client == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            var attributes = new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    ["display_name"] = profile.Username,
                    ["bio"] = profile.Bio,
                    ["updated_at"] = timestamp,
                    ["avatar_path"] = avatarPath ?? string.Empty
                }
            };

            if (profile.Email != Client.Auth.CurrentUser?.Email)
            {
                //attributes.Email = profile.Email;
            }

            var user = await Client.Auth.Update(attributes); // здесь ошибка че то с почтой ваще хз
        }

        
    }
}
