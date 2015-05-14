using AR.Drone.Client;
using AR.Drone.Client.Command;
using AR.Drone.Data.Navigation;
using AR.Drone.MyTool;
using MyDrone;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

namespace TestTwoDrone
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ViconPositionXYZ
    {
        public double x;
        public double y;
        public double z;
    };


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
        private bool _isP2P;

        //定时器
        private DispatcherTimer DelayTimer = new DispatcherTimer();//更新界面信息每200ms一次
        private DispatcherTimer _p2pTimer = new DispatcherTimer();//点到点指令发送 200ms 一次


        private PositionClient _positionClient;
        //获取位置数据的线程
        private MyViconPosition _viconPositionGet;
        ViconPositionXYZ _p1; // 接收位置数据
        ViconPositionXYZ _p2;

        private int P2PTime;
        private float T;
        private float gate;

        //将测试数据写入文件
        //临时使用
        private FileStream _logFileStream;
        private StreamWriter _logWriteStream;

        //替换一行的正则表达式
        //Regex _onelineRegex = new Regex(@".*\r\n");
        //List<String> logs = new List<string>();

        private Key oldKey = Key.Back;

        #endregion


        public MainWindow()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);



            InitializeComponent();
            InitConfig();
            InitListView();
            InitVariable();
            InitTimer();
            InitTest();

        }


        #region 初始化位置

        private void InitConfig()
        {
            String tmp;
            tmp = ConfigurationManager.AppSettings["gate"];
            gate = float.Parse(tmp);
            tmp = ConfigurationManager.AppSettings["P2PTime"];
            P2PTime = int.Parse(tmp);
            T = P2PTime / 1000.0f;
        }

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

            //自制Vicon位置获取
            _positionClient = new PositionClient();
            _viconPositionGet = new MyViconPosition(_positionClient);
            _viconPositionGet.OnViconPositionRecieve += OnViconPositionRecieve;

            //测试文件初始化
            string file = string.Format(@"log_{0:yyyy_MM_dd_HH_mm}.txt", DateTime.Now);
            _logFileStream = new FileStream(System.Environment.CurrentDirectory + @"/" + file, FileMode.OpenOrCreate);
            _logWriteStream = new StreamWriter(_logFileStream);
        }

        /// <summary>
        /// 定时器初始化
        /// </summary>
        /// <returns></returns>
        private void InitTimer()
        {
            //界面更新用定时器
            DelayTimer.Tick += new EventHandler(dTimer_Tick);
            DelayTimer.Interval = new TimeSpan(200 * MSEC);
            DelayTimer.Start();

            //创建自动控制指令发送定时器
            _p2pTimer.Tick += new EventHandler(p2pTimer_Tick);
            _p2pTimer.Interval = new TimeSpan(P2PTime * MSEC);

        }

        private void InitTest()
        {
            //捆绑事件响应
            AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(Window_KeyDown_1), true);
            AddHandler(Keyboard.KeyUpEvent, new KeyEventHandler(Window_KeyUp_1), true);
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            RemoveHandler(Keyboard.KeyDownEvent, new KeyEventHandler(Window_KeyDown_1));
            RemoveHandler(Keyboard.KeyUpEvent, new KeyEventHandler(Window_KeyUp_1));
            //停止与飞机的连接
            foreach (TestViewItem t in _drones)
            {
                t.drone.Stop();
                t.drone.Join();
            }
            //停止定时器
            DelayTimer.IsEnabled = false;
            _p2pTimer.IsEnabled = false;

            //资源释放
            _logWriteStream.Write(tb_log.Text);
            _logWriteStream.Flush();

            _viconPositionGet.Dispose();

            _logWriteStream.Dispose();
            _logFileStream.Dispose();

            base.OnClosed(e);
        }

        #region 事件响应
        private unsafe void OnViconPositionRecieve()
        {
            _p1 = *(ViconPositionXYZ*)_positionClient.getPosition1();
            _p2 = *(ViconPositionXYZ*)_positionClient.getPosition2();
            //foreach (TestViewItem t in _drones)
            //{
            //    if (t.ID.Equals(13))
            //    {
            //        t.currentX = (float)_p1.x;
            //        t.currentY = (float)_p1.y;
            //        t.currentZ = (float)_p1.z;
            //    }
            //    else if (t.ID.Equals(16))
            //    {
            //        t.currentX = (float)_p2.x;
            //        t.currentY = (float)_p2.y;
            //        t.currentZ = (float)_p2.z;
            //    }
            //}
            _drones[0].currentX = (float)_p1.x;
            _drones[0].currentY = (float)_p1.y;
            _drones[0].currentZ = (float)_p1.z;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
        (ThreadStart)delegate()
        {
            writeLog("x:" + _p1.x + " y:" + _p1.y + " z:" + _p1.z, LOG_MODE.LOG_TEST);
        }
        );


        }

        //参数修改事件响应
        private void OnDroneConfigSaved()
        {
            string str = "";
            str += "参数修改成功\r\n";
            str += _drones[lv_droneList.SelectedIndex].Name + "   ID:" + _drones[lv_droneList.SelectedIndex].ID + "   IP:" + _drones[lv_droneList.SelectedIndex].IP + "\r\n";
            str += "目标位置 x:" + _drones[lv_droneList.SelectedIndex]._aimX;
            str += "y:" + _drones[lv_droneList.SelectedIndex]._aimY;
            str += "z:" + _drones[lv_droneList.SelectedIndex]._aimZ;
            str += "\r\n";

            str += "俯仰 ";
            str += "P:" + _drones[lv_droneList.SelectedIndex].P_pitch;
            str += "D:" + _drones[lv_droneList.SelectedIndex].D_pitch;
            str += "\r\n";

            str += "滚转 ";
            str += "P:" + _drones[lv_droneList.SelectedIndex].P_roll;
            str += "D:" + _drones[lv_droneList.SelectedIndex].D_roll;
            str += "\r\n";

            str += "高度 ";
            str += "P:" + _drones[lv_droneList.SelectedIndex].P_gaz;
            str += "\r\n";

            writeLog(str, LOG_MODE.LOG_NORMAL);
        }
        #endregion

        //连接按钮点击响应事件
        //判断文本框中的IP地址合法

        #region 无人机连接
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
                        //MessageBox.Show("IP地址非法", "错误", MessageBoxButton.OK);
                        writeLog("IP地址非法", LOG_MODE.LOG_ERROR);
                        return;
                    }
                }
            }
            catch
            {
                writeLog("IP地址非法", LOG_MODE.LOG_ERROR);
                return;
            }

            //IP重复读判断
            foreach (TestViewItem t in _drones)
            {
                if (t.IP.Equals(tb_ip.Text))
                {
                    writeLog("IP重复 , 飞机连接失败。", LOG_MODE.LOG_ERROR);
                    return;
                }
            }


            TestViewItem viewItem = new TestViewItem();
            viewItem.ID = Convert.ToInt32(format[3]);
            viewItem.Name = format[3] + "号飞机";
            viewItem.IP = tb_ip.Text;
            viewItem.drone = new MyDroneClient(viewItem.IP);
            // 设置PID参数
            viewItem.P_pitch = -0.3f;
            viewItem.P_roll = -0.3f;
            viewItem.P_gaz = 0.4f;
            viewItem.D_pitch = -0.9f;
            viewItem.D_roll = -0.8f;

            //设置目标位置信息
            if (_drones.Count == 0)
            {
                String tmp;
                tmp = ConfigurationManager.AppSettings["aimX"];
                viewItem._aimX = float.Parse(tmp);
                tmp = ConfigurationManager.AppSettings["aimY"];
                viewItem._aimY = float.Parse(tmp);
                tmp = ConfigurationManager.AppSettings["aimZ"];
                viewItem._aimZ = float.Parse(tmp);
            }
            viewItem.drone.NavigationDataAcquired += data => { viewItem.navigationData = data; };
            viewItem.drone.Start();
            _drones.Add(viewItem);

        }

        private void btn_test_Click(object sender, RoutedEventArgs e)
        {
            //if (_isDroneSelect)
            //{
            //    lv_droneList.SelectedIndex = -1;
            //}
            //if (lv_droneList.SelectedItem != null)
            //{
            //    //for (int i = 0; i < lv_droneList.SelectedItems.Count; i++)
            //    //{
            //    //    TestViewItem t = (TestViewItem)lv_droneList.SelectedItems[i];
            //    //    MessageBox.Show(i+":"+t.drone.IsAlive + "," + t.IP);
            //    //}
            //    foreach (Object o in lv_droneList.SelectedItems)
            //    {
            //        TestViewItem t = (TestViewItem)o;
            //        MessageBox.Show(":" + t.drone.IsAlive + "," + t.IP);
            //    }
            //}
            ChoosedReset();


        }

        private void ChoosedReset()
        {
            if (_isDroneSelect)
            {
                lv_droneList.SelectedIndex = -1;
            }

            btn_hand_control.Foreground = Brushes.Black;
            btn_auto_control.Foreground = Brushes.Black;
            _isAuto = false;
            _isHand = false;

            //按钮不可用
            btn_auto_takeoff.IsEnabled = false;
            btn_auto_land.IsEnabled = false;
            btn_auto_point2point.IsEnabled = false;

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
        //
        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                if (!_isAuto && !_isHand)
                {
                    while (_isDroneSelect)
                    {
                        //资源停用再删除
                        _drones[lv_droneList.SelectedIndex].drone.Stop();
                        _drones.RemoveAt(lv_droneList.SelectedIndex);
                    }
                }
                else
                {
                    //MessageBox.Show("请停止控制", "错误", MessageBoxButton.OK);
                    writeLog("请停止控制", LOG_MODE.LOG_ERROR);
                }
            }
            else
            {
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
            }
        }

        //配置参数
        private void btn_drone_config_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                if (!_isAuto && !_isHand)
                {
                    DroneConfig _d = new DroneConfig();
                    _d.d = (TestViewItem)lv_droneList.SelectedItem;
                    _d.OnSaved += OnDroneConfigSaved;
                    _d.ShowDialog();

                }
                else
                {
                    //MessageBox.Show("请停止控制", "错误", MessageBoxButton.OK);
                    writeLog("请停止控制", LOG_MODE.LOG_ERROR);
                }
            }
            else
            {
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
            }

        }

        private void btn_choosereset_Click(object sender, RoutedEventArgs e)
        {
            ChoosedReset();
        }

        #endregion

        #region 键盘控制
        //当进入手动控制模式时，监听键盘按键
        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            // MessageBox.Show(e.Key + "");
            if (oldKey != e.Key)
            {
                oldKey = e.Key;

                writeLog("down" + e.Key, LOG_MODE.LOG_TEST);
                //手动控制监控键盘输入
                #region 当手动控制时按键响应
                if (_isHand)
                {

                    //确认已选择飞机
                    if (!_isDroneSelect)
                    {
                        //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                        writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, yaw: -0.25f);
                                break;
                            }
                        case Key.A:
                            {
                                _drones[lv_droneList.SelectedIndex].drone.Progress(FlightMode.Progressive, yaw: 0.25f);
                                break;
                            }
                        case Key.Z:
                            {
                                ChoosedAutoControl();
                                break;
                            }

                        default:
                            break;
                    }

                    e.Handled = true;
                }
                #endregion

                #region 自动控制
                else if (_isAuto)
                {
                    //确认已选择飞机
                    if (!_isDroneSelect)
                    {
                        //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                        writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                        case Key.Z:
                            {
                                ChoosedHandControl();
                                break;
                            }
                        case Key.A:
                            {
                                if (_isP2P)
                                {
                                    P2PStop();
                                }
                                else
                                {
                                    P2PStart();
                                }
                                break;
                            }

                        default:
                            break;
                    }

                    e.Handled = true;
                }
                #endregion

                else
                {
                    switch (e.Key)
                    {
                        case Key.Z:
                            {
                                ChoosedHandControl();
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        private void Window_KeyUp_1(object sender, KeyEventArgs e)
        {
            writeLog("" + e.Key, LOG_MODE.LOG_TEST);
            oldKey = Key.Back;
            if (_isHand)
            {
                e.Handled = true;
                //Trace.WriteLine("按键抬起");

                if (!_isDroneSelect)
                {
                    // MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                    writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
            ChoosedHandControl();

        }

        private void ChoosedHandControl()
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
            btn_auto_takeoff.IsEnabled = false;
            btn_auto_land.IsEnabled = false;
            btn_auto_point2point.IsEnabled = false;
        }
        private void btn_auto_control_Click(object sender, RoutedEventArgs e)
        {
            ChoosedAutoControl();

        }

        private void ChoosedAutoControl()
        {
            btn_hand_control.Foreground = Brushes.Black;
            btn_auto_control.Foreground = Brushes.Red;
            _isHand = false;
            _isAuto = true;
            //按钮可用
            btn_auto_takeoff.IsEnabled = true;
            btn_auto_land.IsEnabled = true;
            btn_auto_point2point.IsEnabled = true;
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
        /// <summary>
        /// log打印
        /// mode 表示文本输出类别
        /// 0 正常输出
        /// 1 错误
        /// 2 警告
        /// </summary>
        /// <param name="str">要输出的文本内容</param>
        /// <param name="mode">输出文本类别</param>
        /// <returns></returns>
        private void writeLog(string str, LOG_MODE mode)
        {

            //当有3000行数据时，删除前100行
            if (tb_log.LineCount > 3000)
            {
                _logWriteStream.Write(tb_log.Text);
                _logWriteStream.Flush();
                tb_log.Clear();
            }
            switch (mode)
            {

                case LOG_MODE.LOG_NORMAL:
                    {
                        tb_log.AppendText(string.Format(@"正常 {0:HH_mm_ss.fff}:", DateTime.Now) + str + "\r\n");
                        break;
                    }
                case LOG_MODE.LOG_ERROR:
                    {
                        tb_log.AppendText(string.Format(@"错误 {0:HH_mm_ss.fff}:", DateTime.Now) + str + "\r\n");
                        break;
                    }
                case LOG_MODE.LOG_WARNING:
                    {
                        tb_log.AppendText(string.Format(@"警告 {0:HH_mm_ss.fff}:", DateTime.Now) + str + "\r\n");
                        break;
                    }
                case LOG_MODE.LOG_TEST:
                    {
                        tb_log.AppendText(string.Format(@"测试 {0:HH_mm_ss.fff}:", DateTime.Now) + str + "\r\n");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            if (!tb_log.IsFocused)
            {
                tb_log.ScrollToEnd();
            }
            //logs.Add(str);
        }

        #region 自动控制部分
        private void P2PStop()
        {
            _p2pTimer.Stop();
            _p2pTimer.IsEnabled = false;
            _isP2P = false;
            _viconPositionGet.Stop();
            _viconPositionGet.Join();
        }

        private void P2PStart()
        {
            _isP2P = true;
            _viconPositionGet.Start();
            _p2pTimer.IsEnabled = true;
            _p2pTimer.Start();
        }
        #endregion

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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
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
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
            }
        }

        #endregion

        #region 自动控制按钮
        private void btn_auto_takeoff_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                foreach (Object o in lv_droneList.SelectedItems)
                {
                    TestViewItem t = (TestViewItem)o;
                    t.drone.Takeoff();
                }
            }
            else
            {
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
            }
        }

        private void btn_auto_land_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                foreach (Object o in lv_droneList.SelectedItems)
                {
                    TestViewItem t = (TestViewItem)o;
                    t.drone.Land();
                }
            }
            else
            {
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
            }
        }

        private void btn_auto_point2point_Click(object sender, RoutedEventArgs e)
        {
            if (_isDroneSelect)
            {
                if (btn_auto_point2point.Content.Equals("点到点"))
                {

                    btn_auto_point2point.Content = "停  止";

                    P2PStart();


                }
                else if (btn_auto_point2point.Content.Equals("停  止"))
                {
                    btn_auto_point2point.Content = "点到点";

                    P2PStop();

                }
            }
            else
            {
                //MessageBox.Show("没有选择飞机", "错误", MessageBoxButton.OK);
                writeLog("没有选择飞机，请选择飞机", LOG_MODE.LOG_ERROR);
            }
        }


        #endregion

        #region 定时器响应函数
        //更新状态
        private void dTimer_Tick(object sender, EventArgs e)
        {

            //log测试

            if (_drones.Count != 0)
            {
                foreach (TestViewItem t in _drones)
                {
                    t.Updata();
                }
            }
            if (_isAuto && !_isP2P)
            {
                if (_isDroneSelect)
                {
                    foreach (Object o in lv_droneList.SelectedItems)
                    {
                        TestViewItem t = (TestViewItem)o;
                        t.drone.Hover();
                    }
                }
            }
        }

        private void p2pTimer_Tick(object sender, EventArgs e)
        {
            if (_isP2P && _isDroneSelect)
            {
                foreach (Object o in lv_droneList.SelectedItems)
                {
                    TestViewItem t = (TestViewItem)o;
                    //if (!t.IsConected)
                    //{
                    //    return;
                    //}

                    //if (t.navigationData.State.HasFlag(NavigationState.Flying))
                    {
                        //高度控制
                        t._controlZ = t.P_gaz * (t._aimZ - t.currentZ) / 1000.0f;
                        if (t._controlZ > gate) t._controlZ = gate;
                        if (t._controlZ < -gate) t._controlZ = -gate;

                        //X方向控制
                        t.Xerror[1] = t.Xerror[0];
                        t.Xerror[0] = (t._aimX - t.currentX) / 1000.0f;
                        t.Xerror_dot = (t.Xerror[0] - t.Xerror[1]) / T;
                        t._controlX = t.P_pitch * t.Xerror[0] + t.I_pitch * t.inte_x + t.D_pitch * t.Xerror_dot;

                        if (t._controlX > gate) t._controlX = gate;
                        if (t._controlX < -gate) t._controlX = -gate;

                        //Y方向控制
                        t.Yerror[1] = t.Yerror[0];
                        t.Yerror[0] = (t._aimY - t.currentY) / 1000;
                        t.Yerror_dot = (t.Yerror[0] - t.Yerror[1]) / T;
                        t._controlY = t.P_roll * t.Yerror[0] + t.I_roll * t.inte_y + t.D_roll * t.Yerror_dot;

                        if (t._controlY > gate) t._controlY = gate;
                        if (t._controlY < -gate) t._controlY = -gate;

                        ////限幅
                        //float len = (float)Math.Sqrt(_controlY * _controlY + _controlX * _controlX);
                        //if (len > gate)
                        //{
                        //    _controlX = gate * _controlX / len;
                        //    _controlY = gate * _controlY / len;
                        //}

                        ////坐标变换：
                        //_controlX = (float)(c * _controlX - s * _controlY);
                        //_controlY = (float)(c * _controlY + s * _controlX);

                        writeLog("controlx:" + t._controlX + " controly:" + t._controlY + " controlz:" + t._controlZ, LOG_MODE.LOG_TEST);

                        //t.drone.Progress(FlightMode.Progressive, roll: t._controlY, pitch: t._controlX, gaz: t._controlZ);
                    }

                }
            }
        }

        #endregion








    }
}
