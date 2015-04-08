/*
 * 时间 2015 3 23
 * 注释 杨率帅
 *
 * 本代码为应用程序的入口类
 */


using System;
using System.Windows.Forms;
using AR.Drone.Infrastructure;

namespace AR.Drone.WinApp
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        ///     这是应用程序的主入口
        /// </summary>
        [STAThread]
       
        private static void Main()
        {
            /**
             * 判断程序的运行平台
             * 根据不同的运行平台选择对应的FFmpeg库文件
             */
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:

                    //此处为为FFmpeg库文件的地址
                    //string ffmpegPath = string.Format(@"FFmpeg/bin/windows/{0}", Environment.Is64BitProcess ? "x64" : "x86");
                    string ffmpegPath = string.Format(@"../../../FFmpeg/bin/windows/{0}", Environment.Is64BitProcess ? "x64" : "x86");

                    InteropHelper.RegisterLibrariesSearchPath(ffmpegPath);
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    string libraryPath = Environment.GetEnvironmentVariable(InteropHelper.LD_LIBRARY_PATH);
                    InteropHelper.RegisterLibrariesSearchPath(libraryPath);
                    break;
            }

            //启动程序主窗体
            Application.EnableVisualStyles();//使用系统的主题绘制窗体
            Application.SetCompatibleTextRenderingDefault(false);//兼容Net1.0 使用，直接设置false即可
            Application.Run(new MainForm());//启动窗体
        }
    }
}