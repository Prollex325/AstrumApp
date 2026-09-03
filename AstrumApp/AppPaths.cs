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
    }
}
