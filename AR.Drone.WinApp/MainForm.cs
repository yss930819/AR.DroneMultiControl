/*
 * 时间 2015 3 23
 * 注释 杨率帅
 *
 * 本代码为程序主界面代码
 * 从Form 继承
 * 为整个代码的详细函数
 * 
 * 修改构造函数可以连接多架无人机
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR.Drone.Client;
using AR.Drone.Client.Command;
using AR.Drone.Client.Configuration;
using AR.Drone.Data;
using AR.Drone.Data.Navigation;
using AR.Drone.Data.Navigation.Native;
using AR.Drone.Media;
using AR.Drone.Video;
using AR.Drone.Avionics;
using AR.Drone.Avionics.Objectives;
using AR.Drone.Avionics.Objectives.IntentObtainers;

namespace AR.Drone.WinApp
{
    public partial class MainForm : Form
    {
        #region 成员变量
        //使用到的视频文件格式信息常量
        private const string ARDroneTrackFileExt = ".ardrone";
        private const string ARDroneTrackFilesFilter = "AR.Drone track files (*.ardrone)|*.ardrone";


        //无人机类
        private readonly DroneClient _droneClient;
        //视频框架
        private readonly List<PlayerForm> _playerForms;
        //视频包解析工具
        private readonly VideoPacketDecoderWorker _videoPacketDecoderWorker;
        private Settings _settings;
        private VideoFrame _frame;
        private Bitmap _frameBitmap;
        private uint _frameNumber;
        private NavigationData _navigationData;
        //导航数据包
        private NavigationPacket _navigationPacket;
        //包数据保存类
        private PacketRecorder _packetRecorderWorker;
        //视频存储文件流
        private FileStream _recorderStream;
        //自动驾驶
        private Autopilot _autopilot;

        #region 写文件相关变量
        //写文件开关
        //private bool _writeFile = false;
        //Navdata文件流
        //  private FileStream _navdataFileStream;
        // private PacketRecorder _navdataPacketRecorder;
        //视频存储位置
        // private string _vedioDir = System.Environment.CurrentDirectory + @"/vedio";

        #endregion

        #endregion

        public MainForm()
            : this("192.168.1.13")
        {
        }

        public MainForm(String host)
        {
            InitializeComponent();

            //视频解码设置
            _videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _videoPacketDecoderWorker.Start();

            //创建新的无人机连接
            _droneClient = new DroneClient(host);
            //导航数据获取事件，添加事件响应
            _droneClient.NavigationPacketAcquired += OnNavigationPacketAcquired;
            _droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
            _droneClient.NavigationDataAcquired += data => _navigationData = data;

            //定时器更新允许
            tmrStateUpdate.Enabled = true;
            tmrVideoUpdate.Enabled = true;

            _playerForms = new List<PlayerForm>();

            _videoPacketDecoderWorker.UnhandledException += UnhandledException;

        }

        public MainForm(DroneClient droneClient)
        {

            InitializeComponent();

            //视频解码设置
            _videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _videoPacketDecoderWorker.Start();

            //创建新的无人机连接
            _droneClient = droneClient;
            //导航数据获取事件，添加事件响应
            _droneClient.NavigationPacketAcquired += OnNavigationPacketAcquired;
            _droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
            _droneClient.NavigationDataAcquired += data => _navigationData = data;

            //定时器更新允许
            tmrStateUpdate.Enabled = true;
            tmrVideoUpdate.Enabled = true;

            _playerForms = new List<PlayerForm>();

            _videoPacketDecoderWorker.UnhandledException += UnhandledException;


        }


        /// <summary>
        /// 错误处理不份
        /// 让用户拷贝错误信息自行处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private void UnhandledException(object sender, Exception exception)
        {
            MessageBox.Show(exception.ToString(), "Unhandled Exception (Ctrl+C)", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// 重写Load代码
        /// 判断客户端类型
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text += Environment.Is64BitProcess ? " [64-bit]" : " [32-bit]";
        }


        /// <summary>
        /// 重写关闭代码
        /// 将开启的资源关闭
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override void OnClosed(EventArgs e)
        {
            if (_autopilot != null)
            {
                _autopilot.UnbindFromClient();
                _autopilot.Stop();
            }

            StopRecording();

            _droneClient.Dispose();
            _videoPacketDecoderWorker.Dispose();

            base.OnClosed(e);
        }


        /// <summary>
        /// 响应NavigationPacket获取委托
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private void OnNavigationPacketAcquired(NavigationPacket packet)
        {

            // 将数据包入队 准备写入文件
            if (_packetRecorderWorker != null && _packetRecorderWorker.IsAlive)
                _packetRecorderWorker.EnqueuePacket(packet);

            _navigationPacket = packet;
        }


        /// <summary>
        /// 响应VideoPacket获取委托
        /// 将包入队
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            //将视频包入队 准备写入文件
            if (_packetRecorderWorker != null && _packetRecorderWorker.IsAlive)
                _packetRecorderWorker.EnqueuePacket(packet);
            if (_videoPacketDecoderWorker.IsAlive)
                _videoPacketDecoderWorker.EnqueuePacket(packet);
        }

        private void OnVideoPacketDecoded(VideoFrame frame)
        {
            _frame = frame;
        }



        private void btnStart_Click(object sender, EventArgs e)
        {
            _droneClient.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _droneClient.Stop();
        }

        /// <summary>
        /// 视频定时器响应
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void tmrVideoUpdate_Tick(object sender, EventArgs e)
        {
            if (_frame == null || _frameNumber == _frame.Number)
                return;
            _frameNumber = _frame.Number;

            if (_frameBitmap == null)
                _frameBitmap = VideoHelper.CreateBitmap(ref _frame);
            else
                VideoHelper.UpdateBitmap(ref _frameBitmap, ref _frame);

            pbVideo.Image = _frameBitmap;
        }

        /// <summary>
        /// 状态定时器响应
        /// 更新Tree view 中的参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void tmrStateUpdate_Tick(object sender, EventArgs e)
        {
            tvInfo.BeginUpdate();

            //标签一
            TreeNode node = tvInfo.Nodes.GetOrCreate("ClientActive");
            node.Text = string.Format("Client Active: {0}", _droneClient.IsActive);

            //标签二
            node = tvInfo.Nodes.GetOrCreate("Navigation Data");
            if (_navigationData != null) DumpBranch(node.Nodes, _navigationData);

            //标签三
            node = tvInfo.Nodes.GetOrCreate("Configuration");
            if (_settings != null) DumpBranch(node.Nodes, _settings);

            //标签四
            TreeNode vativeNode = tvInfo.Nodes.GetOrCreate("Native");

            NavdataBag navdataBag;
            if (_navigationPacket.Data != null && NavdataBagParser.TryParse(ref _navigationPacket, out navdataBag))
            {
                var ctrl_state = (CTRL_STATES)(navdataBag.demo.ctrl_state >> 0x10);
                node = vativeNode.Nodes.GetOrCreate("ctrl_state");
                node.Text = string.Format("Ctrl State: {0}", ctrl_state);

                var flying_state = (FLYING_STATES)(navdataBag.demo.ctrl_state & 0xffff);
                node = vativeNode.Nodes.GetOrCreate("flying_state");
                node.Text = string.Format("Ctrl State: {0}", flying_state);


                DumpBranch(vativeNode.Nodes, navdataBag);
            }
            tvInfo.EndUpdate();

            if (_autopilot != null && !_autopilot.Active && btnAutopilot.ForeColor != Color.Black)
                btnAutopilot.ForeColor = Color.Black;
        }

        /// <summary>
        /// 解析生成树
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        private void DumpBranch(TreeNodeCollection nodes, object o)
        {
            Type type = o.GetType();

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                TreeNode node = nodes.GetOrCreate(fieldInfo.Name);
                object value = fieldInfo.GetValue(o);

                DumpValue(fieldInfo.FieldType, node, value);
            }

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                TreeNode node = nodes.GetOrCreate(propertyInfo.Name);
                object value = propertyInfo.GetValue(o, null);

                DumpValue(propertyInfo.PropertyType, node, value);
            }
        }

        private void DumpValue(Type type, TreeNode node, object value)
        {
            if (value == null)
                node.Text = node.Name + ": null";
            else
            {
                if (type.Namespace.StartsWith("System") || type.IsEnum)
                    node.Text = node.Name + ": " + value;
                else
                    DumpBranch(node.Nodes, value);
            }
        }

        #region 控制按钮响应事件

        private void btnFlatTrim_Click(object sender, EventArgs e)
        {
            _droneClient.FlatTrim();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _droneClient.Takeoff();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _droneClient.Land();
        }

        private void btnEmergency_Click(object sender, EventArgs e)
        {
            _droneClient.Emergency();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _droneClient.ResetEmergency();
        }

        private void btnSwitchCam_Click(object sender, EventArgs e)
        {
            var configuration = new Settings();
            configuration.Video.Channel = VideoChannelType.Next;
            _droneClient.Send(configuration);
        }

        private void btnHover_Click(object sender, EventArgs e)
        {
            _droneClient.Hover();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f);
        }

        private void btnTurnLeft_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, yaw: 0.25f);
        }

        private void btnTurnRight_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, yaw: -0.25f);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, roll: -0.05f);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, roll: 0.05f);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f);
        }

        private void btnReadConfig_Click(object sender, EventArgs e)
        {
            Task<Settings> configurationTask = _droneClient.GetConfigurationTask();
            configurationTask.ContinueWith(delegate(Task<Settings> task)
                {
                    if (task.Exception != null)
                    {
                        Trace.TraceWarning("Get configuration task is faulted with exception: {0}", task.Exception.InnerException.Message);
                        return;
                    }

                    _settings = task.Result;
                });
            configurationTask.Start();
        }

        private void btnSendConfig_Click(object sender, EventArgs e)
        {
            var sendConfigTask = new Task(() =>
                {
                    if (_settings == null) _settings = new Settings();
                    Settings settings = _settings;

                    if (string.IsNullOrEmpty(settings.Custom.SessionId) ||
                        settings.Custom.SessionId == "00000000")
                    {
                        // set new session, application and profile
                        _droneClient.AckControlAndWaitForConfirmation(); // wait for the control confirmation

                        settings.Custom.SessionId = Settings.NewId();
                        _droneClient.Send(settings);

                        _droneClient.AckControlAndWaitForConfirmation();

                        settings.Custom.ProfileId = Settings.NewId();
                        _droneClient.Send(settings);

                        _droneClient.AckControlAndWaitForConfirmation();

                        settings.Custom.ApplicationId = Settings.NewId();
                        _droneClient.Send(settings);

                        _droneClient.AckControlAndWaitForConfirmation();
                    }

                    settings.General.NavdataDemo = false;
                    settings.General.NavdataOptions = NavdataOptions.All;

                    settings.Video.BitrateCtrlMode = VideoBitrateControlMode.Dynamic;
                    settings.Video.Bitrate = 1000;
                    settings.Video.MaxBitrate = 2000;

                    //settings.Leds.LedAnimation = new LedAnimation(LedAnimationType.BlinkGreenRed, 2.0f, 2);
                    //settings.Control.FlightAnimation = new FlightAnimation(FlightAnimationType.Wave);

                    // record video to usb
                    //settings.Video.OnUsb = true;
                    // usage of MP4_360P_H264_720P codec is a requirement for video recording to usb
                    //settings.Video.Codec = VideoCodecType.MP4_360P_H264_720P;
                    // start
                    //settings.Userbox.Command = new UserboxCommand(UserboxCommandType.Start);
                    // stop
                    //settings.Userbox.Command = new UserboxCommand(UserboxCommandType.Stop);


                    //send all changes in one pice
                    _droneClient.Send(settings);
                });
            sendConfigTask.Start();
        }


        /// <summary>
        /// 停止录像
        /// 释放使用的资源
        /// </summary>
        /// <returns></returns>
        private void StopRecording()
        {
            if (_packetRecorderWorker != null)
            {
                _packetRecorderWorker.Stop();
                _packetRecorderWorker.Join();
                _packetRecorderWorker = null;
            }
            //             if (_navdataPacketRecorder != null)
            //             {
            //                 _navdataPacketRecorder.Stop();
            //                 _navdataPacketRecorder.Join();
            //                 _navdataPacketRecorder = null;
            //             }
            if (_recorderStream != null)
            {
                _recorderStream.Dispose();
                _recorderStream = null;
            }

            //             if (_navdataFileStream != null)
            //             {
            //                 _navdataFileStream.Dispose();
            //                 _navdataFileStream = null;
            //             }
        }


        /// <summary>
        /// 开始记录视频
        /// 按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            string path = string.Format("flight_{0:yyyy_MM_dd_HH_mm}" + ARDroneTrackFileExt, DateTime.Now);

            using (var dialog = new SaveFileDialog { DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter, FileName = path })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    _recorderStream = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
                    _packetRecorderWorker = new PacketRecorder(_recorderStream);
                    _packetRecorderWorker.Start();
                }
            }
        }

        /// <summary>
        /// 停止记录视频
        /// 按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            StopRecording();
        }


        /// <summary>
        /// 视屏回放按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void btnReplay_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    var playerForm = new PlayerForm { FileName = dialog.FileName };
                    playerForm.Closed += (o, args) => _playerForms.Remove(o as PlayerForm);
                    _playerForms.Add(playerForm);
                    playerForm.Show(this);
                }
            }
        }

        #endregion


        // Make sure '_autopilot' variable is initialized with an object
        private void CreateAutopilot()
        {
            if (_autopilot != null) return;

            _autopilot = new Autopilot(_droneClient);
            _autopilot.OnOutOfObjectives += Autopilot_OnOutOfObjectives;
            _autopilot.BindToClient();
            _autopilot.Start();
        }

        // Event that occurs when no objectives are waiting in the autopilot queue
        private void Autopilot_OnOutOfObjectives()
        {
            _autopilot.Active = false;
        }

        // Create a simple mission for autopilot
        private void CreateAutopilotMission()
        {
            _autopilot.ClearObjectives();

            // Do two 36 degrees turns left and right if the drone is already flying
            if (_droneClient.NavigationData.State.HasFlag(NavigationState.Flying))
            {
                const float turn = (float)(Math.PI / 5);
                float heading = _droneClient.NavigationData.Yaw;

                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading + turn, aCanBeObtained: true)));
                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading - turn, aCanBeObtained: true)));
                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading, aCanBeObtained: true)));
            }
            else // Just take off if the drone is on the ground
            {
                _autopilot.EnqueueObjective(new FlatTrim(1000));
                _autopilot.EnqueueObjective(new Takeoff(3500));
            }

            // One could use hover, but the method below, allows to gain/lose/maintain desired altitude
            _autopilot.EnqueueObjective(
                Objective.Create(3000,
                    new VelocityX(0.0f),
                    new VelocityY(0.0f),
                    new Altitude(1.0f)
                )
            );

            _autopilot.EnqueueObjective(new Land(5000));
        }

        // Activate/deactive autopilot
        private void btnAutopilot_Click(object sender, EventArgs e)
        {
            if (!_droneClient.IsActive) return;

            CreateAutopilot();
            if (_autopilot.Active) _autopilot.Active = false;
            else
            {
                CreateAutopilotMission();
                _autopilot.Active = true;
                btnAutopilot.ForeColor = Color.Red;
            }
        }


        /// <summary>
        /// 写文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void btnFileWrite_Click(object sender, EventArgs e)
        {
            //if (btnFileWrite.Text.Equals("写文件"))
            //{
            //    string navdataFile = string.Format(@"navdata_{0:yyyy_MM_dd_HH_mm}.nav", DateTime.Now);
            //    string vedioFile = string.Format(@"vedio_{0:yyyy_MM_dd_HH_mm}.vedio", DateTime.Now);
            //    string dir = System.Environment.CurrentDirectory + @"/navdata";
            //    btnFileWrite.Text = "停  止";
            //    if (!Directory.Exists(dir))
            //    {
            //        Directory.CreateDirectory(dir);
            //    }
            //    if (!Directory.Exists(_vedioDir))
            //    {
            //        Directory.CreateDirectory(_vedioDir);
            //    }

            //    try
            //    {
            //        _navdataFileStream = new FileStream(dir+@"/"+ navdataFile, FileMode.OpenOrCreate);
            //        _recorderStream = new FileStream(_vedioDir + "/" + vedioFile, FileMode.OpenOrCreate);
            //        _packetRecorderWorker = new PacketRecorder(_recorderStream);
            //        _packetRecorderWorker.Start();
            //        _navdataPacketRecorder = new PacketRecorder(_navdataFileStream);
            //        _navdataPacketRecorder.Start();
            //    }
            //    catch (System.Exception ex)
            //    {
            //        Trace.Write("文件创建失败", "我的文件输出流");
            //    }
            //}
            //else if (btnFileWrite.Text.Equals("停  止"))
            //{
            //    _navdataFileStream.Close();
            //    _writeFile = false;
            //    btnFileWrite.Text = "写文件";
            //}

        }

    }
}