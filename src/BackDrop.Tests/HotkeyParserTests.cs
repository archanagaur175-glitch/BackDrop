using BackDrop.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BackDrop.Tests;

[TestClass]
public class HotkeyParserTests
{
    [TestMethod]
    public void Parse_DefaultCtrlAltL()
    {
        var (modifiers, virtualKey) = HotkeyParser.Parse("Ctrl+Alt+L");

        Assert.AreEqual(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, modifiers);
        Assert.AreEqual(0x4C, virtualKey);
    }

    [TestMethod]
    public void Parse_CaseInsensitiveAndSpaces()
    {
        var (modifiers, virtualKey) = HotkeyParser.Parse(" ctrl + shift + f5 ");

        Assert.AreEqual(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_SHIFT, modifiers);
        Assert.AreEqual(0x74, virtualKey); // F5 = VK_F5
    }

    [TestMethod]
    public void Parse_AllModifiers()
    {
        var (modifiers, virtualKey) = HotkeyParser.Parse("Ctrl+Alt+Shift+Win+Space");

        Assert.AreEqual(
            HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT | HotkeyParser.MOD_SHIFT | HotkeyParser.MOD_WIN,
            modifiers);
        Assert.AreEqual(0x20, virtualKey);
    }

    [TestMethod]
    public void Format_RoundTrips()
    {
        var (modifiers, virtualKey) = HotkeyParser.Parse("Ctrl+Alt+L");
        var formatted = HotkeyParser.Format(modifiers, virtualKey);
        var (modifiers2, virtualKey2) = HotkeyParser.Parse(formatted);

        Assert.AreEqual(modifiers, modifiers2);
        Assert.AreEqual(virtualKey, virtualKey2);
        Assert.AreEqual("Ctrl+Alt+L", formatted);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("L")]
    [DataRow("Ctrl+")]
    [DataRow("Ctrl+Alt")]
    [DataRow("Foo+Bar")]
    [DataRow("Ctrl+L+Alt")]
    [DataRow("Ctrl+UnknownKey")]
    public void Parse_Invalid_Throws(string text)
    {
        Assert.ThrowsExactly<FormatException>(() => HotkeyParser.Parse(text));
    }
}
