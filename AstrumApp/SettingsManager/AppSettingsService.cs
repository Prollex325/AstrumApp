using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstrumApp.SettingsManager;

public class AppSettingsService
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettings Settings { get; private set; } = new();

    public AppSettingsService()
    {
        string directory = AppPaths.AppFolder;

        _filePath = Path.Combine(directory, "settings.json");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            Settings = new AppSettings();
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);

            Settings = JsonSerializer.Deserialize<AppSettings>(
                json, _jsonOptions
            ) ?? new AppSettings();
        }
        catch (JsonException)
        {
            Settings = new AppSettings();
        }
    }

    public async Task SaveAsync()
    {
        string directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(Settings, _jsonOptions);

        string tempPath = _filePath + ".tmp";

        await File.WriteAllTextAsync(tempPath, json);

        File.Move(tempPath, _filePath, true);
    }
}