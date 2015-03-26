/*
 * 时间 2015 3 23
 * 注释 杨率帅
 *
 * 本代码为很多类的基本类
 * 在AR.Drone.Infrastructure命名空间
 * 从 DisposableBase 继承 实现DisposeOverride方法以便清理非托管资源
 * 
 * 这是一个线程借口类，提供了线程的基本操作，并释放了线程
 * 
 */

using System;
using System.Diagnostics;
using System.Threading;

namespace AR.Drone.Infrastructure
{
    public abstract class WorkerBase : DisposableBase
    {

        //通知 CancellationToken，告知其应被取消。
        //处理超时任务的取消
        private CancellationTokenSource _cancellationTokenSource;

        //判断是否存活
        public bool IsAlive
        {
            get { return _cancellationTokenSource != null; }
        }

        
        public event Action<Object, Exception> UnhandledException;


        /// <summary>
        /// 启动线程
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            if (_cancellationTokenSource != null)
                return;
            lock (this)
            {
                if (_cancellationTokenSource != null)
                    return;

                _cancellationTokenSource = new CancellationTokenSource();

                var thread = new Thread(RunLoop) {Name = GetType().Name};
                thread.Start();
            }
        }

        
        /// <summary>
        /// 停止线程
        /// </summary>
        /// <returns></returns>
        public void Stop()
        {
            if (_cancellationTokenSource == null)
                return;
            lock (this)
            {
                if (_cancellationTokenSource == null)
                    return;

                _cancellationTokenSource.Cancel();
            }
        }


        /// <summary>
        /// 等待线程结束！！！
        /// </summary>
        /// <returns></returns>
        public void Join()
        {
            while (IsAlive) Thread.Sleep(1);
        }

        

        /// <summary>
        /// 线程运行代码
        /// 放出接口函数Loop供使用者实现线程中需要的功能
        /// </summary>
        /// <returns></returns>
        private void RunLoop()
        {
            try
            {
                CancellationToken token = _cancellationTokenSource.Token;
                Loop(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                OnException(exception);
            }
            finally
            {
                lock (this)
                {
                    CancellationTokenSource cancellationTokenSource = _cancellationTokenSource;
                    _cancellationTokenSource = null;
                    cancellationTokenSource.Dispose();
                }
            }
        }

        /// <summary>
        /// 虚函数让使用者完成实现其在线程中想完成的
        /// token已给出，不需要传递
        /// 
        /// </summary>
        /// <param name="token">由CancellationTokenSource得到的Token</param>
        /// <returns></returns>
        protected abstract void Loop(CancellationToken token);


        /// <summary>
        /// 处理除OperationCanceledException以外错误
        /// </summary>
        /// <param name="exception">错误类</param>
        /// <returns></returns>
        protected virtual void OnException(Exception exception)
        {
            Trace.TraceError("{0} - Exception: {1}", GetType(), exception.Message);
            Trace.TraceError(exception.StackTrace);

            OnUnhandledException(exception);
        }

        /// <summary>
        /// 放出错误给实现这个借口的函数去实现错误处理
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual void OnUnhandledException(Exception exception)
        {
            if (UnhandledException != null)
                UnhandledException(this, exception);
        }

        /// <summary>
        /// 实现DisposableBase类的仿真
        /// </summary>
        /// <returns></returns>
        protected override void DisposeOverride()
        {
            Stop();
        }
    }
}