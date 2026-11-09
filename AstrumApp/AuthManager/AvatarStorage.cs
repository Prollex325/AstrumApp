using AstrumApp.Services;
using Microsoft.IdentityModel.Tokens;
using Supabase.Storage;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AstrumApp.AuthManager
{
    internal class AvatarStorageService
    {
        private readonly Supabase.Client _supabase;
        private const string BucketName = "avatars";

        public AvatarStorageService(Supabase.Client? supabase)
        {
            if (supabase == null) return;
            _supabase = supabase;
        }

        public async Task<string> UploadAvatarAsync(string userId, string localFilePath, string oldPath)
        {
            string extension = Path.GetExtension(localFilePath).ToLowerInvariant();

            string fileName = $"{Guid.NewGuid():N}{extension}";

            string storagePath = $"{userId}/{fileName}";

            try
            {
                await _supabase.Storage
                    .From(BucketName)
                    .Upload(
                        localFilePath,
                        storagePath,
                        new Supabase.Storage.FileOptions
                        {
                            Upsert = false
                        });

                if (!string.IsNullOrEmpty(oldPath))
                {
                    await DeleteAvatarAsync(oldPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ex: {ex}");
            }

            return storagePath;
        }

        public async Task DeleteAvatarAsync(string? storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
                return;

            await _supabase.Storage
                .From(BucketName)
                .Remove(new List<string> { storagePath });
        }

        public async Task<ImageSource> DownloadAvatarAsync(string storagePath)
        {
            byte[] bytes = await _supabase.Storage
                .From(BucketName)
                .Download(storagePath, (EventHandler<float>?)null);
            return FrozenImage.CreateFromBytes(bytes);
        }
    }

}