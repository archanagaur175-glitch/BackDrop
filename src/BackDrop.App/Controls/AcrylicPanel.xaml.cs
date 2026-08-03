using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Controls;

public sealed partial class AcrylicPanel : UserControl
{
    public AcrylicPanel()
    {
        InitializeComponent();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        ContentHost.Content = newContent;
    }
}
