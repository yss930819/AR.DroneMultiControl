/*
 * ʱ�� 2015 3 30
 * ע�� ����˧
 * 
 * ��AtCommand ��̳�
 * ʵ����Land
 * takeoff
 * �� Emergency����״̬
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