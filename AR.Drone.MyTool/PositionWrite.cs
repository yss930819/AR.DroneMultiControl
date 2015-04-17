/*
 * 将Vicon中读到的数据写入文件
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AR.Drone.Infrastructure;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace AR.Drone.MyTool
{
    public class PositionWrite : WorkerBase
    {
        #region 成员变量
        private readonly ConcurrentQueue<position> _packetQueue;
        private readonly string _dir;
        private readonly string _fileName;

        private FileStream _viconFileStream;
        private StreamWriter _viconWriteStream;

        //写结束事件
        public event Action OnWriteEnd;

        #endregion

        public PositionWrite(string dir, string filename)
        {
            _fileName = filename;
            _dir = dir;
            _packetQueue = new ConcurrentQueue<position>();

            _viconFileStream = new FileStream(dir + @"/" + filename, FileMode.OpenOrCreate);
            _viconWriteStream = new StreamWriter(_viconFileStream);
        }

        public void EnqueuePacket(position packet)
        {
            _packetQueue.Enqueue(packet);
        }


        protected override void Loop(System.Threading.CancellationToken token)
        {
            _packetQueue.Flush();
            int num = 0;
            while (token.IsCancellationRequested == false || !_packetQueue.IsEmpty)
            {
                position packet;
                while (_packetQueue.TryDequeue(out packet))
                {
                    _viconWriteStream.WriteLine("-------------------------");
                    _viconWriteStream.WriteLine(packet.times + " " + packet.X1 + " " + packet.Y1 + " " + packet.Z1 + " "
                    + packet.X2 + " " + packet.Y2 + " " + packet.Z2 + " " + packet.X3 + " " + packet.Y3 + " " + packet.Z3 + " "
                    + packet.X4 + " " + packet.Y4 + " " + packet.Z4 + " " + packet.TX + " " + packet.TY + " " + packet.TZ + " "
                    + packet.EX + " " + packet.EY + " " + packet.EZ);
                    _viconWriteStream.WriteLine("-------------------------");
                    _viconWriteStream.Flush();
                }
                Thread.Sleep(10);
            }
            //发送写结束事件
            if (OnWriteEnd != null)
            {
                OnWriteEnd();
            }
        }

        protected override void DisposeOverride()
        {
            _viconWriteStream.Flush();
            _viconWriteStream.Dispose();
            _viconFileStream.Dispose();

            base.DisposeOverride();
        }
    }
}
