namespace BackDrop.Core.Services;

/// <summary>
/// Parses and formats hotkey strings like "Ctrl+Alt+L" into RegisterHotKey
/// modifiers + virtual key codes.
/// </summary>
public static class HotkeyParser
{
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    public static (uint Modifiers, uint VirtualKey) Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new FormatException("Hotkey is empty.");

        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            throw new FormatException("Hotkey must include at least one modifier and one key, e.g. Ctrl+Alt+L.");

        uint modifiers = 0;
        foreach (var part in parts[..^1])
        {
            modifiers |= part.ToUpperInvariant() switch
            {
                "CTRL" or "CONTROL" => MOD_CONTROL,
                "ALT" => MOD_ALT,
                "SHIFT" => MOD_SHIFT,
                "WIN" or "WINDOWS" => MOD_WIN,
                _ => throw new FormatException($"Unknown modifier '{part}'."),
            };
        }

        var key = parts[^1].ToUpperInvariant();
        var virtualKey = key switch
        {
            "L" => 0x4Cu,
            "SPACE" => 0x20u,
            "ENTER" or "RETURN" => 0x0Du,
            "ESC" or "ESCAPE" => 0x1Bu,
            "TAB" => 0x09u,
            "BACKSPACE" => 0x08u,
            "DELETE" => 0x2Eu,
            "HOME" => 0x24u,
            "END" => 0x23u,
            "PGUP" or "PAGEUP" => 0x21u,
            "PGDN" or "PAGEDOWN" => 0x22u,
            "UP" => 0x26u,
            "DOWN" => 0x28u,
            "LEFT" => 0x25u,
            "RIGHT" => 0x27u,
            "INSERT" => 0x2Du,
            _ when key.Length == 1 && char.IsAsciiLetterOrDigit(key[0]) => (uint)key[0],
            _ when key.StartsWith('F') && int.TryParse(key[1..], out var fn) && fn is >= 1 and <= 24 => (uint)(0x6F + fn),
            _ => throw new FormatException($"Unsupported key '{key}'."),
        };

        if (modifiers == 0)
            throw new FormatException("Hotkey must include at least one modifier, e.g. Ctrl+Alt+L.");

        return (modifiers, virtualKey);
    }

    public static string Format(uint modifiers, uint virtualKey)
    {
        var parts = new List<string>();
        if ((modifiers & MOD_CONTROL) != 0) parts.Add("Ctrl");
        if ((modifiers & MOD_ALT) != 0) parts.Add("Alt");
        if ((modifiers & MOD_SHIFT) != 0) parts.Add("Shift");
        if ((modifiers & MOD_WIN) != 0) parts.Add("Win");
        parts.Add(KeyName(virtualKey));
        return string.Join("+", parts);
    }

    private static string KeyName(uint vk) => vk switch
    {
        0x20 => "Space",
        0x0D => "Enter",
        0x1B => "Esc",
        0x09 => "Tab",
        0x08 => "Backspace",
        0x2E => "Delete",
        0x24 => "Home",
        0x23 => "End",
        0x21 => "PageUp",
        0x22 => "PageDown",
        0x26 => "Up",
        0x28 => "Down",
        0x25 => "Left",
        0x27 => "Right",
        0x2D => "Insert",
        >= 0x41 and <= 0x5A => ((char)vk).ToString(),
        >= 0x30 and <= 0x39 => ((char)vk).ToString(),
        >= 0x70 and <= 0x87 => $"F{vk - 0x6F}",
        _ => $"0x{vk:X2}",
    };
}
