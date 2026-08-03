using BackDrop.Core.Services;

namespace BackDrop.App.ViewModels;

/// <summary>
/// Minimal view model for the lock overlay: resolves which video loop to play
/// (user-selected or bundled default). Clock/layout work is handled by controls.
/// </summary>
public sealed class LockOverlayViewModel
{
    private readonly SettingsService _settings = AppServices.Settings;

    public string? ActiveVideoPath =>
        _settings.Settings.ActiveVideoPath ?? AppServices.BundledVideoPath;
}
