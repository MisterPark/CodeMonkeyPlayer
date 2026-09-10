using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeMonkeyPlayer
{
    internal static class Program
    {
        private static System.Threading.Timer shutdownGuard;
        private static void StartShutdownGuard()
        {
            shutdownGuard = new System.Threading.Timer(_ => System.Diagnostics.Process.GetCurrentProcess().Kill(),
                null, 3000, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new PlayerApplication().Run(Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        private sealed class PlayerApplication : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
        {
            public PlayerApplication() { IsSingleInstance = true; EnableVisualStyles = true; ShutdownStyle = Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses; }
            protected override void OnCreateMainForm()
            {
            var form = new Form1();
            form.ShutdownStarted = () =>
            {
                // Only the standalone player owns this escape hatch. Settings have
                // already been flushed; a deadlocked native codec must not keep it alive.
                StartShutdownGuard();
            };
            MainForm = form;
            }
            protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
            {
                base.OnStartupNextInstance(e);
                if (MainForm is Form1 form && !form.IsDisposed) form.ReceiveFiles(e.CommandLine.ToArray());
                e.BringToForeground = true;
            }
        }
    }
}
