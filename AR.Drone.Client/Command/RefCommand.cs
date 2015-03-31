/*
 * 时间 2015 3 30
 * 注释 杨率帅
 * 
 * 从AtCommand 类继承
 * 实现了Land
 * takeoff
 * 和 Emergency几种状态
 * 
 */

namespace AR.Drone.Client.Command
{
    public class RefCommand : AtCommand
    {
        public static AtCommand Land = new RefCommand(RefMode.Land);
        public static AtCommand Takeoff = new RefCommand(RefMode.Takeoff);
        public static AtCommand Emergency = new RefCommand(RefMode.Emergency);

        private readonly RefMode _refMode;

        private RefCommand(RefMode refMode)
        {
            _refMode = refMode;
        }

        protected override string ToAt(int sequenceNumber)
        {
            return string.Format("AT*REF={0},{1}\r", sequenceNumber, (int) _refMode);
        }
    }
}