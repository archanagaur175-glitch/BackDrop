using BackDrop.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackDrop.Tests;

[TestClass]
public class SecretVaultTests
{
    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "BackDrop.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [TestMethod]
    public void ProtectUnprotect_RoundTrips()
    {
        var data = "correct horse battery staple"u8.ToArray();
        var protectedBytes = SecretVault.Protect(data);

        CollectionAssert.AreNotEqual(data, protectedBytes);
        CollectionAssert.AreEqual(data, SecretVault.Unprotect(protectedBytes));
    }

    [TestMethod]
    public void ProtectString_RoundTrips()
    {
        const string value = "4821";
        var protectedValue = SecretVault.ProtectString(value);

        Assert.AreNotEqual(value, protectedValue);
        Assert.AreEqual(value, SecretVault.UnprotectString(protectedValue));
    }

    [TestMethod]
    public void PinManager_SetVerifyClear()
    {
        var service = new SettingsService(NewTempDir());
        var pinManager = new PinManager(service);

        Assert.IsFalse(pinManager.IsEnabled);

        pinManager.SetPin("4821");
        Assert.IsTrue(pinManager.IsEnabled);
        Assert.IsTrue(pinManager.Verify("4821"));
        Assert.IsFalse(pinManager.Verify("0000"));
        Assert.IsFalse(service.Settings.Pin!.ProtectedBlob.Contains("4821"));

        pinManager.ClearPin();
        Assert.IsFalse(pinManager.IsEnabled);
        Assert.IsFalse(pinManager.Verify("4821"));
    }

    [TestMethod]
    [DataRow("123")]
    [DataRow("1234567890123")] // 13 digits
    [DataRow("abcd")]
    [DataRow("12a4")]
    [DataRow("")]
    public void PinManager_InvalidPins_Throw(string pin)
    {
        var service = new SettingsService(NewTempDir());
        var pinManager = new PinManager(service);

        Assert.ThrowsExactly<ArgumentException>(() => pinManager.SetPin(pin));
    }

    [TestMethod]
    public void PinManager_VerifyWithoutPin_ReturnsFalse()
    {
        var service = new SettingsService(NewTempDir());
        var pinManager = new PinManager(service);
        Assert.IsFalse(pinManager.Verify("4821"));
    }
}
