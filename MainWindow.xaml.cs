using IsaacEntityScannerRE.Services;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IsaacEntityScannerRE;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private SharedMemoryManager _shm;
    private PickupTracker _tracker;
    private ItemDatabase _db;
    private UIManager _ui;

    private DispatcherTimer _timer;
    private ProcessLauncher _launcher = new();

    private static string jsonPath = @"X:\Bezplatformowe\The Binding of Isaac Repentance\items_json.json";
    private static string injectorPath = @"Y:\VSProjects\IsaacInjector\Debug\IsaacInjector.exe";

    //private ProcessInjector _injector = new(); OLD PROCESS INJECTOR

    public MainWindow()
    {
        InitializeComponent();

        /* OLD PROCESS INJECTOR
        bool ok = _injector.Inject("isaac-ng.exe",
        @"Y:\VSProjects\IsaacEntityHook\Debug\IsaacEntityHook.dll");
        Output(ok ? "DLL injected" : "Injection failed");*/

        // 1. start injector EXE
        bool ok = _launcher.Launch(injectorPath);

        // 2. wait for DLL / shared memory (na razie brute-force)
        System.Threading.Thread.Sleep(10000);

        // 3. init shared memory
        _shm = new SharedMemoryManager();

        bool connected = false;

        for (int i = 0; i < 20; i++)
        {
            if (_shm.TryInit())
            {
                connected = true;
                break;
            }

            Thread.Sleep(500);
        }

        if (!connected)
        {
            MessageBox.Show("Failed to connect to shared memory.");
            Close();
            return;
        }

        // 4. init database
        _db = new ItemDatabase(jsonPath);

        // 5. UI manager (format + output)
        _ui = new UIManager(_db, BodyPanel, MainScrollViewer);
        Seen_Button.Click += (_, __) => _ui.SetMode(ViewMode.Seen);
        Recent_Button.Click += (_, __) => _ui.SetMode(ViewMode.Recent);

        // 6. tracker (core logic)
        _tracker = new PickupTracker();
        _tracker.OnUpdated += _ui.OnPickupUpdated;

        // 7. main loop
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(200);
        _timer.Tick += Tick;
        _timer.Start();
    }

    private void Tick(object sender, EventArgs e)
    {
        if (!_shm.HasNewData())
            return;

        var entities = _shm.ReadNew();

        _tracker.Update(entities);
    }
}