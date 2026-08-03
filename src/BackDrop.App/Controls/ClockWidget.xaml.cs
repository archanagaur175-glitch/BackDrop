using BackDrop.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Controls;

public sealed partial class ClockWidget : UserControl
{
    private readonly DispatcherTimer _timer;
    private bool _showSeconds;

    public ClockWidget()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => UpdateClock();
        _timer.Start();
        UpdateClock();
    }

    /// <summary>Applies the chosen layout preset: placement + typography.</summary>
    public void ApplyLayout(LayoutKind layout, bool showSeconds)
    {
        _showSeconds = showSeconds;

        switch (layout)
        {
            case LayoutKind.ClassicBottomLeft:
                ClockPanel.HorizontalAlignment = HorizontalAlignment.Left;
                ClockPanel.VerticalAlignment = VerticalAlignment.Bottom;
                ClockText.FontSize = 56;
                ClockText.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                DateText.FontSize = 18;
                break;

            case LayoutKind.BoldCinematic:
                ClockPanel.HorizontalAlignment = HorizontalAlignment.Left;
                ClockPanel.VerticalAlignment = VerticalAlignment.Bottom;
                ClockText.FontSize = 128;
                ClockText.FontWeight = Microsoft.UI.Text.FontWeights.Thin;
                DateText.FontSize = 28;
                DateText.CharacterSpacing = 180;
                break;

            default: // MinimalistCenter
                ClockPanel.HorizontalAlignment = HorizontalAlignment.Center;
                ClockPanel.VerticalAlignment = VerticalAlignment.Center;
                ClockText.FontSize = 96;
                ClockText.FontWeight = Microsoft.UI.Text.FontWeights.SemiLight;
                DateText.FontSize = 22;
                DateText.CharacterSpacing = 0;
                break;
        }

        UpdateClock();
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;
        ClockText.Text = now.ToString(_showSeconds ? "HH:mm:ss" : "HH:mm");
        DateText.Text = now.ToString("dddd, MMMM d");
    }
}
