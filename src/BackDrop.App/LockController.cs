using BackDrop.Core.Services;

namespace BackDrop.App;

/// <summary>
/// Owns the lock lifecycle: registers the global hotkey, spawns a borderless
/// overlay window per physical display, and closes them all on dismissal.
/// </summary>
public sealed class LockController : IDisposable
{
    private readonly HotkeyService _hotkey = new();
    private readonly MultiMonitorService _monitors = new();
    private readonly VideoPlaybackService _playback = new();

    private readonly List<LockOverlayWindow> _windows = new();
    private bool _locked;

    public void Start()
    {
        var settings = AppServices.Settings.Settings;
        var (modifiers, virtualKey) = HotkeyParser.Parse(settings.HotkeyText);
        _hotkey.HotkeyPressed += (_, _) => Engage();
        _hotkey.Start(modifiers, virtualKey);
    }

    public void Engage()
    {
        if (_locked)
            return;

        _locked = true;

        var settings = AppServices.Settings.Settings;
        var videoPath = settings.ActiveVideoPath ?? AppServices.BundledVideoPath;

        foreach (var display in _monitors.GetDisplays())
        {
            var window = new LockOverlayWindow(display, _playback, videoPath);
            window.Dismissed += (_, _) => Dismiss();
            _windows.Add(window);
        }

        if (_windows.Count == 0)
            _locked = false;
    }

    public void Dismiss()
    {
        foreach (var window in _windows)
            window.Close();

        _windows.Clear();
        _locked = false;
    }

    public void Dispose()
    {
        _hotkey.Dispose();
        Dismiss();
    }
}
