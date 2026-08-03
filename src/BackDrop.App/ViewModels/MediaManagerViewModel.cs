using System.Collections.ObjectModel;
using BackDrop.App.Common;
using BackDrop.Core.Models;
using BackDrop.Core.Services;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BackDrop.App.ViewModels;

public sealed class MediaManagerViewModel : ObservableObject
{
    private readonly SettingsService _settings = AppServices.Settings;
    private readonly MediaImportService _import = new();

    private MediaVideoItem? _selectedItem;
    private string _status = string.Empty;

    public MediaManagerViewModel()
    {
        Items = new ObservableCollection<MediaVideoItem>(_settings.Settings.MediaLibrary);
    }

    public ObservableCollection<MediaVideoItem> Items { get; }

    public MediaVideoItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string ActiveVideoLabel
    {
        get
        {
            var path = _settings.Settings.ActiveVideoPath;
            return string.IsNullOrEmpty(path) ? "Active: bundled default loop" : $"Active: {Path.GetFileName(path)}";
        }
    }

    public async Task ImportAsync(IntPtr windowHandle)
    {
        var picker = new FileOpenPicker();
        foreach (var extension in MediaImportService.SupportedExtensions)
            picker.FileTypeFilter.Add(extension);
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        InitializeWithWindow.Initialize(picker, windowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file is null)
            return;

        if (!MediaImportService.IsValidFile(file.Path))
        {
            Status = "Unsupported file type. Supported: " + string.Join(", ", MediaImportService.SupportedExtensions);
            return;
        }

        Status = "Checking playability…";
        if (!await _import.IsPlayableAsync(file.Path))
        {
            Status = "That file could not be decoded. Try an H.264 MP4.";
            return;
        }

        var item = _import.Import(file.Path);
        if (item is null)
        {
            Status = "Import failed.";
            return;
        }

        _settings.Settings.MediaLibrary.Add(item);
        _settings.Save();
        Items.Add(item);
        SelectedItem = item;
        Status = $"Added {item.Name}.";
    }

    public void SetActive()
    {
        if (SelectedItem is null)
        {
            Status = "Select a video first.";
            return;
        }

        _settings.Settings.ActiveVideoPath = SelectedItem.FilePath;
        _settings.Save();
        OnPropertyChanged(nameof(ActiveVideoLabel));
        Status = $"Active loop set to {SelectedItem.Name}.";
    }

    public void RemoveSelected()
    {
        if (SelectedItem is null)
        {
            Status = "Select a video first.";
            return;
        }

        var item = SelectedItem;
        _settings.Settings.MediaLibrary.Remove(item);
        if (_settings.Settings.ActiveVideoPath == item.FilePath)
            _settings.Settings.ActiveVideoPath = null;
        _settings.Save();

        Items.Remove(item);
        SelectedItem = null;
        OnPropertyChanged(nameof(ActiveVideoLabel));
        Status = $"Removed {item.Name}.";
    }
}
