using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AR.Drone.Client;
using AR.Drone.Data;
using System.Collections.Concurrent;
using AR.Drone.Client.Command;
using AR.Drone.Data.Navigation;
using AR.Drone.Client.Navigation;
using AR.Drone.Client.Video;
using AR.Drone.Infrastructure;
using System.Threading;
using System.Diagnostics;
using AR.Drone.Client.Configuration;

namespace MyDrone
{
    public class MyDroneClient :  WorkerBase
    {

        #region 配置变量看是否需要获取相应流
        private bool _isVideo;
        private bool _isNavidata;
        private bool _isCommand;

        public bool IsVideo
        {
            get { return _isVideo;}
            set { _isVideo = value; }
        }

        public bool IsNaviData
        {
            get { return _isNavidata; }
            set { _isNavidata = value; }
        }

        public bool IsCommand
        {
            get { return _isCommand; }
            set { _isCommand = value; }
        }
        #endregion

        #region 使用的常量
        private const string DefaultHostname = "192.168.1.1";
        #endregion

        #region 成员变量
        //定义默认的应答超时时间
        private const int AckControlAndWaitForConfirmationTimeout = 1000;

        //线程安全的队列，用于存储控制序列
        private readonly ConcurrentQueue<AtCommand> _commandQueue;
        //飞机飞行的状态数据
        private NavigationData _navigationData;
        //状态需求？
        private StateRequest _stateRequest;

        //指令发送类
        private readonly CommandSender _commandSender;
        //网络配置器，只存有host主机地址
        private readonly NetworkConfiguration _networkConfiguration;
        //navdata数据获取类
        private readonly NavdataAcquisition _navdataAcquisition;
        //视频数据获取类
        private readonly VideoAcquisition _videoAcquisition;
        #endregion

        #region 构造函数
        public MyDroneClient(string hostname, bool isVedio = true, bool isNavidata = true, bool isCommand = true)
        {
            _isVideo = isVedio;
            _isNavidata = isNavidata;
            _isCommand = isCommand;

            _networkConfiguration = new NetworkConfiguration(hostname);
            _commandQueue = new ConcurrentQueue<AtCommand>();
            _navigationData = new NavigationData();

            _commandSender = new CommandSender(NetworkConfiguration, _commandQueue);
            _navdataAcquisition = new NavdataAcquisition(NetworkConfiguration, OnNavdataPacketAcquired, OnNavdataAcquisitionStarted, OnNavdataAcquisitionStopped);
            _videoAcquisition = new VideoAcquisition(NetworkConfiguration, OnVideoPacketAcquired);
        }

        public MyDroneClient(string hostname)
            : this(hostname,isVedio:false)
        {
        }

        public MyDroneClient()
            : this(DefaultHostname)
        {
        }
        #endregion

        #region Events

        /// <summary>
        /// Event queue for all listeners interested in NavigationPacketAcquired events.
        /// This event will be dispatched on NavdataAcquisition thread.
        /// </summary>
        public event Action<NavigationPacket> NavigationPacketAcquired;

        /// <summary>
        /// Event queue for all listeners interested in NavigationDataAcquired events. 
        /// This event will be dispatched on NavdataAcquisition thread.
        /// </summary>
        public event Action<NavigationData> NavigationDataAcquired;

        /// <summary>
        /// Event queue for all listeners interested in VideoPacketAcquired events.
        /// This event will be dispatched on VideoAcquisition thread.
        /// </summary>
        public event Action<VideoPacket> VideoPacketAcquired;

        #endregion

        #region Properties

        public bool IsActive
        {
            get { return IsAlive; }
        }

        public bool IsConnected
        {
            get { return _navdataAcquisition.IsAcquiring; }
        }

        public NetworkConfiguration NetworkConfiguration
        {
            get { return _networkConfiguration; }
        }

        public NavigationData NavigationData
        {
            get { return _navigationData; }
        }

        #endregion

        protected override void Loop(System.Threading.CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                if (_navdataAcquisition.IsAlive == false) _navdataAcquisition.Start();
                Thread.Sleep(10);
            }

            //循环结束时
            //消除所有启动的线程

            if (_navdataAcquisition.IsAlive) _navdataAcquisition.Stop();
            if (_commandSender.IsAlive) _commandSender.Stop();
            if (_videoAcquisition.IsAlive) _videoAcquisition.Stop();
        }

        #region Private

