using System.Text.Json;
using BackDrop.Core.Models;

namespace BackDrop.Core.Services;

/// <summary>
/// Loads and persists settings to %LocalAppData%\BackDrop\settings.json.
/// Constructor accepts an optional base directory so tests can isolate writes.
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _directory;
    private readonly string _filePath;

    public SettingsService(string? baseDirectory = null)
    {
        _directory = baseDirectory ?? DefaultDirectory;
        _filePath = Path.Combine(_directory, "settings.json");
        Settings = Load();
    }

    public BackDropSettings Settings { get; private set; }

    public static string DefaultDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BackDrop");

    /// <summary>Directory where imported media thumbnails are cached.</summary>
    public string ThumbnailDirectory => Path.Combine(_directory, "Thumbnails");

    public void Save()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(Settings, SerializerOptions));
    }

    private BackDropSettings Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var loaded = JsonSerializer.Deserialize<BackDropSettings>(File.ReadAllText(_filePath));
                if (loaded is not null)
                    return loaded;
            }
        }
        catch (JsonException)
        {
            // Corrupt settings file falls back to defaults rather than crashing at startup.
        }

        return new BackDropSettings();
    }
}
