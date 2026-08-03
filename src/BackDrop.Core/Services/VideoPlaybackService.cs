using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace BackDrop.Core.Services;

/// <summary>
/// Creates MediaPlayer instances for overlay windows: hardware-accelerated
/// (Media Foundation), seamless looping via IsLoopingEnabled, muted by default,
/// one player per monitor.
/// </summary>
public sealed class VideoPlaybackService
{
    public MediaPlayer CreateLoopingPlayer(string? videoPath, bool muted = true)
    {
        var player = new MediaPlayer
        {
            IsLoopingEnabled = true,
            IsMuted = muted,
        };

        if (!string.IsNullOrWhiteSpace(videoPath) && File.Exists(videoPath))
            _ = LoadSourceAsync(player, videoPath);

        return player;
    }

    private static async Task LoadSourceAsync(MediaPlayer player, string path)
    {
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            player.Source = MediaSource.CreateFromStorageFile(file);
            player.Play();
        }
        catch
        {
            // Graceful: overlay falls back to the ambient gradient + clock.
        }
    }
}
