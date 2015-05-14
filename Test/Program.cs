using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using System.Configuration;
using AR.Drone.Client;
using AR.Drone.Vicon;
using AR.Drone.MyTool;
using TeamworkAlgorithm;

namespace Test
{
    class Program
    {

        static unsafe void Main(string[] args)
        {
            //PositionClient v = new PositionClient();
            
            //v.initSocket();
            //int i = 0;
            //while(!i.Equals(100))
            //{
            //    v.RecevieData();
            //    AR.Drone.MyTool.diyPosition _p = *( AR.Drone.MyTool.diyPosition *) v.getPosition1();
            //    Console.WriteLine("1:" + _p.x + "," + _p.y + "," + _p.z + ",");
            //    _p = *(AR.Drone.MyTool.diyPosition*)v.getPosition2();
            //    Console.WriteLine("2:" + _p.x + "," + _p.y + "," + _p.z + ",");
            //    Console.WriteLine(i);
            //    i++;                
            //}

            TestRect t = new TestRect();
            UVATeam u = new UVATeam(3);
            u.init(t);
            TaskLocation.location(u);
            

            Console.ReadKey();
        }
    }
}
