/*
 * 时间 2015 3 29
 * 注释 杨率帅
 *
 * 本代码为 数据文件解包类
 * 可以保存两种数据包
 * 当解释道数据时发送响应事件让函数去响应
 * 
 * 
 */

using System;
using System.IO;
using System.Threading;
using AR.Drone.Data;
using AR.Drone.Infrastructure;
using AR.Drone.Media;

namespace AR.Drone.WinApp
{
    public class FilePlayer : WorkerBase
    {
        #region 成员变量
        private readonly Action<NavigationPacket> _navigationPacketAcquired;
        private readonly string _path;
        private readonly Action<VideoPacket> _videoPacketAcquired;

        //解析结束事件
        public event Action OnFileEnd;

        #endregion



        /// <summary>
        /// 构造函数
        /// 使用前要传入文件的位置
        /// 响应委托函数
        /// </summary>
        /// <param name="path"></param>
        /// <param name="navigationPacketAcquired"></param>
        /// <param name="videoPacketAcquired"></param>
        /// <returns></returns>
        public FilePlayer(string path, Action<NavigationPacket> navigationPacketAcquired, Action<VideoPacket> videoPacketAcquired)
        {
            _path = path;
            _navigationPacketAcquired = navigationPacketAcquired;
            _videoPacketAcquired = videoPacketAcquired;
        }


        /// <summary>
        /// 线程解析数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override void Loop(CancellationToken token)
        {
            //创立文件流
            using (var stream = new FileStream(_path, FileMode.Open))
            //创立包解析类
            using (var reader = new PacketReader(stream))
            {
                while (stream.Position < stream.Length && token.IsCancellationRequested == false)
                {
                    PacketType packetType = reader.ReadPacketType();
                    switch (packetType)
                    {
                        case PacketType.Navigation:
                            NavigationPacket navigationPacket = reader.ReadNavigationPacket();
                            _navigationPacketAcquired(navigationPacket);
                            break;
                        case PacketType.Video:
                            VideoPacket videoPacket = reader.ReadVideoPacket();
                            _videoPacketAcquired(videoPacket);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            if (OnFileEnd != null)
            {
                OnFileEnd();
            }
        }
    }
}