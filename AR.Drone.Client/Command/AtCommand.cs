/*
 * ʱ�� 2015 3 25
 * ע�� ����˧
 *
 * ʹ�ñ�����Ҫʵ��ToAt����
 * �����뽫�����������ֵת��ΪAT����ָ��
 * ����CreatePayload���Է���һ��byte������
 * ��Ҫ������͵��ļ�
 * 
 */
using System.Text;

namespace AR.Drone.Client.Command
{
    public abstract class AtCommand
    {
        protected abstract string ToAt(int sequenceNumber);

        public byte[] CreatePayload(int sequenceNumber)
        {
            string at = ToAt(sequenceNumber);
            byte[] payload = Encoding.ASCII.GetBytes(at);
            return payload;
        }
    }
}