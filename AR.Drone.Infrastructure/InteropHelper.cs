/*
 * 时间 2015 3 23
 * 注释 杨率帅
 *
 * 本类为辅助工具类
 * 用来设置FFmpeg库的准确位置
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AR.Drone.Infrastructure
{
    public static class InteropHelper
    {
        public const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH"; //字符串常量

        public static void RegisterLibrariesSearchPath(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    SetDllDirectory(path);
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    string currentValue = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH) ?? string.Empty;
                    string newValue = string.IsNullOrEmpty(currentValue) ? path : currentValue + Path.PathSeparator + path;
                    Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, newValue);
                    break;
            }
        }

        
        [DllImport("kernel32", SetLastError = true)]
        /// <summary>
        /// 调用系统本身的动态链接库，设置本程序需要调用的动态链接库
        /// </summary>
        /// <param name="lpPathName">要设置的链接库的地址</param>
        /// <returns></returns>
        public static extern bool SetDllDirectory(string lpPathName);
    }
}