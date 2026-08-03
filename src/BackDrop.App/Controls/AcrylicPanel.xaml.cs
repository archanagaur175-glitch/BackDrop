using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Controls;

/// <summary>
/// A rounded glass card. Child content is forwarded to the inner ContentPresenter
/// via a RelativeSource binding on Content — WinUI 3's ContentControl exposes no
/// OnContentChanged override, so the binding is the supported forwarding mechanism.
/// </summary>
public sealed partial class AcrylicPanel : UserControl
{
    public AcrylicPanel()
    {
        InitializeComponent();
    }
}
