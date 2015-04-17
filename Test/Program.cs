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

namespace Test
{
    class Program
    {

        static unsafe void Main(string[] args)
        {
            ViconClient v = new ViconClient();
            
            v.init();
            int i = 0;
            while(!i.Equals(100))
            {
                v.MyRecieve();
                position _p = *(position*) v.pos;
                Console.WriteLine(_p.times);
                i++;                
            }
            

            Console.ReadKey();
        }
    }
}
