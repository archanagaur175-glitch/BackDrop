using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Controls;

/// <summary>
/// A rounded card. Child content is forwarded to the inner ContentPresenter through
/// a property-changed callback on the Content DP — robust at XAML parse time, unlike
/// an x:Bind to the UserControl's own inherited Content property (WinUI 3 parse
/// fragility) or a RelativeSource AncestorType binding (WPF-only, throws at runtime).
/// </summary>
public sealed partial class AcrylicPanel : UserControl
{
    public AcrylicPanel()
    {
        InitializeComponent();

        // Content is assigned by the parent's XAML parser AFTER this constructor
        // runs, so forward it reactively instead of reading it once here.
        RegisterPropertyChangedCallback(ContentProperty, OnContentPropertyChanged);
    }

    private void OnContentPropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        ContentHost.Content = ((AcrylicPanel)sender).Content;
    }
}
