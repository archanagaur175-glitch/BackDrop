namespace BackDrop.Core.Models;

/// <summary>
/// Local, opt-in PIN gate. Only a DPAPI-protected salt+hash blob is persisted —
/// never the raw PIN. Encrypted under the current Windows user profile.
/// </summary>
public sealed class PinRecord
{
    public string ProtectedBlob { get; set; } = string.Empty;
}
