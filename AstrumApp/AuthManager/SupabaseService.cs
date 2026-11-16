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
        public SupabaseClient? Client { get; private set; }

        private AvatarStorageService? _avatarStorage;
        private Dictionary<string, object> _userMetadata = new();


        public bool PrepareClient()
        {
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL")
                ?? throw new InvalidOperationException("SUPABASE_URL не задан.");

            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY")
                ?? throw new InvalidOperationException("SUPABASE_KEY не задан.");

            try
            {
                Client = new SupabaseClient(url, key, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false
                });

                SessionStorage? storage = App.AccountManager.CreateStorage();

                Client.Auth.SetPersistence(storage);
                return true;
            } catch (Exception ex) {
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

    }
}
