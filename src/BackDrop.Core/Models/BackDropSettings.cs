using System.Text.Json.Serialization;

namespace BackDrop.Core.Models;

/// <summary>Persisted user settings (survives across runs, stored as JSON).</summary>
public sealed class BackDropSettings
{
    /// <summary>Global hotkey, e.g. "Ctrl+Alt+L". Parsed by <see cref="Core.Services.HotkeyParser"/>.</summary>
    public string HotkeyText { get; set; } = "Ctrl+Alt+L";

    /// <summary>Transparent HKCU Run entry — only ever enabled by explicit user action.</summary>
    public bool StartWithWindows { get; set; }

    public string Layout { get; set; } = nameof(LayoutKind.MinimalistCenter);

    public bool ShowSeconds { get; set; } = true;

    /// <summary>Vignette overlay opacity in [0,1].</summary>
    public double VignetteIntensity { get; set; } = 0.45;

    /// <summary>Audio muted by default; BackDrop is a visual utility.</summary>
    public bool MuteVideo { get; set; } = true;

    /// <summary>Currently selected loop (null → bundled default loop).</summary>
    public string? ActiveVideoPath { get; set; }

    public List<MediaVideoItem> MediaLibrary { get; set; } = new();

    /// <summary>Optional PIN gate. Null when disabled.</summary>
    public PinRecord? Pin { get; set; }

    [JsonIgnore]
    public LayoutKind ActiveLayout =>
        Enum.TryParse<LayoutKind>(Layout, true, out var layout) ? layout : LayoutKind.MinimalistCenter;
}
