using BackDrop.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BackDrop.App.Views;

public sealed partial class MediaManagerView : UserControl
{
    private IntPtr _windowHandle;

    public MediaManagerViewModel ViewModel { get; } = new MediaManagerViewModel();

    public MediaManagerView()
    {
        InitializeComponent();
    }

    public void Initialize(IntPtr windowHandle) => _windowHandle = windowHandle;

    private async void Import_Click(object sender, RoutedEventArgs e)
        => await ViewModel.ImportAsync(_windowHandle);

    private void SetActive_Click(object sender, RoutedEventArgs e) => ViewModel.SetActive();

    private void Remove_Click(object sender, RoutedEventArgs e) => ViewModel.RemoveSelected();
}
