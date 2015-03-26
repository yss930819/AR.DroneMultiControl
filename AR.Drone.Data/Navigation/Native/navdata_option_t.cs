/*
 * 时间 2015 3 26
 * 注释 杨率帅
 * 
 * 导航数据选择结构体。
 * 由两个变量：tag 和 size
 * tag用于区分数据类型
 */

using System.Runtime.InteropServices;

namespace AR.Drone.Data.Navigation.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct navdata_option_t
    {
        public ushort tag;
        public ushort size;
        // public byte* data; no need to map will be processed manually
    }
}