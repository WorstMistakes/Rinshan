using System;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Rinshan")]
[assembly: System.Reflection.AssemblyProduct("Rinshan")]
[assembly: System.Reflection.AssemblyVersion("1.4.0.0")]

namespace Rinshan
{
    static class Program
    {
        const string MutexName = "Rinshan.SingleInstance";
        const string ShowEventName = "Rinshan.ShowWindow";

        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (var mutex = new Mutex(true, MutexName, out createdNew))
            using (var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName))
            {
                if (!createdNew)
                {
                    // Already running: ask the existing window to come to the front and exit
                    showEvent.Set();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var form = new MainForm();

                var listener = new Thread(() =>
                {
                    while (true)
                    {
                        showEvent.WaitOne();
                        try { form.BeginInvoke(new Action(form.ShowFromTray)); } catch { return; }
                    }
                });
                listener.IsBackground = true;
                listener.Start();

                Application.Run(form);
            }
            // After an update, start the new exe once this instance is gone and has released its mutex
            if (RestartPath != null)
                try { System.Diagnostics.Process.Start(RestartPath); } catch { }
        }

        public static string RestartPath;

        // Files embedded with /resource in build.ps1 (panel.html, translate.js)
        public static string LoadResource(string name)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (var reader = new System.IO.StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
