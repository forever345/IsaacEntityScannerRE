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
    private DispatcherTimer _timer;
    private ProcessLauncher _launcher = new();

    //private ProcessInjector _injector = new(); OLD PROCESS INJECTOR

    public MainWindow()
    {
        InitializeComponent();

        Output("UI started...");

        /* OLD PROCESS INJECTOR
        bool ok = _injector.Inject("isaac-ng.exe",
        @"Y:\VSProjects\IsaacEntityHook\Debug\IsaacEntityHook.dll");
        Output(ok ? "DLL injected" : "Injection failed");*/

        bool ok = _launcher.Launch(@"Y:\VSProjects\IsaacInjector\Debug\IsaacInjector.exe");

        Output(ok ? "Injector EXE launched" : "Failed to launch injector");

        Thread.Sleep(10000);

        _shm = new SharedMemoryManager();
        _shm.Init();

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(200);
        _timer.Tick += Tick;
        _timer.Start();
    }

    public void Output(string text)
    {
        OutputBox.AppendText(text + "\n");
        OutputBox.ScrollToEnd();
    }

    private void Tick(object sender, EventArgs e)
    {
        _shm.ReadAll(entity =>
        {
            Output($"ptr={entity.ptr} type={entity.type} variant={entity.variant} id={entity.id}");
        });
    }
}