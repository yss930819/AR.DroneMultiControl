/*
 * 时间 2015 3 23
 * 注释 杨率帅
 *
 * 
 * 本类从IDisposable继承，并实现此接口
 * 本类主要用来释放
 * 非托管的资源，比如文件句柄，用于线程同步的Mutex对象
 * 使用者实现 DisposeOverride()方法
 * 在其中清理非托管资源
 */


using System;

namespace AR.Drone.Infrastructure
{
    public abstract class DisposableBase : IDisposable
    {
        //资源被清理标记。
        private bool _disposed;
        
        /// <summary>
        /// 调用本方法以清理该类的飞托管资源
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && _disposed == false)
            {
                DisposeOverride();

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 实现该方法以清理非托管资源
        /// </summary>
        /// <returns></returns>
        protected abstract void DisposeOverride();
    }
}