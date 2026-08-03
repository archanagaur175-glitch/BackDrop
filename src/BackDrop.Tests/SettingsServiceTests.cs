using BackDrop.Core.Models;
using BackDrop.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackDrop.Tests;

[TestClass]
public class SettingsServiceTests
{
    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "BackDrop.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [TestMethod]
    public void MissingFile_ReturnsDefaults()
    {
        var service = new SettingsService(NewTempDir());

        Assert.AreEqual("Ctrl+Alt+L", service.Settings.HotkeyText);
        Assert.IsFalse(service.Settings.StartWithWindows);
        Assert.IsNull(service.Settings.Pin);
        Assert.AreEqual(LayoutKind.MinimalistCenter, service.Settings.ActiveLayout);
    }

    [TestMethod]
    public void SaveThenReload_RoundTrips()
    {
        var dir = NewTempDir();
        var service = new SettingsService(dir);

        service.Settings.HotkeyText = "Ctrl+Shift+K";
        service.Settings.StartWithWindows = true;
        service.Settings.Layout = nameof(LayoutKind.BoldCinematic);
        service.Settings.VignetteIntensity = 0.8;
        service.Settings.MediaLibrary.Add(new MediaVideoItem
        {
            Name = "loop.mp4",
            FilePath = @"C:\videos\loop.mp4",
            SizeBytes = 5_000_000, // 5.0 MB (DisplaySize divides by 1,000,000)
        });
        service.Save();

        var reloaded = new SettingsService(dir);
        Assert.AreEqual("Ctrl+Shift+K", reloaded.Settings.HotkeyText);
        Assert.IsTrue(reloaded.Settings.StartWithWindows);
        Assert.AreEqual(LayoutKind.BoldCinematic, reloaded.Settings.ActiveLayout);
        Assert.AreEqual(0.8, reloaded.Settings.VignetteIntensity, 0.0001);
        Assert.AreEqual(1, reloaded.Settings.MediaLibrary.Count);
        Assert.AreEqual("loop.mp4", reloaded.Settings.MediaLibrary[0].Name);
        Assert.AreEqual("5.0 MB", reloaded.Settings.MediaLibrary[0].DisplaySize);
    }

    [TestMethod]
    public void CorruptJson_FallsBackToDefaults()
    {
        var dir = NewTempDir();
        File.WriteAllText(Path.Combine(dir, "settings.json"), "{ not valid json !!");

        var service = new SettingsService(dir);

        Assert.AreEqual("Ctrl+Alt+L", service.Settings.HotkeyText);
        Assert.IsNull(service.Settings.Pin);
    }

    [TestMethod]
    public void ThumbnailDirectory_IsUnderBaseDirectory()
    {
        var dir = NewTempDir();
        var service = new SettingsService(dir);
        Assert.AreEqual(Path.Combine(dir, "Thumbnails"), service.ThumbnailDirectory);
    }
}
