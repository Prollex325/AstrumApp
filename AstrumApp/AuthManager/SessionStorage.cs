using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Supabase.Gotrue.Interfaces;
using AuthSession = Supabase.Gotrue.Session;

namespace AstrumApp.AuthManager
{
    public class SessionStorage : IGotrueSessionPersistence<AuthSession>
    {
        private static readonly string SessionPath = Path.Combine(AppPaths.AppFolder, "session.dat");

        public void SaveSession(AuthSession session)
        {
            string json = JsonConvert.SerializeObject(session);
            byte[] data = Encoding.UTF8.GetBytes(json);

            byte[] encrypted = ProtectedData.Protect(
                data,
                null,
                DataProtectionScope.CurrentUser
            );

            Directory.CreateDirectory(
                Path.GetDirectoryName(SessionPath)!
            );

            File.WriteAllBytes(SessionPath, encrypted);
        }

        public AuthSession? LoadSession()
        {
            if (!File.Exists(SessionPath))
                return null;

            try
            {
                byte[] encrypted = File.ReadAllBytes(SessionPath);

                byte[] data = ProtectedData.Unprotect(
                    encrypted,
                    null,
                    DataProtectionScope.CurrentUser
                );

                string json = Encoding.UTF8.GetString(data);

                return JsonConvert.DeserializeObject<AuthSession>(json);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is CryptographicException ||
                ex is JsonException)
            {
                // Повреждённая или недоступная сессия
                return null;
            }
        }

        public void DestroySession()
        {
            if (File.Exists(SessionPath))
                File.Delete(SessionPath);
        }
    }
}
