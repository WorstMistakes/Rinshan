using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Rinshan
{
    // The F2 menu: a standalone window drawn with HTML (src/panel.html). It can be moved anywhere, even to
    // another monitor, and closing it only hides it so it reopens instantly with F2.
    class MenuPanel : Form
    {
        readonly WebView2 web = new WebView2();
        readonly JavaScriptSerializer json = new JavaScriptSerializer();
        string pendingState;
        bool pageReady;

        public bool AllowClose;
        public event Action<Dictionary<string, object>> Command;

        public MenuPanel(CoreWebView2Environment env)
        {
            Text = Lang.T("Rinshan — Menú", "Rinshan — Menu", "Rinshan — Menu");
            StartPosition = FormStartPosition.Manual;
            MinimumSize = new Size(760, 480);
            BackColor = Color.FromArgb(0x14, 0x10, 0x19);

            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = BackColor;
            Controls.Add(web);

            FormClosing += (s, e) =>
            {
                if (AllowClose || e.CloseReason != CloseReason.UserClosing) return;
                e.Cancel = true;
                Hide();
            };
            Load += async (s, e) =>
            {
                await web.EnsureCoreWebView2Async(env);
                var core = web.CoreWebView2;
                core.Settings.AreDefaultContextMenusEnabled = false;
                core.Settings.IsZoomControlEnabled = false;
                core.Settings.IsStatusBarEnabled = false;
                core.Settings.AreBrowserAcceleratorKeysEnabled = false;
                core.WebMessageReceived += (s2, e2) =>
                {
                    Dictionary<string, object> msg;
                    try { msg = json.Deserialize<Dictionary<string, object>>(e2.TryGetWebMessageAsString()); }
                    catch { return; }
                    if (msg != null && Command != null) Command(msg);
                };
                core.NavigationCompleted += (s2, e2) =>
                {
                    pageReady = true;
                    if (pendingState != null) Render(pendingState);
                };
                core.NavigateToString(Program.LoadResource("Rinshan.panel.html"));
            };
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        // Dark title bar to match the page (attribute 20 on current Windows 10/11, 19 on older builds)
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int on = 1;
            if (DwmSetWindowAttribute(Handle, 20, ref on, 4) != 0) DwmSetWindowAttribute(Handle, 19, ref on, 4);
        }

        public void Render(object state)
        {
            Render(json.Serialize(state));
        }

        void Render(string stateJson)
        {
            pendingState = stateJson;
            if (pageReady) web.CoreWebView2.ExecuteScriptAsync("render(" + stateJson + ")");
        }

        public void ShowAndFocus()
        {
            if (!Visible) Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            web.Focus();
        }
    }
}
