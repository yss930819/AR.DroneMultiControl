using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AR.Drone.MyTool
{

    [StructLayout(LayoutKind.Sequential)]
	public struct position
{
    public double times;
    public double X1;
    public double Y1;
    public double Z1;
    public double X2;
    public double Y2;
    public double Z2;
    public double X3;
    public double Y3;
    public double Z3;
    public double X4;
    public double Y4;
    public double Z4;
	public double TX;
	public double TY;
	public double TZ;
	public double EX;
	public double EY;
	public double EZ;

};
}
