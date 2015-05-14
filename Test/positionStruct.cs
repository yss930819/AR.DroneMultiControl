using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AR.Drone.MyTool
{

    [StructLayout(LayoutKind.Sequential)]
    public struct diyPosition
    {
        public double x;
        public double y;
        public double z;
    };
}