        private void OnNavdataAcquisitionStarted()
        {
            if (_commandSender.IsAlive == false && _isCommand) _commandSender.Start();
            if (_videoAcquisition.IsAlive == false && _isVideo) _videoAcquisition.Start();
        }

        private void OnNavdataAcquisitionStopped()
        {
            if (_commandSender.IsAlive) _commandSender.Stop();
            if (_videoAcquisition.IsAlive) _videoAcquisition.Stop();
        }

        /// <summary>
        /// 为订阅NavdataPacketAcquired的类发送信息
        /// 通知其事件发生。
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private void OnNavdataPacketAcquired(NavigationPacket packet)
        {
            if (NavigationPacketAcquired != null)
                NavigationPacketAcquired(packet);

            UpdateNavigationData(packet);
        }


        /// <summary>
        /// 更新NavgationData
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private void UpdateNavigationData(NavigationPacket packet)
        {
            NavigationData navigationData;
            if (NavigationPacketParser.TryParse(ref packet, out navigationData))
            {
                OnNavigationDataAcquired(navigationData);

                _navigationData = navigationData;

                ProcessStateTransitions(navigationData.State);
            }
        }


        /// <summary>
        /// 导航数据接收事件发送消息
        /// </summary>
        /// <param name="navigationData"></param>
        /// <returns></returns>
        private void OnNavigationDataAcquired(NavigationData navigationData)
        {
            if (NavigationDataAcquired != null)
                NavigationDataAcquired(navigationData);
        }

