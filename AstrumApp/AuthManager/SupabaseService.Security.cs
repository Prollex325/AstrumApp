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
        public async Task<bool> UpdateUserPassword(string newPassword)
        {
            if (Client == null) return false;

            var attributes = new UserAttributes
            {
                Password = newPassword
            };

            var user = await Client.Auth.Update(attributes);
            return user != null;
        }

        public async Task<bool> VerifyPasswordAsync(string password)
        {
            if (Client?.Auth.CurrentUser is not { } currentUser)
                throw new InvalidOperationException("Пользователь не авторизован.");

            string email = currentUser.Email
                ?? throw new InvalidOperationException("Email пользователя отсутствует.");

            string url = Environment.GetEnvironmentVariable("SUPABASE_URL")!;
            string key = Environment.GetEnvironmentVariable("SUPABASE_KEY")!;

            // Отдельный клиент, не связанный с основной сессией
            var verifier = new SupabaseClient(url, key, new SupabaseOptions
            {
                AutoRefreshToken = false,
                AutoConnectRealtime = false
            });

            await verifier.InitializeAsync();

            try
            {
                var session = await verifier.Auth.SignIn(email, password);

                return session?.User?.Id == currentUser.Id;
            }
            catch (GotrueException ex) when (
                ex.StatusCode == 400 &&
                ex.Message.Contains("Invalid login credentials",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            finally
            {
                if (verifier.Auth.CurrentSession != null)
                {
                    try
                    {
                        await verifier.Auth.SignOut(
                            Supabase.Gotrue.Constants.SignOutScope.Local);
                    }
                    catch
                    {
                        // Ошибку очистки временной сессии можно записать в лог.
                    }
                }
            }
        }

    }
}
