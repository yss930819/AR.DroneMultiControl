/*
 * 时间 2015 3 28
 * 注释 杨率帅
 *
 * 本代码为 获取飞机传输下来的视频数据
 * 从WorkerBase 类继承 需要实现Loop虚函数来完成视频传输
 * 
 * 
 */

using System;
using System.Net.Sockets;
using System.Threading;
using AR.Drone.Client.Video.Native;
using AR.Drone.Infrastructure;
using AR.Drone.Data;

namespace AR.Drone.Client.Video
{
    public class VideoAcquisition : WorkerBase
    {
        #region 成员变量

        //视频传输端口
        public const int VideoPort = 5555;
        //缓存大小
        public const int FrameBufferSize = 0x100000;
        //流读取大小
        public const int NetworkStreamReadSize = 0x1000;
        //主机ip
        private readonly NetworkConfiguration _configuration;
        //视频获取委托
        private readonly Action<VideoPacket> _videoPacketAcquired;

        #endregion


        /// <summary>
        /// 构造函数
        /// 
        /// </summary>
        /// <param name="configuration">主机ip</param>
        /// <param name="videoPacketAcquired">视频获取函数委托</param>
        /// <returns></returns>
        public VideoAcquisition(NetworkConfiguration configuration, Action<VideoPacket> videoPacketAcquired)
        {
            _configuration = configuration;
            _videoPacketAcquired = videoPacketAcquired;
        }

        /// <summary>
        /// 获取视频数据线程
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override unsafe void Loop(CancellationToken token)
        {
            //创建tcp连接
            using (var tcpClient = new TcpClient(_configuration.DroneHostname, VideoPort))
            //获取网络流
            using (NetworkStream stream = tcpClient.GetStream())
            {
                var packet = new VideoPacket();
                byte[] packetData = null;
                int offset = 0;
                int frameStart = 0;
                int frameEnd = 0;
                //由缓存大小建立数组
                var buffer = new byte[FrameBufferSize];
                //只用指针
                //防止地址改变
                fixed (byte* pBuffer = &buffer[0])
                    //循环获取视频数据
                    while (token.IsCancellationRequested == false)
                    {
                        //判断是否可用
                        if (tcpClient.Available == 0)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        //从流中读取数据 读入buffer
                        int read = stream.Read(buffer, offset, NetworkStreamReadSize);

                        //无数据继续读取
                        if (read == 0)
                        {
                            continue;
                        }

                        //重新设置读取位置
                        offset += read;
                        if (packetData == null)
                        {
                            // lookup for a frame start
                            //设置数据的最大检索范围。要减去parrot_video_encapsulation_t的大小
                            
                            int maxSearchIndex = offset - sizeof (parrot_video_encapsulation_t);
                            for (int i = 0; i < maxSearchIndex; i++)
                            {
                                if (buffer[i] == 'P' && buffer[i + 1] == 'a' && buffer[i + 2] == 'V' && buffer[i + 3] == 'E')
                                {
                                    parrot_video_encapsulation_t pve = *(parrot_video_encapsulation_t*) (pBuffer + i);
                                    packetData = new byte[pve.payload_size];
                                    packet = new VideoPacket
                                        {
                                            Timestamp = DateTime.UtcNow.Ticks,
                                            FrameNumber = pve.frame_number,
                                            Width = pve.display_width,
                                            Height = pve.display_height,
                                            FrameType = VideoFrameTypeConverter.Convert(pve.frame_type),
                                            Data = packetData
                                        };
                                    frameStart = i + pve.header_size;
                                    frameEnd = frameStart + packet.Data.Length;
                                    break;
                                }
                            }
                            if (packetData == null)
                            {
                                // frame is not detected
                                offset -= maxSearchIndex;
                                Array.Copy(buffer, maxSearchIndex, buffer, 0, offset);
                            }
                        }
                        if (packetData != null && offset >= frameEnd)
                        {
                            // frame acquired
                            //拷贝视频数据
                            Array.Copy(buffer, frameStart, packetData, 0, packetData.Length);
                            //发送视频获取信息
                            _videoPacketAcquired(packet);

                            // clean up acquired frame
                            packetData = null;
                            offset -= frameEnd;
                            Array.Copy(buffer, frameEnd, buffer, 0, offset);
                        }
                    }
            }
        }
    }
}