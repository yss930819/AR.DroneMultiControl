/*
 * ʱ�� 2015 3 26
 * ע�� ����˧
 * 
 * ��������ͷ��Ϣ�ṹ��
 */

using System.Runtime.InteropServices;

namespace AR.Drone.Data.Navigation.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct navdata_t
    {
        public uint header;
        public uint ardrone_state;
        public uint sequence;
        public int vision_defined;
        //public navdata_option_t[] options; no need to map will be processed manually
    }
}