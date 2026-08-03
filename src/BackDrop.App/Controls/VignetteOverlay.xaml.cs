using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Controls;

public sealed partial class VignetteOverlay : UserControl
{
    public VignetteOverlay()
    {
        InitializeComponent();
    }

    /// <summary>0..1 — how strongly the frame darkens.</summary>
    public double Intensity
    {
        get => VignetteRoot.Opacity;
        set => VignetteRoot.Opacity = Math.Clamp(value, 0, 1);
    }
}
