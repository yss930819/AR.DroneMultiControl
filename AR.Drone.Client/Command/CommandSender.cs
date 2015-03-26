/*
 * ʱ�� 2015 3 26
 * ע�� ����˧
 * 
 * �����WokerBase�̳�
 * ʵ����Loop�麯��
 * ������ΪCommandָ�����
 * ����ָ������
 * �ڹ���ʱ��Ҫ����command���� �� ����������Ϣ
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
        #region ��������
        //�ɻ�ָ����ն˿�
        public const int CommandPort = 5556;
        //���ַ���
        public const int KeepAliveTimeout = 20;

        //ָ������
        private readonly ConcurrentQueue<AtCommand> _commandQueue;
        //�����������
        private readonly NetworkConfiguration _configuration;
        #endregion



        /// <summary>
        ///  ���캯������Ҫ����ָ�����к���������
        /// </summary>
        /// <param name="configuration">ָ������</param>
        /// <param name="commandQueue">��������</param>
        /// <returns></returns>
        public CommandSender(NetworkConfiguration configuration, ConcurrentQueue<AtCommand> commandQueue)
        {
            _configuration = configuration;
            _commandQueue = commandQueue;
        }

        /// <summary>
        /// ˽�к������������ָ�stream��
        /// </summary>
        /// <param name="stream">����stream</param>
        /// <param name="command">Ҫ��ӵ�ָ��</param>
        /// <param name="sequenceNumber">����ָ������ ʹ�õ�������</param>
        /// <returns></returns>
        private void AddCommand(Stream stream, AtCommand command, ref int sequenceNumber)
        {
            byte[] payload = command.CreatePayload(sequenceNumber);
            //׷�������command ���� ComWdgCommand�����
            Trace.WriteIf((command is ComWdgCommand) == false, Encoding.ASCII.GetString(payload));
            stream.Write(payload, 0, payload.Length);
            sequenceNumber++;
        }

        /// <summary>
        /// ��д����Loop
        /// ����������ָ��ʹ���߳�����
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override void Loop(CancellationToken token)
        {
            int sequenceNumber = 1;

            //ʹ��using����֮���ͷ�udpClient ��Դ
            using (var udpClient = new UdpClient(CommandPort))
            {
                udpClient.Connect(_configuration.DroneHostname, CommandPort);

                byte[] firstMessage = BitConverter.GetBytes(1);
                udpClient.Send(firstMessage, firstMessage.Length);

                _commandQueue.Enqueue(ComWdgCommand.Default);
                Stopwatch swKeepAlive = Stopwatch.StartNew();

                //�ȴ�ȡ����Ϣ���Թر��߳�
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
                            //���б��ж�ȡָ���ӵ�����
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