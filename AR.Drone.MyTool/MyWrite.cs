/*
 * 时间 2015 3 29
 * 作者 杨率帅
 *
 * 本代码为 Packet数据解析存储类
 * 通过本方法可以解析2类packet
 * 
 * 在解析全部结束时发送 WriteEnd 消息
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System.Drawing;

using AR.Drone.Data;
using AR.Drone.Infrastructure;
using AR.Drone.Data.Navigation.Native;
using AR.Drone.Video;
using System.Reflection;



namespace AR.Drone.MyTool
{
    public class MyWrite : WorkerBase
    {

        #region 成员变量
        private readonly ConcurrentQueue<object> _packetQueue;
        private readonly string _dir;
        private readonly string _fileName;

        private readonly string _vedioDir;
        private readonly string _navdataDir;
        private readonly VideoPacketDecoder _videoDecoder;
        //存储
        private Bitmap _frameBitmap;

        //写结束事件
        public event Action OnWriteEnd;

        #endregion


        public MyWrite(string dir, string filename)
        {
            _videoDecoder = new VideoPacketDecoder(PixelFormat.BGR24);

            _fileName = filename;
            _dir = dir;
            _packetQueue = new ConcurrentQueue<object>();

            _vedioDir = _dir + @"/" + _fileName + @"/vedio";
            _navdataDir = _dir + @"/" + _fileName + @"/navdata";


            //创建vedio文件夹
            if (!Directory.Exists(_vedioDir))
            {
                Directory.CreateDirectory(_vedioDir);
            }
            //创建navdata文件夹
            if (!Directory.Exists(_navdataDir))
            {
                Directory.CreateDirectory(_navdataDir);
            }
        }
        #region 队列添加
        public void EnqueuePacket(NavigationPacket packet)
        {
            _packetQueue.Enqueue(packet);
        }

        public void EnqueuePacket(VideoPacket packet)
        {
            _packetQueue.Enqueue(packet);
        }
        #endregion

        protected override void Loop(CancellationToken token)
        {
            _packetQueue.Flush();
            int num = 0;
            while (token.IsCancellationRequested == false || !_packetQueue.IsEmpty)
            {
                object packet = null;
                while (_packetQueue.TryDequeue(out packet))
                {
                    if (packet is NavigationPacket)
                    {
                        var navigationPacket = (NavigationPacket)packet;
                        Write(navigationPacket);
                    }
                    else if (packet is VideoPacket)
                    {
                        num++;
                        var videoPacket = (VideoPacket)packet;
                        Write(videoPacket);
                    }
                    else
                    {
                        string message = string.Format("Not supported packet type - {0}.", packet.GetType().Name);
                        throw new NotSupportedException(message);
                    }
                }
                Thread.Sleep(10);
            }
            //发送写结束事件
            if (OnWriteEnd != null)
            {
                OnWriteEnd();
            }

        }

        /// <summary>
        /// 写图像文件
        /// </summary>
        /// <param name="videoPacket"></param>
        /// <returns></returns>
        private void Write(VideoPacket videoPacket)
        {
            VideoFrame frame;
            if (_videoDecoder.TryDecode(ref videoPacket, out frame))
            {
                WriteBitmap(frame);
            }

        }

        private void WriteBitmap(VideoFrame frame)
        {
            if (_frameBitmap == null)
                _frameBitmap = VideoHelper.CreateBitmap(ref frame);
            else
                VideoHelper.UpdateBitmap(ref _frameBitmap, ref frame);

            _frameBitmap.Save(_vedioDir + @"/" + frame.Timestamp + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        /// <summary>
        /// 写导航数据文件
        /// </summary>
        /// <param name="navigationPacket"></param>
        /// <returns></returns>
        private void Write(NavigationPacket navigationPacket)
        {
            using (FileStream navFileStream = new FileStream(_navdataDir + @"/" + navigationPacket.Timestamp + ".nav", FileMode.OpenOrCreate))
            {
                using (StreamWriter navWriter = new StreamWriter(navFileStream))
                {
                    NavdataBag navdataBag;

                    if (navigationPacket.Data != null && NavdataBagParser.TryParse(ref navigationPacket, out navdataBag))
                    {
                        DumpBranch(navWriter, navdataBag);
                    }
                }
            }
        }

        #region 导航数据遍历
        private void DumpBranch(StreamWriter navWriter, Object o)
        {
            Type type = o.GetType();

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                navWriter.Write("[" + fieldInfo.Name + "]");
                object value = fieldInfo.GetValue(o);
                DumpValue(fieldInfo.FieldType, navWriter, value);
                navWriter.WriteLine();
            }
        }

        private void DumpValue(Type type, StreamWriter navWriter, object value)
        {
            if (value == null)
                navWriter.Write(": null");
            else
            {
                if (type.Namespace.StartsWith("System") || type.IsEnum)
                    navWriter.Write(":" + value + "");
                else
                {
                    navWriter.WriteLine();
                    DumpBranch(navWriter, value);
                }
            }
        }
        #endregion

        protected override void DisposeOverride()
        {
            base.DisposeOverride();
        }
    }


}
