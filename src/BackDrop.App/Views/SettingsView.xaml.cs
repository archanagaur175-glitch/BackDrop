using BackDrop.App.ViewModels;
using BackDrop.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace BackDrop.App.Views;

public sealed partial class SettingsView : UserControl
{
    private bool _syncingPinToggle;

    public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

    public SettingsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>Hands the window handle to the media manager for the file picker.</summary>
    public void Initialize(IntPtr windowHandle) => MediaManager.Initialize(windowHandle);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Select the saved layout radio.
        var layout = ViewModel.Layout;
        if (layout == nameof(LayoutKind.BoldCinematic))
            LayoutCinematicRadio.IsChecked = true;
        else if (layout == nameof(LayoutKind.ClassicBottomLeft))
            LayoutClassicRadio.IsChecked = true;
        else
            LayoutMinimalistRadio.IsChecked = true;

        if (Application.Current.Resources["EntranceFadeInStoryboard"] is Storyboard storyboard)
        {
            Storyboard.SetTarget(storyboard, RootGrid);
            storyboard.Begin();
        }
    }

    private void Layout_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { IsChecked: true } radio && radio.Tag is string tag)
            ViewModel.SetLayout(tag);
    }

    private void ApplyHotkey_Click(object sender, RoutedEventArgs e) => ViewModel.ApplyHotkey();

    private void PinToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_syncingPinToggle)
            return;

        if (PinToggle.IsOn)
        {
            // Toggling on is only meaningful once a PIN exists — guide the user
            // and snap the toggle back off so the UI never lies about state.
            if (!ViewModel.PinEnabled)
            {
                ViewModel.PinStatus = "Enter a 4–12 digit PIN and press Set PIN.";
                SyncPinToggle();
            }
        }
        else
        {
            ViewModel.ClearPin();
        }
    }

    private void SetPin_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetPin(PinPasswordBox.Password);
        PinPasswordBox.Password = string.Empty;
        SyncPinToggle();
    }

    private void RemovePin_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearPin();
        SyncPinToggle();
    }

    /// <summary>Syncs the toggle to real PIN state without re-entering <see cref="PinToggle_Toggled"/>.</summary>
    private void SyncPinToggle()
    {
        _syncingPinToggle = true;
        PinToggle.IsOn = ViewModel.PinEnabled;
        _syncingPinToggle = false;
    }
}
