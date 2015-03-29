/*
 * 时间 2015 3 29
 * 注释 杨率帅
 *
 * 本代码为 视频回放窗体
 * 
 * 通过本窗体可以将之前存储的数据进行解码
 * 同时可以回放录制的视频
 * 
 */


using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AR.Drone.Data;
using AR.Drone.Video;
using AR.Drone.MyTool;

namespace AR.Drone.WinApp
{
    public partial class PlayerForm : Form
    {
        #region 成员变量
        private readonly VideoPacketDecoderWorker _videoPacketDecoderWorker;
        private string _fileName;
        //文件解析器
        private FilePlayer _filePlayer;

        private VideoFrame _frame;
        private Bitmap _frameBitmap;
        private decimal _frameNumber;

        private bool _isClosing = false;

        #region 文件储存
        //基本信息
        //文件所在位置
        private string _dir;
        //文件名
        private string _baseName;

        private bool _isWriter;
        private FileStream _totalFileStream;
        private StreamWriter _totalWriteStream;
        private MyWrite _fileWriter;
        #endregion

        #endregion




        /// <summary>
        /// 构造函数，初始化窗体
        /// </summary>
        /// <returns></returns>
        public PlayerForm()
        {
            InitializeComponent();

            _videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _videoPacketDecoderWorker.Start();

            tmrVideoUpdate.Enabled = true;

            _videoPacketDecoderWorker.UnhandledException += UnhandledException;
        }

        //传入要回放的文件
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                Text = Path.GetFileName(_fileName);
                _dir = Path.GetDirectoryName(_fileName);
                _baseName = Path.GetFileNameWithoutExtension(_fileName);
            }
        }


        /// <summary>
        /// 响应错误消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private void UnhandledException(object sender, Exception exception)
        {
            MessageBox.Show(exception.ToString(), "Unhandled Exception (Ctrl+C)", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void StartPlaying()
        {
            StopPlaying();
            if (_filePlayer == null) _filePlayer = new FilePlayer(_fileName, OnNavigationPacketAcquired, OnVideoPacketAcquired);
            _filePlayer.UnhandledException += UnhandledException;
            _filePlayer.OnFileEnd += OnFileEnd;
            _filePlayer.Start();
        }

        private void OnFileEnd()
        {
            _fileWriter.Stop();
            _fileWriter.Join();
        }

        private void OnWriteEnd()
        {
            StopWrite();
        }

        private void OnNavigationPacketAcquired(NavigationPacket obj)
        {
            if (_fileWriter != null && _isWriter)
            {
                _fileWriter.EnqueuePacket(obj);
            }
            if (_isWriter)
            {
                _totalWriteStream.WriteLine("**Nav**");
                _totalWriteStream.WriteLine("Nav:"+obj.Timestamp);
                _totalWriteStream.WriteLine();
               // _totalWriteStream.Flush();
            }
        }

        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (_fileWriter != null && _isWriter)
            {
                _fileWriter.EnqueuePacket(packet);
            }
             if (_isWriter)
            {
                _totalWriteStream.WriteLine("%%Vedio%%");
                _totalWriteStream.WriteLine("Vedio:"+packet.Timestamp);
                _totalWriteStream.WriteLine();
               // _totalWriteStream.Flush();
            }
            _videoPacketDecoderWorker.EnqueuePacket(packet);
            Thread.Sleep(20);
        }

        private void StopPlaying()
        {
            if (_filePlayer != null)
            {
                _filePlayer.Stop();
                _filePlayer.Join();
            }
        }

        private void StopWrite()
        {
            if (_isWriter)
            {
                _isWriter = false;

                
                //通过线程修改控件属性时要用如下方法
                //判断窗体未被释放
                if (!_isClosing)
                {
                    this.Invoke(new Action(() =>
                    {//当需要操作界面元素时，需要用Invoke，注意这里面不能有繁琐的操作

                        this.btnDecode.Text = "解析完成";
                        btnDecode.Enabled = true;
                        btnClose.Enabled = true;
                        btnReplay.Enabled = true;

                        //Thread.Sleep(1000);如果这么写，就会卡住主线程
                    }));
                }
                
                _totalWriteStream.WriteLine("-------------结束------------");
                //将缓存区写入文件
                _totalWriteStream.Flush();

                //释放资源
                if (_totalWriteStream != null)
                {
                    _totalWriteStream.Dispose();
                    _totalWriteStream = null;
                }
                if (_totalFileStream != null)
                {
                    _totalFileStream.Dispose();
                    _totalFileStream = null;
                }
            }
        }


        /// <summary>
        /// 重写显示方法
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
        }

        private void OnVideoPacketDecoded(VideoFrame frame)
        {
            _frame = frame;
        }

        private void tmrVideoUpdate_Tick(object sender, EventArgs e)
        {
            if (_frame == null || _frameNumber == _frame.Number)
                return;
            _frameNumber = _frame.Number;
            _totalWriteStream.WriteLine("%% frameNumber%%");
            _totalWriteStream.WriteLine("" + _frame.Number);

            if (_frameBitmap == null)
                _frameBitmap = VideoHelper.CreateBitmap(ref _frame);
            else
                VideoHelper.UpdateBitmap(ref _frameBitmap, ref _frame);

            pbVideo.Image = _frameBitmap;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnReplay_Click(object sender, EventArgs e)
        {
            StopPlaying();
            StartPlaying();
        }

        protected override void OnClosed(EventArgs e)
        {
            _isClosing = true;
            StopPlaying();
            StopWrite();

            _videoPacketDecoderWorker.Dispose();

            base.OnClosed(e);
        }

        /// <summary>
        /// 解析按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void btnDecode_Click(object sender, EventArgs e)
        {
            
            StopWrite();
            _isWriter = true;

            //写总文件
            string totalFile = string.Format(@"total_" + _baseName + ".tl");
            _totalFileStream = new FileStream(_dir + @"/" + totalFile, FileMode.OpenOrCreate);
            _totalWriteStream = new StreamWriter(_totalFileStream);
            _totalWriteStream.WriteLine("-------------开始------------");

            //将视频文件与数据文件分别写入
            _fileWriter = new MyWrite(_dir,_baseName);
            _fileWriter.OnWriteEnd += OnWriteEnd;
            _fileWriter.Start();


            btnDecode.Text = "解析中...";
            btnDecode.Enabled = false;
            btnClose.Enabled = false;
            btnReplay.Enabled = false;
   
            StartPlaying();

        }


    }
}