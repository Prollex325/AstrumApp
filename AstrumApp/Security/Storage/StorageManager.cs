using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using AstrumApp.Security.Models;


namespace AstrumApp.Security.Storage
{
    internal class StorageManager
    {
        public SecurityData? Load()
        {
            string json = File.ReadAllText(AppPaths.SecurityFile);
            if (string.IsNullOrWhiteSpace(json)) return null;

            return JsonSerializer.Deserialize<SecurityData>(json);
        }

        public void Save(SecurityData data)
        {
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(AppPaths.SecurityFile, json);
        }
    }
}
