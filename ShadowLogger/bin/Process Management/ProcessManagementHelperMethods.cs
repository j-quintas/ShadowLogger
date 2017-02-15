using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ShadowLogger.bin.Process_Management
{
    internal static class ProcessManagementHelperMethods
    {

        #region " P/Invoke Declarations "

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessRights dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ProcessBasicInformation processInformation, int processInformationLength, ref int returnLength);

        #endregion

        #region " Enum's "

        [Flags()]
        private enum ProcessAccessRights
        {
            /// <summary>
            /// Required to create a thread.
            /// </summary>
            CreateThread = 0x2,

            /// <summary>
            ///
            /// </summary>
            SetSessionId = 0x4,

            /// <summary>
            /// Required to perform an operation on the address space of a process
            /// </summary>
            VmOperation = 0x8,

            /// <summary>
            /// Required to read memory in a process using ReadProcessMemory.
            /// </summary>
            VmRead = 0x10,

            /// <summary>
            /// Required to write to memory in a process using WriteProcessMemory.
            /// </summary>
            VmWrite = 0x20,

            /// <summary>
            /// Required to duplicate a handle using DuplicateHandle.
            /// </summary>
            DupHandle = 0x40,

            /// <summary>
            /// Required to create a process.
            /// </summary>
            CreateProcess = 0x80,

            /// <summary>
            /// Required to set memory limits using SetProcessWorkingSetSize.
            /// </summary>
            SetQuota = 0x100,

            /// <summary>
            /// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            /// </summary>
            SetInformation = 0x200,

            /// <summary>
            /// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken).
            /// </summary>
            QueryInformation = 0x400,

            /// <summary>
            /// Required to suspend or resume a process.
            /// </summary>
            SuspendResume = 0x800,

            /// <summary>
            /// Required to retrieve certain information about a process (see GetExitCodeProcess, GetPriorityClass, IsProcessInJob, QueryFullProcessImageName).
            /// A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION.
            /// </summary>
            QueryLimitedInformation = 0x1000,

            /// <summary>
            /// Required to wait for the process to terminate using the wait functions.
            /// </summary>
            Synchronize = 0x100000,

            /// <summary>
            /// Required to delete the object.
            /// </summary>
            Delete = 0x10000,

            /// <summary>
            /// Required to read information in the security descriptor for the object, not including the information in the SACL.
            /// To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
            /// </summary>
            ReadControl = 0x20000,

            /// <summary>
            /// Required to modify the DACL in the security descriptor for the object.
            /// </summary>
            WriteDac = 0x40000,

            /// <summary>
            /// Required to change the owner in the security descriptor for the object.
            /// </summary>
            WriteOwner = 0x80000,

            StandardRightsRequired = 0xf0000,

            Terminate = 0x1,

            /// <summary>
            /// All possible access rights for a process object.
            /// </summary>
            AllAccess = StandardRightsRequired | Synchronize | 0xffff
        }

        #endregion

        internal static IntPtr GetProcessHandle(int pid)
        {
            return OpenProcess(ProcessAccessRights.QueryLimitedInformation, false, pid);
        }

        internal static IntPtr GetProcessQueryLimitedInformationHandle(int pid)
        {
            return OpenProcess(ProcessAccessRights.QueryLimitedInformation, false, pid);
        }

        internal static IntPtr GetProcessTerminateHandle(int pid)
        {
            return OpenProcess(ProcessAccessRights.Terminate, false, pid);
        }

        internal static string GetProcessPath(IntPtr pHandle)
        {
            StringBuilder processpath = new StringBuilder(256);
            int capacity = processpath.Capacity;
            QueryFullProcessImageName(pHandle, 0, processpath, ref capacity);
            return processpath.ToString();
        }

        internal static string GetProcessMemory(Process p)
        {
            return GetSizeFromBytes(p.WorkingSet64);
        }

        private static string GetSizeFromBytes(long bytes)
        {
            if (bytes.ToString().Length >= 10)
            {
                return String.Format("{0:#.###} GB", (bytes/1024)/1000/1000);
            }
            else if (bytes.ToString().Length >= 7)
            {
                return String.Format("{0:#.###} MB", (bytes/1024)/1000);
            }
            else if (bytes.ToString().Length >= 4)
            {
                return String.Format("{0:#.###} KB", (bytes/1024));
            }
            else
            {
                return String.Format("{0} B", (bytes));
            }
        }


        #region " Parent Process ID "

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessBasicInformation
        {
            // These members must match PROCESS_BASIC_INFORMATION
            private readonly IntPtr Reserved1;
            private readonly IntPtr PebBaseAddress;
            private readonly IntPtr Reserved2_0;
            private readonly IntPtr Reserved2_1;
            private readonly IntPtr UniqueProcessId;
            internal readonly IntPtr InheritedFromUniqueProcessId;
        }

        internal static string GetParentProcessIdFromHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return "N/A";
            }

            ProcessBasicInformation pbi = new ProcessBasicInformation();
            int returnLength = 0;
            uint status = Convert.ToUInt32(NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), ref returnLength));

            if (status != 0)
            {
                return "NTStatus" + " " + status;
            }

            string returnValue = string.Empty;

            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.Id == pbi.InheritedFromUniqueProcessId.ToInt32())
                {
                    returnValue = proc.Id + " (" + proc.ProcessName + ")";
                }
                proc.Dispose();
            }

            return returnValue == string.Empty ? "NOT RUNNING" : returnValue;
        }

        #endregion
    }
}
