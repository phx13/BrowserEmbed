using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Design;
using System.Threading;

namespace winform主体
{
    public class ProcessHelper
    {
        #region 变量定义
        EventHandler exeEvent = null;
        Control ctrl = null;
        string infoArgs="";
        Process p = null;
        Hashtable processWnd = null;

        delegate bool EnumDelegate(IntPtr hwnd, uint IParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hwndChild, IntPtr hwndParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern long MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", EntryPoint = "IsWindow")]
        private static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "EnumWindows", SetLastError = true)]
        private static extern bool EnumWindows(EnumDelegate enumEvent, uint IParam);

        [DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hwnd, ref uint processId);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hwnd, int index, Int32 dwnewlong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        private static extern Int32 GetWindowLong(IntPtr hwnd, int index);
       
        public bool isStarted
        {
            get
            {
                return (this.p != null);
            }
        }

        private const int GWL_STYLE = -16;
        private const Int32 WS_CAPTION = 12582912;
        private const Int32 WS_THICKFRAME = 262144;

        #endregion
        
        #region 初始化
        public ProcessHelper(Control c, string info)
        {
            exeEvent=new EventHandler(ExeIdle);
            ctrl=c;
            infoArgs = info;
            processWnd = new Hashtable();
        }
        #endregion

        #region 开启进程
        public IntPtr Start(string exeName)
        {
            if (p != null)
            {
                Stop();
            }
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(exeName);
                info.UseShellExecute = true; 
                info.Arguments = infoArgs;
                p = Process.Start(info);
                p.WaitForInputIdle();
                Application.Idle += exeEvent;
            }
            catch
            {
                if (p != null)
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        p = null;
                    }
                }
            }
            return p.Handle;
        }
        #endregion

        #region 句柄
        void ExeIdle(object sender,EventArgs e)
        {
            if (this.p==null||this.p.HasExited)
            {
                this.p = null;
                Application.Idle -= exeEvent;
                return;
            }
            while (p.MainWindowHandle==IntPtr.Zero)
            {
                Thread.Sleep(100);
                p.Refresh();
            }
            Application.Idle-=exeEvent;
            Input(ctrl);
        }
        #endregion

        #region 终止
        public void Stop()
        {
            if (isStarted)
	        {
		        try 
	            {	        
		            if (!p.HasExited)
	                {
		                p.Kill();
	                }
	            }
	            catch (Exception)
	            {

	            }
                p=null;
	        }
        }
        #endregion

        #region 嵌入
        private void Input(Control cont)
        {
            IntPtr hwnd = GetHandle();
            SetParent(hwnd, cont.Handle);
            Int32 style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, style & (~WS_CAPTION) & (~WS_THICKFRAME));
            MoveWindow(hwnd, 0, 0, cont.Width, cont.Height, true);
        }
        #endregion

        #region 获取句柄
        private IntPtr GetHandle()
        {
            IntPtr ptrWnd = IntPtr.Zero;
            uint uiPid = (uint)p.Id;
            object objWnd = processWnd[uiPid];
            if (objWnd!=null)
            {
                ptrWnd = (IntPtr)objWnd;
                if (ptrWnd!=IntPtr.Zero)
                {
                    return ptrWnd;
                }
                else
                {
                    ptrWnd = IntPtr.Zero;
                }
            }

            bool res = EnumWindows(new EnumDelegate(EnumEvent), uiPid);
            if (!res)
            {
                objWnd = processWnd[uiPid];
                if (objWnd!=null)
                {
                    ptrWnd = (IntPtr)objWnd;
                }
            }
            return ptrWnd;
        }

        private bool EnumEvent(IntPtr hwnd, uint IParam)
        {
            uint uiPid=0;
            GetWindowThreadProcessId(hwnd, ref uiPid);
            if (uiPid == IParam)
            {
                processWnd[uiPid] = hwnd;
                return false;
            }
            return true;
        }
        #endregion
    }
}
