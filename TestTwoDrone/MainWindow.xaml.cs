using AR.Drone.Client.Command;
using MyDrone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace TestTwoDrone
{
    public class TestViewItem : INotifyPropertyChanged
    {
        #region 参数改变时发送通知
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region 公开字段(用于ListView获取)
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                NotifyPropertyChanged("id");
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged("name");
            }
        }
        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
                NotifyPropertyChanged("ip");
            }
        }

        public bool IsConected
        {
            get { return drone.IsConnected; }
            set
            {
                NotifyPropertyChanged("IsConected");
            }
        }

        #endregion

        private int id;
        private string name;
        private string ip;
        public MyDroneClient drone;
        public Nav
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 成员变量（测试使用）
        //常量 100毫微秒 与 1毫秒的差
        private const long MSEC = 10000;
        //无人机容器变量用于绑定数据
        private ObservableCollection<TestViewItem> _drones;
        //手动与自动控制的判断
        private bool _isAuto;
        private bool _isHand;

        private DispatcherTimer DelayTimer = new DispatcherTimer();
        #endregion


        public MainWindow()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);


            
            InitializeComponent();
            InitListView();
            InitVariable();
            InitTimer();

        }


        #region 初始化位置

        /// <summary>
        /// ListView 初始化函数
        /// </summary>
        /// <returns></returns>
        private void InitListView()
        {
            _drones = new ObservableCollection<TestViewItem>();
            lv_droneList.ItemsSource = _drones;
        }


        /// <summary>
        /// 程序使用到的一些变量的初始化
        /// </summary>
        /// <returns></returns>
        private void InitVariable()
        {
            //初始化默认两个按钮都没有选择
            _isAuto = false;
            _isHand = false;
        }

        private void InitTimer()
        {
            DelayTimer.Tick += new EventHandler(dTimer_Tick);
            DelayTimer.Interval = new TimeSpan(2 * MSEC);
            DelayTimer.Start();
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            //停止与飞机的连接
            foreach (TestViewItem t in _drones)
            {
                t.drone.Stop();
                t.drone.Join();
            }
            DelayTimer.Stop();

            base.OnClosed(e);
        }


        //连接按钮点击响应事件
        //判断文本框中的IP地址合法
        private void bt_link_Click(object sender, RoutedEventArgs e)
        {
            string[] format = new string[4];
            try
            {

                string s = ".";
                format = tb_ip.Text.Split(s.ToCharArray());
                for (int i = 0; i < 4; i++)
                {
                    int temp = Convert.ToInt32(format[i]);
                    if (temp >= 255 || temp < 0)
                    {
                        MessageBox.Show("IP地址非法", "错误", MessageBoxButton.OK);
                        return;
                    }
                }
            }
            catch
            {
                MessageBox.Show("IP地址非法", "错误", MessageBoxButton.OK);
                return;
            }

            TestViewItem viewItem = new TestViewItem();
            viewItem.ID = Convert.ToInt32(format[3]);
            viewItem.Name = format[3] + "号飞机";
            viewItem.IP = tb_ip.Text;
            viewItem.drone = new MyDroneClient(viewItem.IP);
            //绑定数据
            viewItem.drone.Start();
            _drones.Add(viewItem);

        }

        private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                lv_droneList.SelectedIndex = -1;
            }
        }
        //
        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Stop();
                _drones.RemoveAt(lv_droneList.SelectedIndex);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        #region 键盘控制
        //当进入手动控制模式时，监听键盘按键
        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            // MessageBox.Show(e.Key + "");
            //手动控制监控键盘输入
            if (_isHand)
            {
                Trace.WriteLine("接收键盘按键");
               
                //确认已选择飞机
                if (!_isDroneSelect)
                {
                    MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                    return;
                }

                //确认已连接飞机
                if (!_drones[lv_droneList.SelectedIndex].IsConected)
                {
                    return;
                }

                //判断按键
                switch (e.Key)
                {
                    case Key.T:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Takeoff();
                            break;
                        }
                    case Key.H:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Hover();
                            break;
                        }
                    case Key.G:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Land();
                            break;
                        }
                    case Key.I:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, pitch: -0.1f);
                            break;
                        }
                    case Key.K:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, pitch: 0.1f);
                            break;
                        }
                    case Key.J:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, roll: -0.1f);
                            break;
                        }
                    case Key.L:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, roll: 0.1f);
                            break;
                        }
                    case Key.W:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, gaz: 0.25f);
                            break;
                        }
                    case Key.S:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, gaz: -0.25f);
                            break;
                        }
                    case Key.D:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, yaw:-0.25f);
                            break;
                        }
                    case Key.A:
                        {
                            _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, yaw: 0.25f);
                            break;
                        }
                       
                    default:
                        break;
                }

                e.Handled = true;
            }

        }

        private void Window_KeyUp_1(object sender, KeyEventArgs e)
        {
            if (_isHand)
            {
                e.Handled = true;
                Trace.WriteLine("按键抬起");

                if (!_isDroneSelect)
                {
                    MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                    return;
                }

                //确认已连接飞机
                if (!_drones[lv_droneList.SelectedIndex].IsConected)
                {
                    return;
                }

                _drones[lv_droneList.SelectedIndex].drone.Hover();
            }
        }
        #endregion

        private void btn_hand_control_Click(object sender, RoutedEventArgs e)
        {
            btn_hand_control.Foreground = Brushes.Red;
            btn_auto_control.Foreground = Brushes.Black;
            _isHand = true;
            _isAuto = false;

            //按钮可用
            btn_hand_takeoff.IsEnabled = true;
            btn_hand_land.IsEnabled = true;
            btn_hand_hover.IsEnabled = true;
            btn_hand_left.IsEnabled = true;
            btn_hand_forward.IsEnabled = true;
            btn_hand_right.IsEnabled = true;
            btn_hand_back.IsEnabled = true;
            btn_hand_yawleft.IsEnabled = true;
            btn_hand_yawright.IsEnabled = true;
            btn_hand_up.IsEnabled = true;
            btn_hand_down.IsEnabled = true;
            //按钮不可用
        }

        private void btn_auto_control_Click(object sender, RoutedEventArgs e)
        {
            btn_hand_control.Foreground = Brushes.Black;
            btn_auto_control.Foreground = Brushes.Red;
            _isHand = false;
            _isAuto = true;
            //按钮可用

            //按钮不可用
            btn_hand_takeoff.IsEnabled = false;
            btn_hand_land.IsEnabled = false;
            btn_hand_hover.IsEnabled = false;
            btn_hand_left.IsEnabled = false;
            btn_hand_forward.IsEnabled = false;
            btn_hand_right.IsEnabled = false;
            btn_hand_back.IsEnabled = false;
            btn_hand_yawleft.IsEnabled = false;
            btn_hand_yawright.IsEnabled = false;
            btn_hand_up.IsEnabled = false;
            btn_hand_down.IsEnabled = false;
        }

        #region 被调用的公共函数
        private bool _isDroneSelect { get { return -1 != lv_droneList.SelectedIndex; } }
        #endregion


        #region 手动控制按钮响应事件
        private void btn_hand_takeoff_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Takeoff();
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_land_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Land();
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_hover_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Hover();
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_forward_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, pitch: -0.05f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_left_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, roll: -0.05f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_right_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, roll: 0.05f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_back_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, pitch: 0.05f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_up_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, gaz: 0.25f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_down_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, gaz: -0.25f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_yawleft_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, yaw: 0.25f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        private void btn_hand_yawright_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, yaw: -0.25f);
            }
            else
            {
                MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
            }
        }

        #endregion

        #region 定时器响应函数
        private void dTimer_Tick(object sender, EventArgs e)
        {
            if (_drones.Count != 0)
            {
                _drones[0].IsConected = true;
            }
        }
        #endregion

    }
}
