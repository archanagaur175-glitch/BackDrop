using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace BackDrop.App;

public partial class App : Application
{
    /// <summary>Held for the lifetime of the process so the named mutex is never collected.</summary>
    private static Mutex? _instanceMutex;

    public static new App Current => (App)Application.Current;

    public MainWindow? MainWindow { get; private set; }

    private TrayService? _tray;
    private LockController? _lockController;

    public App()
    {
        // A tray-first app that dies silently is undiagnosable. Capture the
        // ORIGINAL exception (before the XAML runtime converts it into a
        // 0xc000027b stowed exception), log every unhandled exception, and show
        // a dialog instead of vanishing.
        AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
            CrashLog.Write($"First-chance: {e.Exception.GetType().Name}: {e.Exception.Message}");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            CrashLog.Write($"AppDomain unhandled: {e.ExceptionObject}");
        UnhandledException += (_, e) =>
        {
            CrashLog.Write($"Application unhandled: {e.Exception}");
            ShowStartupError(e.Exception);
            e.Handled = true; // we logged + told the user; avoid a second stowed crash
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashLog.Write($"Unobserved task exception: {e.Exception}");
            e.SetObserved();
        };

        try
        {
            InitializeComponent();
            CrashLog.Write("App.InitializeComponent OK.");
        }
        catch (Exception ex)
        {
            CrashLog.Write($"FATAL: App.xaml failed to parse: {ex}");
            ShowStartupError(ex);
            throw;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        CrashLog.Write("BackDrop OnLaunched: begin.");

        try
        {
            // Single-instance: a second launch exits immediately (a tray app must not stack).
            _instanceMutex = new Mutex(true, @"Local\BackDrop.SingleInstance", out var createdNew);
            if (!createdNew)
            {
                CrashLog.Write("Another instance is already running — exiting quietly.");
                Environment.Exit(0);
                return;
            }

            CrashLog.Write("OnLaunched: creating main window…");
            MainWindow = new MainWindow();
            MainWindow.Activate();
            MainWindow.AfterActivated();
            CrashLog.Write("OnLaunched: main window created + activated. (settings window shown on first launch)");

            CrashLog.Write("OnLaunched: starting lock controller (hotkey)…");
            _lockController = new LockController();
            try
            {
                _lockController.Start();
                CrashLog.Write("OnLaunched: hotkey service started.");
            }
            catch (Exception ex)
            {
                // A failed hotkey must not take the whole app down.
                CrashLog.Write($"OnLaunched: hotkey start failed (non-fatal): {ex}");
            }

            CrashLog.Write("OnLaunched: creating tray icon…");
            try
            {
                _tray = new TrayService(ShowMainWindow, EngageLock, ExitApplication);
                _tray.Initialize();
                CrashLog.Write("OnLaunched: tray icon created.");
            }
            catch (Exception ex)
            {
                // A failed tray icon must not take the whole app down either —
                // the settings window stays visible so the app is never invisible.
                CrashLog.Write($"OnLaunched: tray init failed (non-fatal): {ex}");
            }

            CrashLog.Write("BackDrop started OK.");
        }
        catch (Exception ex)
        {
            CrashLog.Write($"FATAL startup exception: {ex}");
            ShowStartupError(ex);
        }
    }

    public void ShowMainWindow()
    {
        if (MainWindow is null)
            return;
        MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            MainWindow.AppWindow.Show();
            MainWindow.Activate();
        });
    }

    public void EngageLock() => _lockController?.Engage();

    public void ExitApplication()
    {
        _tray?.Dispose();
        _lockController?.Dispose();
        Exit();
    }

    private static void ShowStartupError(Exception? ex)
    {
        try
        {
            _ = MessageBoxW(
                IntPtr.Zero,
                $"BackDrop failed to start.\n\n{ex}",
                "BackDrop — startup error",
                0x10 /* MB_ICONERROR */ | 0x40000 /* MB_TOPMOST */);
        }
        catch
        {
            // Nothing more we can do; the crash log has the details.
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
