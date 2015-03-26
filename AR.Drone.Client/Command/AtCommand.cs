/*
 * 时间 2015 3 25
 * 注释 杨率帅
 *
 * 使用本函数要实现ToAt方法
 * 本代码将输入的序列数值转化为AT控制指令
 * 调用CreatePayload可以反回一个byte的数组
 * 即要打包发送的文件
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