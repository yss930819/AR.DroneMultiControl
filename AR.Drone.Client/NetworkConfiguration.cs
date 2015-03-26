/*
 * 时间 2015 3 26
 * 注释 杨率帅
 * 
 * 基本类只有一个变量DroneHostname
 * 使用时DroneHostname 的set方法不可调用
 * 只能在初始化时赋值
 * 
 */

namespace AR.Drone.Client
{
    public class NetworkConfiguration
    {
        public NetworkConfiguration(string hostname)
        {
            DroneHostname = hostname;
        }

        public string DroneHostname { get; private set; }
    }
}