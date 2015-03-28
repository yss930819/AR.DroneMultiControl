/*
 * 时间 2015 3 28
 * 注释 杨率帅
 *
 * 本代码为 视频数据的Frame
 * 存储视视频数据的相应参数
 */

using System.Runtime.InteropServices;

namespace AR.Drone.Video
{
    public class VideoFrame
    {
        public long Timestamp;
        public uint Number;
        public int Width;
        public int Height;
        public int Depth;
        public PixelFormat PixelFormat;
        public byte[] Data;
    }
}