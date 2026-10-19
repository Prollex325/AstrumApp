using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AstrumApp
{
    public class AppPaths
    {
        public static string AppDataFolder { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string LocalAppDataFolder { get; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string AppFolder { get; } = Path.Combine(LocalAppDataFolder, "AstrumApp");
        public static string SecurityFile { get; } = Path.Combine(AppFolder, "security.json");
        public static string DefaultAvatarPath { get; } = "pack://application:,,,/Assets/user/default_avatar.png";
        public static string AvatarPath { get; } = Path.Combine(AppFolder, "user", "avatar.png");
    }
}
