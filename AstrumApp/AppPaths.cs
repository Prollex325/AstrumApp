using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AstrumApp
{
    public class AppPaths
    {
        public static string AppDataFolder { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string LocalAppDataFolder { get;  }  = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string AppFolder { get; } = CreateDirectory(Path.Combine(LocalAppDataFolder, "AstrumData"));
        public static string CacheFolder { get; } = CreateDirectory(Path.Combine(AppFolder, "Cache"));
        public static string AuthFolder { get; } = CreateDirectory(Path.Combine(AppFolder, "Auth"));
        public static string UserFolder { get; } = CreateDirectory(Path.Combine(AppFolder, "user"));
        public static string AnnouncementsCache { get; } = CreateDirectory(Path.Combine(CacheFolder, "Announcements"));
        public static string SecurityFile { get; } = Path.Combine(AppFolder, "security.json");
        public static string DefaultAvatarPath { get; } = "pack://application:,,,/Assets/user/default_avatar.png";
        public static string? AvatarPath
        {
            get
            {
                string? userId = App.Session?.UserId;

                if (string.IsNullOrWhiteSpace(userId)) return null;

                return Path.Combine(CacheFolder, userId, "avatar.png");
                /*throw new InvalidOperationException(
                    "Невозможно получить путь к аватару: пользователь не выбран.");*/

            }
        }

        public static string? AvatarPathTemp
        {
            get
            {
                string? userId = App.Session?.UserId;

                if (string.IsNullOrWhiteSpace(userId)) return null;

                return Path.Combine(CacheFolder, userId, "avatar_temp.png");
                /*throw new InvalidOperationException(
                    "Невозможно получить путь к аватару: пользователь не выбран.");*/

            }
        }
        public static string SettingsPath { get; } = Path.Combine(AppFolder, "settings.json");
        public static string ChangelogPath { get; } = Path.Combine(AppContext.BaseDirectory, "Docs", "CHANGELOG.md");

        private static string CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
