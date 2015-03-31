using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AR.Drone.Client;
using AR.Drone.WinApp;
using AR.Drone.MyTool;
using System.Windows.Interop;
using AR.Drone.Infrastructure;

namespace AR.Drone.Multi
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<DroneClient> _drones;
        public MainWindow()
        {
            /**
             * 判断程序的运行平台
             * 根据不同的运行平台选择对应的FFmpeg库文件
             */
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:

                    //此处为为FFmpeg库文件的地址
                    //string ffmpegPath = string.Format(@"FFmpeg/bin/windows/{0}", Environment.Is64BitProcess ? "x64" : "x86");
                    string ffmpegPath = string.Format(@"../../../FFmpeg/bin/windows/{0}", Environment.Is64BitProcess ? "x64" : "x86");

                    InteropHelper.RegisterLibrariesSearchPath(ffmpegPath);
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    string libraryPath = Environment.GetEnvironmentVariable(InteropHelper.LD_LIBRARY_PATH);
                    InteropHelper.RegisterLibrariesSearchPath(libraryPath);
                    break;
            }

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            _drones = new List<DroneClient>();
            InitializeComponent();
        }

        private void btn_link_Click(object sender, RoutedEventArgs e)
        {
            string host = tb_ip.Text;
            if (host.Equals(""))
            {
                DroneClient drone = new DroneClient();
                MainForm form = new MainForm(drone);
                drone.Start();
                _drones.Add(drone);

                WindowInteropHelper helper = new WindowInteropHelper(this);

                form.Show(new WindowWrapper(helper.Handle));

            }
            else
            {
                DroneClient drone = new DroneClient(host);
                MainForm form = new MainForm(drone);
                drone.Start();
                _drones.Add(drone);

                WindowInteropHelper helper = new WindowInteropHelper(this);

                form.Show(new WindowWrapper(helper.Handle));
            }
        }
    }
}
