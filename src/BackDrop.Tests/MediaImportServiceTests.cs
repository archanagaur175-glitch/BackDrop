using BackDrop.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackDrop.Tests;

[TestClass]
public class MediaImportServiceTests
{
    /// <summary>Writable temp file path; the BackDrop.Tests dir may not exist on a fresh runner.</summary>
    private static string TempFilePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "BackDrop.Tests");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, Guid.NewGuid().ToString("N") + ".mp4");
    }

    [TestMethod]
    [DataRow(@"C:\videos\loop.mp4", true)]
    [DataRow(@"C:\videos\loop.MP4", true)]
    [DataRow(@"C:\videos\loop.mkv", true)]
    [DataRow(@"C:\videos\loop.webm", true)]
    [DataRow(@"C:\videos\loop.gif", false)]
    [DataRow(@"C:\videos\loop", false)]
    [DataRow("", false)]
    public void IsSupportedExtension_Matches(string path, bool expected)
    {
        Assert.AreEqual(expected, MediaImportService.IsSupportedExtension(path));
    }

    [TestMethod]
    public void IsValidFile_MissingFile_ReturnsFalse()
    {
        Assert.IsFalse(MediaImportService.IsValidFile(@"C:\definitely\missing\loop.mp4"));
    }

    [TestMethod]
    public void IsValidFile_EmptyFile_ReturnsFalse()
    {
        var temp = TempFilePath();
        File.WriteAllBytes(temp, Array.Empty<byte>());
        try
        {
            Assert.IsFalse(MediaImportService.IsValidFile(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [TestMethod]
    public void IsValidFile_RealFile_ReturnsTrue()
    {
        var temp = TempFilePath();
        File.WriteAllBytes(temp, new byte[1024]);
        try
        {
            Assert.IsTrue(MediaImportService.IsValidFile(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [TestMethod]
    public void Import_InvalidPath_ReturnsNull()
    {
        var service = new MediaImportService();
        Assert.IsNull(service.Import(@"C:\missing\loop.mp4"));
    }

    [TestMethod]
    public void Import_ValidFile_CreatesItem()
    {
        var temp = TempFilePath();
        File.WriteAllBytes(temp, new byte[2048]);
        try
        {
            var service = new MediaImportService();
            var item = service.Import(temp);

            Assert.IsNotNull(item);
            Assert.AreEqual("2.0 KB", item!.DisplaySize);
            Assert.AreEqual(Path.GetFileName(temp), item.Name);
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
