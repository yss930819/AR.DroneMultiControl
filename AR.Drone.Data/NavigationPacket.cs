/*
 * 时间 2015 3 26
 * 注释 杨率帅
 *
 * 本代码为导航数据包结构体
 * 存储飞机传过来的导航数据信息
 * Data为数据数组
 */
using System.Runtime.InteropServices;

namespace AR.Drone.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NavigationPacket
    {
        public long Timestamp;
        public byte[] Data;
    }
}