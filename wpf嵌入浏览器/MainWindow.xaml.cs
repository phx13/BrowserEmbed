using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using winform主体;

namespace wpf嵌入浏览器
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.Panel _hostPanel;
        Process _process;
        ManualResetEvent _eventDone = new ManualResetEvent(false);
        public MainWindow()
        {
            InitializeComponent();
            _hostPanel = new System.Windows.Forms.Panel();
            win.Child = _hostPanel;
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartAndEmbedProcess("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe");
        }

        public Process StartApp(string exeName)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(exeName);
                info.UseShellExecute = true;
                _process = Process.Start(info);
                _process.WaitForInputIdle();
            }
            catch
            {

            }
            return _process;
        }

        public bool StartAndEmbedProcess(string processPath)
        {
            bool isStartAndEmbedSuccess = false;
            _eventDone.Reset();

            _process = StartApp(processPath);
            if (_process == null)
            {
                return false;
            }

            Thread thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (_process.MainWindowHandle != (IntPtr)0)
                    {
                        _eventDone.Set(); 
                        break;
                    }
                    Thread.Sleep(10);
                }
            }));
            thread.Start();

            if (_eventDone.WaitOne(10000))
            {
                isStartAndEmbedSuccess = EmbedApp(_process.MainWindowHandle);
            }
            
            return isStartAndEmbedSuccess;
        }

        private bool EmbedApp(IntPtr hwnd)
        {
            bool isEmbedSuccess = false;
            IntPtr processHwnd = hwnd;
            IntPtr panelHwnd = _hostPanel.Handle;

            if (processHwnd != (IntPtr)0 && panelHwnd != (IntPtr)0)
            {
                int setTime = 0;
                while (!isEmbedSuccess && setTime < 10)
                {
                    isEmbedSuccess = (SetParent(processHwnd, panelHwnd) != 0);
                    Thread.Sleep(100);
                    setTime++;
                }
                MoveWindow(_process.MainWindowHandle, 0, 0, (int)ActualWidth, (int)ActualHeight, true);
            }

            return isEmbedSuccess;
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
    }
}
