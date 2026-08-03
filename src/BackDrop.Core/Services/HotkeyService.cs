using System.Runtime.InteropServices;

namespace BackDrop.Core.Services;

/// <summary>
/// Global hotkey via RegisterHotKey on a dedicated message-only window running
/// on its own STA thread. Fires regardless of the foreground application.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int WM_QUIT = 0x0012;

    private static readonly WndProcDelegate DefWndProc = (h, m, w, l) => DefWindowProc(h, m, w, l);

    private Thread? _thread;
    private IntPtr _hwnd = IntPtr.Zero;
    private bool _running;

    public event EventHandler? HotkeyPressed;

    public void Start(uint modifiers, uint virtualKey)
    {
        if (_running)
            throw new InvalidOperationException("HotkeyService is already running.");

        _running = true;
        _thread = new Thread(() => Run(modifiers, virtualKey))
        {
            IsBackground = true,
            Name = "BackDrop.HotkeyThread",
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    public void Dispose()
    {
        _running = false;
        if (_hwnd != IntPtr.Zero)
            PostMessage(_hwnd, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        _thread?.Join(TimeSpan.FromSeconds(2));
        _thread = null;
    }

    private void Run(uint modifiers, uint virtualKey)
    {
        _hwnd = CreateMessageOnlyWindow();
        if (_hwnd == IntPtr.Zero)
        {
            _running = false;
            return;
        }

        RegisterHotKey(_hwnd, 1, modifiers, virtualKey);

        while (_running && GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
        {
            if (msg.message == WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        UnregisterHotKey(_hwnd, 1);
        DestroyWindow(_hwnd);
        _hwnd = IntPtr.Zero;
    }

    private static IntPtr CreateMessageOnlyWindow()
    {
        var className = "BackDropHotkeyWindow";
        var hInstance = GetModuleHandle(null);
        var wc = new WNDCLASS
        {
            lpfnWndProc = DefWndProc,
            hInstance = hInstance,
            lpszClassName = className,
        };
        RegisterClass(ref wc);
        return CreateWindowEx(
            0, className, className, 0, 0, 0, 0, 0,
            new IntPtr(-3) /* HWND_MESSAGE */, IntPtr.Zero, hInstance, IntPtr.Zero);
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASS
    {
        public uint style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int ptX;
        public int ptY;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}
