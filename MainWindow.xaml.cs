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

        LaunchInjector();

        if (!InitSharedMemory())
            return;

        InitDatabase();
        InitUI();
        InitTracker();
        InitTimer();
    }

    private void LaunchInjector()
    {
        _launcher.Launch(injectorPath);
    }

    private bool InitSharedMemory()
    {
        _shm = new SharedMemoryManager();

        for (int i = 0; i < 20; i++)
        {
            if (_shm.TryInit())
                return true;

            Thread.Sleep(500);
        }

        MessageBox.Show("Failed to connect to shared memory.");

        Close();

        return false;
    }

    private void InitDatabase()
    {
        _db = new ItemDatabase(jsonPath);
    }

    private void InitUI()
    {
        _ui = new UIManager(_db, BodyPanel, MainScrollViewer);

        Seen_Button.Click += (_, __) => _ui.SetMode(ViewMode.Seen);
        Recent_Button.Click += (_, __) => _ui.SetMode(ViewMode.Recent);
    }

    private void InitTracker()
    {
        _tracker = new PickupTracker();

        _tracker.OnUpdated += _ui.OnPickupUpdated;
    }

    private void InitTimer()
    {
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