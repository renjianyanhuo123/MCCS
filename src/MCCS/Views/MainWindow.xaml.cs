using MahApps.Metro.Controls;
using System.Windows.Interop;
using System.Windows;

namespace MCCS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed; 
        } 

        private const int WmSyscommand = 0x0112;
        private const int ScMinimize = 0xF020;

        private HwndSource? _hwndSource; 

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(hwnd);
            if (_hwndSource != null) _hwndSource.AddHook(WndProc);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _hwndSource?.RemoveHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmSyscommand)
            {
                var command = wParam.ToInt32() & 0xFFF0;

                if (command == ScMinimize)
                {
                    // 阻止最小化
                    handled = true;
                    return IntPtr.Zero;
                }
            }

            return IntPtr.Zero;
        }
    }
}