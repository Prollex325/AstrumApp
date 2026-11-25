using System;
using System.IO;
using System.Text.Json;

namespace AstrumApp.Services.Configuration;

public sealed class PublicAppConfig
{
    public string SupabaseUrl { get; set; } = "";
    public string SupabaseKey { get; set; } = "";

    public static PublicAppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.public.json");

        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<PublicAppConfig>(json)
            ?? throw new InvalidOperationException(
                "Не удалось загрузить конфигурацию Astrum."
            );
    }
}