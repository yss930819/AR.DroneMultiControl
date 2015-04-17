/*
 * 从Vicon获取数据的线程
 * 启动线程从Vicon获取位置数据
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using AR.Drone.Client;
using AR.Drone.Infrastructure;
using AR.Drone.Vicon;

namespace AR.Drone.MyTool
{
    public class MyViconPosition : WorkerBase
    {


        #region 成员变量
        private readonly PositionClient _positionClient;
        private readonly ViconClient _viconClient;
        #endregion

        //数据接收后发送事件响应
        public event Action OnViconPositionRecieve;


        public MyViconPosition(PositionClient _positionClient)
        {
            this._positionClient = _positionClient;
            _viconClient = null;
            _positionClient.initSocket();
        }

        public MyViconPosition(ViconClient _viconClient)
        {
            this._viconClient = _viconClient;
            _positionClient = null;
            this._viconClient.init();
        }

        protected override void Loop(System.Threading.CancellationToken token)
        {
            Trace.WriteLine("启动");
            while (token.IsCancellationRequested == false)
            {
                
                if (_positionClient != null)
                {
                     _positionClient.RecevieData();
                }
                if (_viconClient != null)
                {
                    _viconClient.MyRecieve();
                }
               
                if (OnViconPositionRecieve != null)
                {
                    OnViconPositionRecieve();
                }

                Thread.Sleep(10);
            }
        }


        protected override void DisposeOverride()
        {
            base.DisposeOverride();
        }
    }
}
