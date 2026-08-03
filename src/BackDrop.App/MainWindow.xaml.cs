using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using WinRT.Interop;

namespace BackDrop.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SystemBackdrop = new MicaBackdrop();
    }

    /// <summary>Call immediately after <see cref="Window.Activate"/> — AppWindow/HWND are only valid post-activation.</summary>
    public void AfterActivated()
    {
        AppWindow.Resize(new SizeInt32(880, 760));

        var hwnd = WindowNative.GetWindowHandle(this);
        SettingsHost.Initialize(hwnd);

        // Taskbar/window icon (unpackaged apps show a generic icon otherwise).
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "BackDrop.ico");
        if (File.Exists(iconPath))
            AppWindow.SetIcon(iconPath);
    }
}
