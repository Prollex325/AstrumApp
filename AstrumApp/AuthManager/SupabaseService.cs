using AstrumApp.Models;
using AstrumApp.Profile;
using AstrumApp.Services;
using AstrumApp.Services.Configuration;
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
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AuthUser = Supabase.Gotrue.User;
using SupabaseClient = Supabase.Client;

namespace AstrumApp.AuthManager
{
    public partial class SupabaseService
    {
        public SupabaseClient? Client { get; private set; }

        private AvatarStorageService? _avatarStorage;
        private Dictionary<string, object> _userMetadata = new();

        public async Task<List<Announcement>> LoadAnnouncementsAsync()
        {
            if (Client == null) return new();
            var response = await Client.From<Announcement>().Get();

            var announcements = response.Models;
            return announcements;
        }

        public string? GetAnnouncementImageUrl(string imagePath)
        {
            if (Client == null) return null;
            if (string.IsNullOrWhiteSpace(imagePath)) return null;

            return Client.Storage
                .From("announcements")
                .GetPublicUrl(imagePath);
        }

        public bool PrepareClient(bool rememberMe = true)
        {
            var config = PublicAppConfig.Load();

            var url = Environment.GetEnvironmentVariable("SUPABASE_URL")
                ?? config.SupabaseUrl;

            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY")
                ?? config.SupabaseKey;

            try
            {
                Client = new SupabaseClient(url, key, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                });

                if (rememberMe)
                {
                    SessionStorage storage = App.AccountManager.CreateStorage();
                    Client.Auth.SetPersistence(storage);
                }

                return true;
            } catch (Exception ex) {
                Debug.WriteLine($"[Supabase] PrepareClient failed: {ex}");
                return false;
            }
        }

        public async Task<User?> RestoreSessionAsync()
        {
            if (Client == null) return null;

            Client.Auth.LoadSession();

            await Client.InitializeAsync();

            return Client.Auth.CurrentUser;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            if (Client == null)
                return false;

            try
            {
                var settings = await Client.Auth.Settings()
                    .WaitAsync(TimeSpan.FromSeconds(5));

                return settings != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка подключения к Supabase: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActivateAccountAsync(string? userId, string message)
        {
            bool res = PrepareClient();
            if (res)
            {
                User? user = await RestoreSessionAsync();
                if (App.Current.MainWindow is MainWindow window)
                {
                    if (userId == null) window.NavigateToAuth(Apps.Authentication.NavigationPage.Login, message);
                    else
                    {
                        if (user != null)
                        {
                            window.NavigateToMain();
                            await ApplyUserAsync(user);
                        }
                        else return false;
                    }
                    return true;
                }
                else return false;
            }
            else return false;
        }

    }
}
