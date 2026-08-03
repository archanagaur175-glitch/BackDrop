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
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Single-instance: a second launch exits immediately (a tray app must not stack).
        _instanceMutex = new Mutex(true, @"Local\BackDrop.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            Environment.Exit(0);
            return;
        }

        MainWindow = new MainWindow();
        MainWindow.Activate();
        MainWindow.AfterActivated();

        // Tray-first app: keep the settings window hidden until the user opens it.
        MainWindow.AppWindow.Hide();

        _lockController = new LockController();
        _lockController.Start();

        _tray = new TrayService(ShowMainWindow, EngageLock, ExitApplication);
        _tray.Initialize();
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
}
