/*
 * 时间 2015 3 29
 * 注释 杨率帅
 *
 * 本代码为 Packet数据保存类
 * 可以保存两种数据包
 * 
 */

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using AR.Drone.Infrastructure;
using AR.Drone.Data;

namespace AR.Drone.Media
{
    public class PacketRecorder : WorkerBase
    {
        private readonly ConcurrentQueue<object> _packetQueue;
        private readonly Stream _stream;

        public PacketRecorder(Stream stream)
        {
            _stream = stream;
            _packetQueue = new ConcurrentQueue<object>();
        }

        public void EnqueuePacket(NavigationPacket packet)
        {
            _packetQueue.Enqueue(packet);
        }

        public void EnqueuePacket(VideoPacket packet)
        {
            _packetQueue.Enqueue(packet);
        }

        protected override void Loop(CancellationToken token)
        {
            _packetQueue.Flush();

            using (var recorder = new PacketWriter(_stream))
            {
                while (token.IsCancellationRequested == false)
                {
                    object packet;
                    while (_packetQueue.TryDequeue(out packet))
                    {
                        recorder.WritePacket(packet);
                    }
                    Thread.Sleep(1);
                }
            }
        }
    }
}