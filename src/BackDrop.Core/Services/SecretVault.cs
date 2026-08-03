using System.Security.Cryptography;
using System.Text;

namespace BackDrop.Core.Services;

/// <summary>
/// Local secret store backed by DPAPI (System.Security.Cryptography.ProtectedData,
/// CurrentUser scope). Works identically for packaged and unpackaged apps — it
/// relies on the Windows user profile, not package identity.
/// </summary>
public static class SecretVault
{
    public static byte[] Protect(byte[] data) =>
        ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);

    public static byte[] Unprotect(byte[] data) =>
        ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);

    public static string ProtectString(string value) =>
        Convert.ToBase64String(Protect(Encoding.UTF8.GetBytes(value)));

    public static string UnprotectString(string value) =>
        Encoding.UTF8.GetString(Unprotect(Convert.FromBase64String(value)));
}
