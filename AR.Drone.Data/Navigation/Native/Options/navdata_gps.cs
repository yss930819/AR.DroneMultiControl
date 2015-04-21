using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AR.Drone.Data.Navigation.Native.Options
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct navdata_gps_t
    {
        public ushort tag;
        public ushort size;
        public double latitude;
        public double longitude;
    }
}
