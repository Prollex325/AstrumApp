using System;
using System.Diagnostics;
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
        private string? _sessionPath;
        public bool RememberMe { get; set; } = true;
        public bool RememberChoice { get; set; } = true;

        public SessionStorage(string? path = null)
        {
            _sessionPath = path;
        }

        public void SaveSession(AuthSession session)
        {
            if (!RememberMe) return;

            if (_sessionPath == null)
            {
                string id = session.User.Id!;
                _sessionPath = Path.Combine(AppPaths.AuthFolder, $"{id}.dat");
            } else
            {
                string id = session.User.Id!;
                if ((Path.Combine(AppPaths.AuthFolder, $"{id}.dat") != _sessionPath)) return;
            }

            string json = JsonConvert.SerializeObject(session);
            byte[] data = Encoding.UTF8.GetBytes(json);

            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);

            Directory.CreateDirectory(Path.GetDirectoryName(_sessionPath)!);

            File.WriteAllBytes(_sessionPath, encrypted);
        }

        public AuthSession? LoadSession()
        {
            if (!File.Exists(_sessionPath))
                return null;

            try
            {
                byte[] encrypted = File.ReadAllBytes(_sessionPath);

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
            if (File.Exists(_sessionPath))
                File.Delete(_sessionPath);
        }
    }
}
