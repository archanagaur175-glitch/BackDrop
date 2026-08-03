using BackDrop.App.Common;
using BackDrop.Core.Services;

namespace BackDrop.App.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings = AppServices.Settings;

    private string _hotkeyStatus = string.Empty;
    private string _pinStatus = string.Empty;

    public string HotkeyText
    {
        get => _settings.Settings.HotkeyText;
        set
        {
            _settings.Settings.HotkeyText = value;
            OnPropertyChanged();
        }
    }

    public bool StartWithWindows
    {
        get => _settings.Settings.StartWithWindows;
        set
        {
            _settings.Settings.StartWithWindows = value;
            StartupService.SetEnabled(value);
            _settings.Save();
            OnPropertyChanged();
        }
    }

    public bool ShowSeconds
    {
        get => _settings.Settings.ShowSeconds;
        set
        {
            _settings.Settings.ShowSeconds = value;
            _settings.Save();
            OnPropertyChanged();
        }
    }

    public double VignetteIntensity
    {
        get => _settings.Settings.VignetteIntensity;
        set
        {
            _settings.Settings.VignetteIntensity = value;
            _settings.Save();
            OnPropertyChanged();
        }
    }

    public string Layout => _settings.Settings.Layout;

    public bool PinEnabled => _settings.Settings.Pin is not null;

    public string HotkeyStatus
    {
        get => _hotkeyStatus;
        private set
        {
            _hotkeyStatus = value;
            OnPropertyChanged();
        }
    }

    public string PinStatus
    {
        get => _pinStatus;
        set
        {
            _pinStatus = value;
            OnPropertyChanged();
        }
    }

    public void SetLayout(string layout)
    {
        _settings.Settings.Layout = layout;
        _settings.Save();
        OnPropertyChanged(nameof(Layout));
    }

    public void ApplyHotkey()
    {
        try
        {
            var (modifiers, virtualKey) = HotkeyParser.Parse(HotkeyText);
            HotkeyStatus = $"Applied: {HotkeyParser.Format(modifiers, virtualKey)}";
            _settings.Save();
        }
        catch (FormatException ex)
        {
            HotkeyStatus = ex.Message;
        }
    }

    public void SetPin(string pin)
    {
        try
        {
            new PinManager(_settings).SetPin(pin);
            PinStatus = "PIN set — dismissal now requires it.";
        }
        catch (ArgumentException ex)
        {
            PinStatus = ex.Message;
        }

        OnPropertyChanged(nameof(PinEnabled));
    }

    public void ClearPin()
    {
        new PinManager(_settings).ClearPin();
        PinStatus = "PIN removed.";
        OnPropertyChanged(nameof(PinEnabled));
    }
}
