using System;
using System.Threading;
using System.Windows.Forms;
using ShadowLogger.bin;

namespace ShadowLogger
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        private static readonly Mutex Mutex = new Mutex(true, "DIFFERENT NAME");


        [STAThread]
        private static void Main()
        {
            if (Mutex.WaitOne(TimeSpan.Zero, true))
            {
                Mutex.ReleaseMutex();
            }
            else
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
