using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using AR.Drone.Client;

namespace Test
{
    class Program
    {

        static void Main(string[] args)
        {
            bool flag = true;
            int i = 0;
            PositionClient _p = new PositionClient();
            _p.initSocket();
            while (flag)
            {
                _p.RecevieData();

                i++;

                //if (i >= 10)
                //{
                //    flag = false;
                //}
            }

            Console.ReadKey();
        }
    }
}
