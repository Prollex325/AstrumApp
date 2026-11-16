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
        public static string AppFolder { get; } = CreateDirectory(Path.Combine(LocalAppDataFolder, "AstrumApp"));
        public static string CacheFolder { get; } = CreateDirectory(Path.Combine(AppFolder, "Cache"));
        public static string AuthFolder { get; } = CreateDirectory(Path.Combine(AppFolder, "Auth"));
        public static string UserFolder { get; } = CreateDirectory(Path.Combine(AppFolder, "user"));
        public static string SecurityFile { get; } = Path.Combine(AppFolder, "security.json");
        public static string DefaultAvatarPath { get; } = "pack://application:,,,/Assets/user/default_avatar.png";
        public static string AvatarPath { get; } = Path.Combine(UserFolder, "avatar.png");
        public static string AvatarPathTemp { get; } = Path.Combine(UserFolder, "avatar_temp.png");
        public static string SettingsPath { get; } = Path.Combine(AppFolder, "settings.json");
        public static string ChangelogPath { get; } = Path.Combine(AppContext.BaseDirectory, "Docs", "CHANGELOG.md");

        private static string CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
