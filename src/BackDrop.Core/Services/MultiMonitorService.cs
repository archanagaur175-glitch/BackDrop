using System.Runtime.InteropServices;

namespace BackDrop.Core.Services;

/// <summary>Pixel bounds of a display (X, Y = top-left origin).</summary>
public sealed record DisplayBounds(int X, int Y, int Width, int Height);

/// <summary>Physical-pixel bounds of one display.</summary>
public sealed record DisplayAreaInfo(string Id, DisplayBounds Bounds, DisplayBounds WorkArea, bool IsPrimary);

/// <summary>
/// Enumerates physical displays via user32 (EnumDisplayMonitors/GetMonitorInfo).
/// Zero external dependencies — works in any project, unlike the Windows App
/// SDK's Windows.Graphics.DisplayArea which requires a WASDK reference.
/// </summary>
public sealed class MultiMonitorService
{
    public IReadOnlyList<DisplayAreaInfo> GetDisplays()
    {
        var result = new List<DisplayAreaInfo>();

        EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (hMonitor, _, _, _) =>
            {
                var info = new MONITORINFOEX { cbSize = (uint)Marshal.SizeOf<MONITORINFOEX>() };
                if (GetMonitorInfo(hMonitor, ref info))
                {
                    result.Add(new DisplayAreaInfo(
                        new string(info.szDevice).TrimEnd('\0'),
                        ToBounds(info.rcMonitor),
                        ToBounds(info.rcWork),
                        (info.dwFlags & MONITORINFOF_PRIMARY) != 0));
                }
                return true;
            },
            IntPtr.Zero);

        return result;
    }

    private static DisplayBounds ToBounds(RECT r) => new(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);

    private const int MONITORINFOF_PRIMARY = 0x1;

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MONITORINFOEX
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
}
