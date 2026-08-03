using System.Security.Cryptography;
using System.Text;
using BackDrop.Core.Models;

namespace BackDrop.Core.Services;

/// <summary>
/// Local, opt-in PIN gate. The PIN itself is never stored: a random salt plus a
/// SHA-256 digest is wrapped with DPAPI into settings.json. Verification uses a
/// constant-time compare. Always recoverable via Settings → Remove PIN.
/// </summary>
public sealed class PinManager
{
    private const int SaltLength = 16;
    private readonly SettingsService _settings;

    public PinManager(SettingsService settings) => _settings = settings;

    public bool IsEnabled => _settings.Settings.Pin is not null;

    public void SetPin(string pin)
    {
        var normalized = Normalize(pin);
        if (normalized.Length is < 4 or > 12)
            throw new ArgumentException("PIN must be 4–12 digits.");

        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = ComputeHash(normalized, salt);
        var blob = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, blob, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, blob, salt.Length, hash.Length);

        _settings.Settings.Pin = new PinRecord { ProtectedBlob = SecretVault.ProtectString(Convert.ToBase64String(blob)) };
        _settings.Save();
    }

    public void ClearPin()
    {
        _settings.Settings.Pin = null;
        _settings.Save();
    }

    public bool Verify(string pin)
    {
        var record = _settings.Settings.Pin;
        if (record is null) return false;

        try
        {
            var blob = Convert.FromBase64String(SecretVault.UnprotectString(record.ProtectedBlob));
            if (blob.Length != SaltLength + 32)
                return false;

            var salt = blob[..SaltLength];
            var expected = blob[SaltLength..];
            var actual = ComputeHash(Normalize(pin), salt);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (CryptographicException)
        {
            // Blob unreadable (e.g. profile reset) — treated as not verified; recoverable via Settings.
            return false;
        }
    }

    private static byte[] ComputeHash(string pin, byte[] salt)
    {
        var pinBytes = Encoding.UTF8.GetBytes(pin);
        var combined = new byte[salt.Length + pinBytes.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(pinBytes, 0, combined, salt.Length, pinBytes.Length);
        return SHA256.HashData(combined);
    }

    private static string Normalize(string pin) => new(pin.Where(char.IsAsciiDigit).ToArray());
}
