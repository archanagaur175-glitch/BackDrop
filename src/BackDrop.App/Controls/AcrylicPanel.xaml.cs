using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Controls;

/// <summary>
/// A rounded card. Content is provided through the ContentControl template's
/// ContentPresenter (auto-bound to Content), so `<controls:AcrylicPanel>` works
/// exactly like a Button — no code-behind forwarding required.
/// </summary>
public sealed partial class AcrylicPanel : ContentControl
{
    public AcrylicPanel()
    {
        InitializeComponent();
    }
}
