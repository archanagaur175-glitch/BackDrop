using System.Text.Json.Serialization;

namespace BackDrop.Core.Models;

/// <summary>A user-imported video loop tracked in the media library.</summary>
public sealed class MediaVideoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime AddedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Optional cached thumbnail path (under %LocalAppData%\BackDrop\Thumbnails).</summary>
    public string? ThumbnailPath { get; set; }

    [JsonIgnore]
    public string DisplaySize => FormatSize(SizeBytes);

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1_000_000_000 => $"{bytes / 1_000_000_000.0:0.0} GB",
        >= 1_000_000 => $"{bytes / 1_000_000.0:0.0} MB",
        >= 1_000 => $"{bytes / 1_000.0:0.0} KB",
        _ => $"{bytes} B",
    };
}
