namespace BackDrop.App;

/// <summary>
/// Best-effort logger for startup phases and unhandled exceptions.
/// A tray app that fails silently is undiagnosable; this file makes failures
/// visible by writing to %LocalAppData%\BackDrop\app.log. Never throws.
/// </summary>
public static class CrashLog
{
    private static readonly object Gate = new();

    private static string LogPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BackDrop",
        "app.log");

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never take the app down.
        }
    }
}
