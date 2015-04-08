using System;

namespace AR.Drone.Client.Command
{
    [Flags]
    public enum FlightMode
    {
        /// <summary>
        /// The hover.
        /// 悬停
        /// </summary>
        Hover = 0,

        /// <summary>
        /// The progressive is a flight motion enabling bit.
        /// </summary>
        Progressive = 1 << 0,

        /// <summary>
        /// The combined yaw.
        /// </summary>
        CombinedYaw = 1 << 1,

        /// <summary>
        /// The absolute control.
        /// 绝对控制
        /// </summary>
        AbsoluteControl = 1 << 2
    }
}