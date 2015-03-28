/*
 * ʱ�� 2015 3 27
 * ע�� ����˧
 *
 * ������Ϊ ��ȡ�ɻ�����������data����
 * ��WorkerBase ��̳� ��Ҫʵ��Loop�麯��������Լ���
 * 
 * 
 */

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AR.Drone.Infrastructure;
using AR.Drone.Data;

namespace AR.Drone.Client.Navigation
{
    public class NavdataAcquisition : WorkerBase
    {
        #region ��Ա����

        //���ݻ�ȡ�˿�
        public const int NavdataPort = 5554;
        //
        public const int KeepAliveTimeout = 200;
        //��ȡnavdata�ĳ�ʱʱ��
        public const int NavdataTimeout = 2000;

        //������ַ �ɹ���ʱ����
        private readonly NetworkConfiguration _configuration;
        //�����ݽ��յ�ʱ����Ӧ
        private readonly Action<NavigationPacket> _packetAcquired;
        //��Ӧ��ʼǰί��
        private readonly Action _onAcquisitionStarted;
        //��Ӧ������ί��
        private readonly Action _onAcquisitionStopped;

        //�ж��Ƿ����ڻ�ȡ
        private bool _isAcquiring;

        #endregion


        /// <summary>
        /// ���캯��
        /// ����ʱҪ��readonly������ֵ
        /// </summary>
        /// <param name="configuration">host����</param>
        /// <param name="packetAcquired">��Ӧί��</param>
        /// <param name="onAcquisitionStarted">��Ӧ��ʼǰί��</param>
        /// <param name="onAcquisitionStopped">��Ӧ������ί��</param>
        /// <returns></returns>
        public NavdataAcquisition(NetworkConfiguration configuration, Action<NavigationPacket> packetAcquired, Action onAcquisitionStarted,
                                  Action onAcquisitionStopped)
        {
            _configuration = configuration;
            _packetAcquired = packetAcquired;
            _onAcquisitionStarted = onAcquisitionStarted;
            _onAcquisitionStopped = onAcquisitionStopped;
        }

        public bool IsAcquiring
        {
            get { return _isAcquiring; }
        }

        /// <summary>
        /// �߳�
        /// ������ɻ�ͨѶ��ȡ����
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override void Loop(CancellationToken token)
        {
            _isAcquiring = false;
            //�½�UDP�ͻ��˶˿�
            using (var udpClient = new UdpClient(NavdataPort))
                try
                {
                    //��ɻ�����
                    udpClient.Connect(_configuration.DroneHostname, NavdataPort);

                    //������ȷ�����ӿ���
                    SendKeepAliveSignal(udpClient);
                    //�����κ�ip����NavdataPort������
                    var remoteEp = new IPEndPoint(IPAddress.Any, NavdataPort);

                    //��λ��ʱʱ��ı���
                    Stopwatch swKeepAlive = Stopwatch.StartNew();
                    Stopwatch swNavdataTimeout = Stopwatch.StartNew();
                    while (token.IsCancellationRequested == false && swNavdataTimeout.ElapsedMilliseconds < NavdataTimeout)
                    {
                        //udp�ͻ��˲�����ʹ��ʱ���߳�����
                        if (udpClient.Available == 0)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            //�������ݣ�����ʽ
                            byte[] data = udpClient.Receive(ref remoteEp);
                            //�����µ�packet
                            var packet = new NavigationPacket
                                {
                                    Timestamp = DateTime.UtcNow.Ticks,
                                    Data = data
                                };
                            //������ʱ��ʱ��
                            swNavdataTimeout.Restart();

                            //�������ڻ�ȡ״̬Ϊ��
                            _isAcquiring = true;
                            _onAcquisitionStarted();

                            //�����Ի�ȡ���İ�
                            _packetAcquired(packet);
                        }

                        if (swKeepAlive.ElapsedMilliseconds > KeepAliveTimeout)
                        {
                            SendKeepAliveSignal(udpClient);
                            swKeepAlive.Restart();
                        }
                    }
                }
                finally
                {
                    //��֮ǰ״̬Ϊ���ڻ�ȡ
                    //�ͽ���رգ��������¼�
                    if (_isAcquiring)
                    {
                        _isAcquiring = false;
                        _onAcquisitionStopped();
                    }
                }
        }

        
        /// <summary>
        /// ���ʹ����Ϣ
        /// ����1
        /// </summary>
        /// <param name="udpClient">���ӺõĿͻ���</param>
        /// <returns></returns>
        private void SendKeepAliveSignal(UdpClient udpClient)
        {
            byte[] payload = BitConverter.GetBytes(1);
            try
            {
                udpClient.Send(payload, payload.Length);
            }
            catch (SocketException)
            {
            }
        }
    }
}