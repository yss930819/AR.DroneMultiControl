using AR.Drone.Data.Navigation;
using MyDrone;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TestTwoDrone
{
    public class TestViewItem : INotifyPropertyChanged
    {
        #region 参数改变时发送通知
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region 公开字段(用于ListView获取)
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                NotifyPropertyChanged("id");
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged("name");
            }
        }
        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
                NotifyPropertyChanged("ip");
            }
        }

        public bool IsConected
        {
            get { return drone.IsConnected; }
        }
        public float DroneBattery
        {
            get
            {
                if (navigationData != null)
                    return navigationData.Battery.Percentage;
                else
                    return -1;
            }
        }

        public void Updata()
        {
            NotifyPropertyChanged("DroneBattery");
            NotifyPropertyChanged("IsConected");
        }

        #endregion

        private int id;
        private string name;
        private string ip;
        public MyDroneClient drone;
        public NavigationData navigationData;

        //自动控制参数
        public float currentX = 0;
        public float currentY = 0;
        public float currentZ = 0;

        public float _aimX;
        public float _aimY;
        public float _aimZ;

        public float _controlX = 0;
        public float[] Xerror = { 0, 0 };
        public float Xerror_dot;
        public float inte_x = 0;
        public float P_pitch = -0.2f;
        public float D_pitch = -0.3f;
        public float I_pitch = -0.3f;

        public float _controlY = 0;
        public float[] Yerror = { 0, 0 };
        public float Yerror_dot;
        public float inte_y = 0;
        public float P_roll = -0.2f;
        public float D_roll = -0.3f;
        public float I_roll = -0.2f;

        public float _controlZ = 0;
        public float P_gaz = -0.4f;
    }
}
