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
        public async Task<string> FirstUploadAvatarAsync(string avatarPath)
        {
            string appPath = File.Exists(avatarPath) ? avatarPath : string.Empty;

            string newStoragePath = string.Empty;
            if (!string.IsNullOrEmpty(appPath))
            {
                newStoragePath = await UpdateAvatarAsync(appPath);
            }

            return newStoragePath;
        }

        public async Task<bool> UpdateAvatarPathAsync(string avatarPath)
        {
            if (Client == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            var attributes = new UserAttributes
            {
                Data = new Dictionary<string, object>
                {
                    ["avatar_path"] = avatarPath ?? string.Empty
                }
            };

            var user = await Client.Auth.Update(attributes);

            return user != null;
        }

        public async Task<string> UpdateAvatarAsync(string filePath)
        {
            if (Client == null || Client.Auth.CurrentUser == null || Client.Auth.CurrentUser.Id == null || _avatarStorage == null)
                throw new InvalidOperationException("Supabase client is not initialized.");

            string oldPath = Client.Auth.CurrentUser.UserMetadata.GetValueOrDefault("avatar_path")?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(filePath))
            {
                await _avatarStorage.DeleteAvatarAsync(oldPath);
                return string.Empty;
            }
            return await _avatarStorage.UploadAvatarAsync(Client.Auth.CurrentUser.Id, filePath, oldPath);
        }

    }
}
