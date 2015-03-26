/*
 * 时间 2015 3 25
 * 注释 杨率帅
 *
 * 本代码为导航数据信息
 * 是数据基本服务
 * 提供了飞机上可读下来的数据集
 * 
 */


using System.Runtime.InteropServices;

namespace AR.Drone.Data.Navigation
{
    /*
     * 导航数据类，
     * 使用该类即可知道所有导航信息
     */ 
    public class NavigationData
    {
        public NavigationState State;
        public float Yaw; // radians - Yaw - Z
        public float Pitch; // radians - Pitch - Y
        public float Roll; // radians - Roll - X
        public float Altitude; // meters
        public Vector3 Velocity; // meter/second
        public Battery Battery;
        public Magneto Magneto;
        public float Time; // seconds
        public Video Video;
        public Wifi Wifi;
    }

    [StructLayout(LayoutKind.Sequential)]
    /**
     * 电池信息
     */
    public struct Battery
    {
        public bool Low;
        public float Percentage;
        public float Voltage; // in volts

        public override string ToString()
        {
            return string.Format("{{Low:{0} Percentage:{1} Voltage:{2}}}", Low, Percentage, Voltage);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    /**
     * 三方向速度
     */
    public struct Vector3
    {
        public float X; // meter/second
        public float Y; // meter/second
        public float Z; // meter/second

        public override string ToString()
        {
            return string.Format("{{X:{0} Y:{1} Z:{2}}}", X, Y, Z);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    /**
     * 电机速度 
     */
    public struct Magneto
    {
        public Vector3 Rectified;
        public Vector3 Offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    /**
     * wifi信号质量
     */ 
    public struct Wifi
    {
        public float LinkQuality; // 1 is perfect, less than 1 is worse

        public override string ToString()
        {
            return string.Format("{{LinkQuality:{0}}}", LinkQuality);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    /*
     * 视频流编号
     */ 
    public struct Video
    {
        public uint FrameNumber;

        public override string ToString()
        {
            return string.Format("{{FrameNumber:{0}}}", FrameNumber);
        }
    }
}