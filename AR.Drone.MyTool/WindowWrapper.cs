using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR.Drone.MyTool
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr intPtr)
        {
            this._hwnd = intPtr;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }
}
