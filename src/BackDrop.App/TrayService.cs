using H.NotifyIcon;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace BackDrop.App;

/// <summary>System tray icon with a Lock Now / Settings / Exit context menu.</summary>
public sealed class TrayService : IDisposable
{
    private readonly Action _showMainWindow;
    private readonly Action _engageLock;
    private readonly Action _exitApplication;

    private TaskbarIcon? _tray;

    public TrayService(Action showMainWindow, Action engageLock, Action exitApplication)
    {
        _showMainWindow = showMainWindow;
        _engageLock = engageLock;
        _exitApplication = exitApplication;
    }

    public void Initialize()
    {
        var menu = new MenuFlyout();

        var lockItem = new MenuFlyoutItem { Text = "Lock Now" };
        lockItem.Click += (_, _) => _engageLock();

        var settingsItem = new MenuFlyoutItem { Text = "Settings" };
        settingsItem.Click += (_, _) => _showMainWindow();

        var exitItem = new MenuFlyoutItem { Text = "Exit" };
        exitItem.Click += (_, _) => _exitApplication();

        menu.Items.Add(lockItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(settingsItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(exitItem);

        _tray = new TaskbarIcon
        {
            ToolTipText = "BackDrop — cinematic lock screen",
            IconSource = new GeneratedIconSource
            {
                Text = "\uE72E", // Segoe MDL2 lock glyph
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 18,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            },
            ContextMenu = menu,
        };

        _tray.ForceCreate();
    }

    public void Dispose() => _tray?.Dispose();
}
