/*
 * 时间 2015 3 27
 * 注释 杨率帅
 *
 * 本代码为 获取飞机传输下来的data数据
 * 从WorkerBase 类继承 需要实现Loop虚函数来完成自己的
 * 
 * 
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AR.Drone.Infrastructure;
using AR.Drone.Data;

namespace AR.Drone.Client.Navigation
{
    public class NavdataAcquisition : WorkerBase
    {
        #region 成员变量

        //数据获取端口
        public const int NavdataPort = 5554;
        //
        public const int KeepAliveTimeout = 200;
        //获取navdata的超时时间
        public const int NavdataTimeout = 2000;

        //主机地址 由构造时传入
        private readonly NetworkConfiguration _configuration;
        //当数据接收到时的响应
        private readonly Action<NavigationPacket> _packetAcquired;
        //响应开始前委托
        private readonly Action _onAcquisitionStarted;
        //响应结束后委托
        private readonly Action _onAcquisitionStopped;

        //判断是否正在获取
        private bool _isAcquiring;

        #endregion


        /// <summary>
        /// 构造函数
        /// 构造时要对readonly变量赋值
        /// </summary>
        /// <param name="configuration">host主机</param>
        /// <param name="packetAcquired">响应委托</param>
        /// <param name="onAcquisitionStarted">响应开始前委托</param>
        /// <param name="onAcquisitionStopped">响应结束后委托</param>
        /// <returns></returns>
        public NavdataAcquisition(NetworkConfiguration configuration, Action<NavigationPacket> packetAcquired, Action onAcquisitionStarted,
                                  Action onAcquisitionStopped)
        {
            _configuration = configuration;
            _packetAcquired = packetAcquired;
            _onAcquisitionStarted = onAcquisitionStarted;
            _onAcquisitionStopped = onAcquisitionStopped;
        }

        public bool IsAcquiring
        {
            get { return _isAcquiring; }
        }

        /// <summary>
        /// 线程
        /// 用来与飞机通讯获取数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override void Loop(CancellationToken token)
        {
            _isAcquiring = false;
            //新建UDP客户端端口
            using (var udpClient = new UdpClient(NavdataPort))
                try
                {
                    //与飞机连接
                    udpClient.Connect(_configuration.DroneHostname, NavdataPort);

                    //发数据确认连接可行
                    SendKeepAliveSignal(udpClient);
                    //接收任何ip传入NavdataPort的数据
                    var remoteEp = new IPEndPoint(IPAddress.Any, NavdataPort);

                    //定位超时时间的变量
                    Stopwatch swKeepAlive = Stopwatch.StartNew();
                    Stopwatch swNavdataTimeout = Stopwatch.StartNew();
                    while (token.IsCancellationRequested == false && swNavdataTimeout.ElapsedMilliseconds < NavdataTimeout)
                    {
                        //udp客户端不可以使用时让线程休眠
                        if (udpClient.Available == 0)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            //接收数据，阻塞式
                            byte[] data = udpClient.Receive(ref remoteEp);
                            //创建新的packet
                            var packet = new NavigationPacket
                                {
                                    Timestamp = DateTime.UtcNow.Ticks,
                                    Data = data
                                };
                            //重启超时计时器
                            swNavdataTimeout.Restart();

                            //设置正在获取状态为真
                            _isAcquiring = true;
                            _onAcquisitionStarted();

                            //发送以获取到的包
                            _packetAcquired(packet);
                        }

                        if (swKeepAlive.ElapsedMilliseconds > KeepAliveTimeout)
                        {
                            SendKeepAliveSignal(udpClient);
                            swKeepAlive.Restart();
                        }
                    }
                }
                finally
                {
                    //若之前状态为正在获取
                    //就将其关闭，并发送事件
                    if (_isAcquiring)
                    {
                        _isAcquiring = false;
                        _onAcquisitionStopped();
                    }
                }
        }

        
        /// <summary>
        /// 发送存活信息
        /// 发送1
        /// </summary>
        /// <param name="udpClient">连接好的客户机</param>
        /// <returns></returns>
        private void SendKeepAliveSignal(UdpClient udpClient)
        {
            byte[] payload = BitConverter.GetBytes(1);
            try
            {
                udpClient.Send(payload, payload.Length);
            }
            catch (SocketException)
            {
            }
        }
    }
}