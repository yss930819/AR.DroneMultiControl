/*
 * 时间 2015 3 26
 * 注释 杨率帅
 * 
 * 本类从WokerBase继承
 * 实现了Loop虚函数
 * 本代码为Command指令发送类
 * 发送指令是在
 * 在构造时，要传入command序列 和 网络配置信息
 * 
 * 
 */



using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using AR.Drone.Infrastructure;

namespace AR.Drone.Client.Command
{
    public class CommandSender : WorkerBase
    {
        #region 变量声明
        //飞机指令接收端口
        public const int CommandPort = 5556;
        //保持飞行
        public const int KeepAliveTimeout = 20;

        //指令序列
        private readonly ConcurrentQueue<AtCommand> _commandQueue;
        //网络基本配置
        private readonly NetworkConfiguration _configuration;
        #endregion



        /// <summary>
        ///  构造函数，需要传入指令序列和网络配置
        /// </summary>
        /// <param name="configuration">指令序列</param>
        /// <param name="commandQueue">网络配置</param>
        /// <returns></returns>
        public CommandSender(NetworkConfiguration configuration, ConcurrentQueue<AtCommand> commandQueue)
        {
            _configuration = configuration;
            _commandQueue = commandQueue;
        }

        /// <summary>
        /// 私有函数，用于添加指令到stream中
        /// </summary>
        /// <param name="stream">连接stream</param>
        /// <param name="command">要添加的指令</param>
        /// <param name="sequenceNumber">序列指令数量 使用的是引用</param>
        /// <returns></returns>
        private void AddCommand(Stream stream, AtCommand command, ref int sequenceNumber)
        {
            byte[] payload = command.CreatePayload(sequenceNumber);
            //追踪输入的command 不是 ComWdgCommand的情况
            Trace.WriteIf((command is ComWdgCommand) == false, Encoding.ASCII.GetString(payload));
            stream.Write(payload, 0, payload.Length);
            sequenceNumber++;
        }

        /// <summary>
        /// 重写方法Loop
        /// 将发送数据指令使用线程启动
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override void Loop(CancellationToken token)
        {
            int sequenceNumber = 1;

            //使用using方便之后释放udpClient 资源
            using (var udpClient = new UdpClient(CommandPort))
            {
                udpClient.Connect(_configuration.DroneHostname, CommandPort);

                byte[] firstMessage = BitConverter.GetBytes(1);
                udpClient.Send(firstMessage, firstMessage.Length);

                _commandQueue.Enqueue(ComWdgCommand.Default);
                Stopwatch swKeepAlive = Stopwatch.StartNew();

                //等待取消信息用以关闭线程
                while (token.IsCancellationRequested == false)
                {
                    bool comWdgCommandNeeded = swKeepAlive.ElapsedMilliseconds > KeepAliveTimeout;
                    if (_commandQueue.Count > 0 || comWdgCommandNeeded)
                    {
                        using (var ms = new MemoryStream())
                        {
                            if (comWdgCommandNeeded) 
                            {
                                AddCommand(ms, ComWdgCommand.Default, ref sequenceNumber);
                                swKeepAlive.Restart();
                            }

                            AtCommand command;
                            //从列表中读取指令并添加到流中
                            while (_commandQueue.TryDequeue(out command))
                            {
                                AddCommand(ms, command, ref sequenceNumber);
                            }

                            byte[] fullPayload = ms.ToArray();
                            udpClient.Send(fullPayload, fullPayload.Length);
                        }
                    }

                    Thread.Sleep(5);
                }
            }
        }
    }
}