using BackDrop.Core.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;

namespace BackDrop.App.Views;

public sealed partial class LockOverlayView : UserControl
{
    private readonly VideoPlaybackService _playback;
    private readonly string? _videoPath;
    private readonly PinManager _pinManager;
    private bool _pinUnlocked;

    /// <summary>Raised when the user dismisses the lock (or enters the correct PIN).</summary>
    public event EventHandler? Dismissed;

    public LockOverlayView(VideoPlaybackService playback, string? videoPath)
    {
        _playback = playback;
        _videoPath = videoPath;
        _pinManager = new PinManager(AppServices.Settings);

        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = AppServices.Settings.Settings;

        // Seamless hardware-accelerated loop.
        var player = _playback.CreateLoopingPlayer(_videoPath, settings.MuteVideo);
        MediaElement.SetMediaPlayer(player);

        Vignette.Intensity = settings.VignetteIntensity;
        Clock.ApplyLayout(settings.ActiveLayout, settings.ShowSeconds);

        // If a PIN is configured, hide the hint and require it before dismissal.
        if (settings.Pin is not null)
        {
            HintText.Visibility = Visibility.Collapsed;
            PinPanel.Visibility = Visibility.Visible;
            PinBox.Focus(FocusState.Programmatic);
        }
        else
        {
            Focus(FocusState.Programmatic);
        }

        FadeIn();
    }

    private void FadeIn()
    {
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(650)),
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 2.2 },
        };
        Storyboard.SetTarget(animation, RootGrid);
        Storyboard.SetTargetProperty(animation, "Opacity");
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    // ---- Dismissal ----

    private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Let the PIN box consume its own keystrokes (reference compare, not value).
        if (PinPanel.Visibility == Visibility.Visible && ReferenceEquals(e.OriginalSource, PinBox))
            return;

        TryDismiss();
    }

    private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e) => TryDismiss();

    private void RootGrid_PointerMoved(object sender, PointerRoutedEventArgs e) => TryDismiss();

    private void TryDismiss()
    {
        if (_pinManager.IsEnabled && !_pinUnlocked)
        {
            PinPanel.Visibility = Visibility.Visible;
            PinBox.Focus(FocusState.Programmatic);
            return;
        }

        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    // ---- PIN gate ----

    private void PinBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
            VerifyPin();
        e.Handled = true;
    }

    private void UnlockButton_Click(object sender, RoutedEventArgs e) => VerifyPin();

    private void VerifyPin()
    {
        if (_pinManager.Verify(PinBox.Password))
        {
            _pinUnlocked = true;
            PinPanel.Visibility = Visibility.Collapsed;
            Dismissed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            PinErrorText.Visibility = Visibility.Visible;
            PinBox.SelectAll();
            PinBox.Focus(FocusState.Programmatic);
        }
    }
}