        /// <summary>
        /// 解析NavigationState状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private void ProcessStateTransitions(NavigationState state)
        {
            if (state.HasFlag(NavigationState.Bootstrap))
            {
                _commandQueue.Flush();
                var settings = new Settings();
                settings.General.NavdataDemo = false;
                Send(settings);
            }

            if (state.HasFlag(NavigationState.Watchdog))
            {
                Trace.TraceWarning("Communication Watchdog!");
            }

            switch (_stateRequest)
            {
                case StateRequest.None:
                    return;
                case StateRequest.Emergency:
                    if (state.HasFlag(NavigationState.Flying))
                        Send(RefCommand.Emergency);
                    else
                        _stateRequest = StateRequest.None;
                    break;
                case StateRequest.ResetEmergency:
                    Send(RefCommand.Emergency);
                    _stateRequest = StateRequest.None;
                    break;
                case StateRequest.Land:
                    if (state.HasFlag(NavigationState.Flying) &&
                        state.HasFlag(NavigationState.Landing) == false)
                    {
                        Send(RefCommand.Land);
                    }
                    else
                        _stateRequest = StateRequest.None;
                    break;
                case StateRequest.Fly:
                    if (state.HasFlag(NavigationState.Landed) &&
                        state.HasFlag(NavigationState.Takeoff) == false &&
                        state.HasFlag(NavigationState.Emergency) == false)
                    {
                        Send(RefCommand.Takeoff);
                    }
                    else
                        _stateRequest = StateRequest.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (VideoPacketAcquired != null)
                VideoPacketAcquired(packet);
        }

        #endregion

        #region Api

        //public Task<Settings> GetConfigurationTask()
        //{
        //    //var configurationAcquisition = new ConfigurationAcquisition(this);
        //    var task = configurationAcquisition.CreateTask();
        //    return task;
        //}

        //发送控制指令
        public void Send(AtCommand command)
        {
            _commandQueue.Enqueue(command);
        }

        public void Send(Settings settings)
        {
            KeyValuePair<string, string> item;
            while (settings.Changes.TryDequeue(out item))
            {
                if (string.IsNullOrEmpty(settings.Custom.SessionId) == false &&
                    string.IsNullOrEmpty(settings.Custom.ProfileId) == false &&
                    string.IsNullOrEmpty(settings.Custom.ApplicationId) == false)
                    Send(new ConfigIdsCommand(settings.Custom.SessionId, settings.Custom.ProfileId, settings.Custom.ApplicationId));

                Send(new ConfigCommand(item.Key, item.Value));
            }
        }

        public bool AckControlAndWaitForConfirmation()
        {
            Stopwatch swTimeout = Stopwatch.StartNew();

            var state = NavigationState.Unknown;
            Action<NavigationData> onNavigationData = nd => state = nd.State;
            NavigationDataAcquired += onNavigationData;
            try
            {
                bool ackControlSent = false;
                while (swTimeout.ElapsedMilliseconds < AckControlAndWaitForConfirmationTimeout)
                {
                    if (state.HasFlag(NavigationState.Command))
                    {
                        Send(ControlCommand.AckControlMode);
                        ackControlSent = true;
                    }

                    if (ackControlSent && state.HasFlag(NavigationState.Command) == false)
                    {
                        return true;
                    }
                    Thread.Sleep(5);
                }
                return false;
            }
            finally
            {
                NavigationDataAcquired -= onNavigationData;
                Trace.Write(string.Format("AckCommand done in {0} ms.", swTimeout.ElapsedMilliseconds));
            }
        }

        public void Emergency()
        {
            _stateRequest = StateRequest.Emergency;
        }

        public void ResetEmergency()
        {
            _stateRequest = StateRequest.ResetEmergency;
        }

        public void Land()
        {
            if (_navigationData.State.HasFlag(NavigationState.Flying))
                _stateRequest = StateRequest.Land;
        }

        public void Takeoff()
        {
            if (_navigationData.State.HasFlag(NavigationState.Landed))
                _stateRequest = StateRequest.Fly;
        }

        public void FlatTrim()
        {
            if (_navigationData.State.HasFlag(NavigationState.Landed))
                Send(FlatTrimCommand.Default);
        }

        public void Hover()
        {
            if (_navigationData.State.HasFlag(NavigationState.Flying))
                Send(new ProgressCommand(FlightMode.Hover, 0, 0, 0, 0));
        }

        /// <summary>
        /// This command controls the drone flight motions.
        /// 控制飞机指令模块
        /// </summary>
        /// <param name="mode">Enabling the use of progressive commands and/or the Combined Yaw mode (bitfield).</param>
        /// <param name="roll">Drone left-right tilt - value in range [−1..1].</param>
        /// <param name="pitch">Drone front-back tilt - value in range [−1..1].</param>
        /// <param name="yaw">Drone angular speed - value in range [−1..1].</param>
        /// <param name="gaz">Drone vertical speed - value in range [−1..1].</param>
        public void Progress(FlightMode mode, float roll = 0, float pitch = 0, float yaw = 0, float gaz = 0)
        {
            if (roll > 1 || roll < -1)
                throw new ArgumentOutOfRangeException("roll");
            if (pitch > 1 || pitch < -1)
                throw new ArgumentOutOfRangeException("pitch");
            if (yaw > 1 || yaw < -1)
                throw new ArgumentOutOfRangeException("yaw");
            if (gaz > 1 || gaz < -1)
                throw new ArgumentOutOfRangeException("gaz");

            if (_navigationData.State.HasFlag(NavigationState.Flying))
                Send(new ProgressCommand(mode, roll, pitch, yaw, gaz));
        }

        /// <summary>
        /// This command controls the drone flight motions.
        /// </summary>
        /// <param name="mode">Enabling the use of progressive commands and/or the Combined Yaw mode (bitfield).</param>
        /// <param name="roll">Drone left-right tilt - value in range [−1..1].</param>
        /// <param name="pitch">Drone front-back tilt - value in range [−1..1].</param>
        /// <param name="yaw">Drone angular speed - value in range [−1..1].</param>
        /// <param name="gaz">Drone vertical speed - value in range [−1..1].</param>
        /// <param name="psi">Magneto psi - value in range [−1..1]</param>
        /// <param name="accuracy">Magneto psi accuracy - value in range [−1..1].</param>
        public void ProgressWithMagneto(FlightMode mode, float roll = 0, float pitch = 0, float yaw = 0, float gaz = 0, float psi = 0, float accuracy = 0)
        {
            if (roll > 1 || roll < -1)
                throw new ArgumentOutOfRangeException("roll");
            if (pitch > 1 || pitch < -1)
                throw new ArgumentOutOfRangeException("pitch");
            if (yaw > 1 || yaw < -1)
                throw new ArgumentOutOfRangeException("yaw");
            if (gaz > 1 || gaz < -1)
                throw new ArgumentOutOfRangeException("gaz");
            if (psi > 1 || psi < -1)
                throw new ArgumentOutOfRangeException("psi");
            if (accuracy > 1 || accuracy < -1)
                throw new ArgumentOutOfRangeException("accuracy");

            if (_navigationData.State.HasFlag(NavigationState.Flying))
                Send(new ProgressWithMagnetoCommand(mode, roll, pitch, yaw, gaz, psi, accuracy));
        }

        #endregion

        protected override void DisposeOverride()
        {
            base.DisposeOverride();

            _navdataAcquisition.Dispose();
            _commandSender.Dispose();
            _videoAcquisition.Dispose();
        }

       
    }
}
