using BackDrop.Core.Services;

namespace BackDrop.App;

/// <summary>
/// Shared application-level service instances. A single SettingsService is used
/// everywhere so in-memory settings edits are never clobbered by a stale copy.
/// </summary>
public static class AppServices
{
    public static SettingsService Settings { get; } = new SettingsService();

    /// <summary>Bundled default loop (copied to output as Content). Null when absent.</summary>
    public static string? BundledVideoPath { get; } = FindBundledVideo();

    private static string? FindBundledVideo()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Videos", "default-loop.mp4");
        return File.Exists(path) ? path : null;
    }
}
