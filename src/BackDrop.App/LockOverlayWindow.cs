using BackDrop.App.Views;
using BackDrop.Core.Models;
using BackDrop.Core.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace BackDrop.App;

/// <summary>
/// A borderless, always-on-top, fullscreen overlay window covering one physical
/// display. Raised <see cref="Dismissed"/> when the user dismisses the lock.
/// </summary>
public sealed class LockOverlayWindow : Window
{
    public event EventHandler? Dismissed;

    public LockOverlayWindow(DisplayAreaInfo display, VideoPlaybackService playback, string? videoPath)
    {
        Title = "BackDrop";

        var view = new LockOverlayView(playback, videoPath);
        view.Dismissed += (_, _) => Dismissed?.Invoke(this, EventArgs.Empty);
        Content = view;

        Activate();

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }

        AppWindow.Move(new PointInt32(display.Bounds.X, display.Bounds.Y));
        AppWindow.Resize(new SizeInt32(display.Bounds.Width, display.Bounds.Height));
        AppWindow.IsShownInSwitchers = false;
    }
}
