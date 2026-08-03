using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using BackDrop.Core.Models;

namespace BackDrop.Core.Services;

/// <summary>
/// Validates and imports user-selected video loops. Persistence of the chosen
/// path + metadata goes through SettingsService; thumbnails are cached under
/// %LocalAppData%\BackDrop\Thumbnails.
/// </summary>
public sealed class MediaImportService
{
    /// <summary>Containers we accept. H.264/MP4 is the recommended codec.</summary>
    public static readonly string[] SupportedExtensions =
        { ".mp4", ".m4v", ".mkv", ".mov", ".wmv", ".avi", ".webm" };

    private const long MaxFileSizeBytes = 8L * 1024 * 1024 * 1024; // 8 GB sanity cap

    public static bool IsSupportedExtension(string path)
    {
        var extension = Path.GetExtension(path);
        return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValidFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !IsSupportedExtension(path))
            return false;

        var info = new FileInfo(path);
        return info.Exists && info.Length > 0 && info.Length <= MaxFileSizeBytes;
    }

    public MediaVideoItem? Import(string path)
    {
        if (!IsValidFile(path))
            return null;

        var info = new FileInfo(path);
        return new MediaVideoItem
        {
            Name = info.Name,
            FilePath = path,
            SizeBytes = info.Length,
            AddedUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Probes a file with the OS media stack to confirm it decodes before
    /// accepting it into the library.
    /// </summary>
    public async Task<bool> IsPlayableAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!IsValidFile(path))
            return false;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var player = new MediaPlayer();
        player.MediaOpened += (_, _) => tcs.TrySetResult(true);
        player.MediaFailed += (_, _) => tcs.TrySetResult(false);

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            player.Source = MediaSource.CreateFromStorageFile(file);
            player.Pause();

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(12), cancellationToken));
            return completed == tcs.Task && await tcs.Task;
        }
        catch
        {
            return false;
        }
        finally
        {
            player.Dispose();
        }
    }
}
