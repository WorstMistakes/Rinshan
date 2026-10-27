using System;
using System.Diagnostics;
using System.Drawing;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Rinshan
{
    // Secondary window for replays and game reviews. Shares the main window's WebView2 profile,
    // so logins on Mortal/NAGA persist between uses.
    class BrowserWindow : Form
    {
        readonly WebView2 web = new WebView2();
        readonly CoreWebView2Environment env;
        readonly string startUrl;
        readonly string fillValue;   // when set, pasted into the review site's log field (never submitted)

        // Finds the log URL field on the page, fills it and highlights it. Retries for a while
        // because the review sites render their forms after load. It never clicks submit.
        const string FillScript = @"
(function(u){
  var tries = 0;
  function pick(){
    var radio = document.querySelector('input[name=""input-method""][value=""log-url""]');
    if (radio && !radio.checked) radio.click();
    var exact = document.querySelector('input[name=""log-url""]');
    if (exact) return exact;
    var els = [].slice.call(document.querySelectorAll('input[type=url],input[type=text],input:not([type]),textarea'))
      .filter(function(e){ return e.offsetParent !== null && !e.readOnly && !e.disabled; });
    return els.filter(function(e){ return /url|log|haifu|tenhou|牌譜/i.test((e.name||'')+(e.id||'')+(e.placeholder||'')); })[0];
  }
  function attempt(){
    var f = pick();
    if (!f) { if (++tries < 40) setTimeout(attempt, 250); return; }
    var proto = f.tagName == 'TEXTAREA' ? HTMLTextAreaElement.prototype : HTMLInputElement.prototype;
    Object.getOwnPropertyDescriptor(proto, 'value').set.call(f, u);
    f.dispatchEvent(new Event('input', {bubbles:true}));
    f.dispatchEvent(new Event('change', {bubbles:true}));
    f.style.outline = '3px solid #e5a00d';
    f.scrollIntoView({block:'center'});
  }
  attempt();
})";

        public BrowserWindow(CoreWebView2Environment env, string title, string url, string fillValue, Icon icon)
        {
            this.env = env;
            this.startUrl = url;
            this.fillValue = fillValue;

            Text = title;
            Icon = icon;
            Size = new Size(1200, 820);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;

            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, RenderMode = ToolStripRenderMode.System };
            if (fillValue != null)
            {
                bar.Items.Add(new ToolStripLabel(Lang.T("Partida: ", "Game: ", "Partida: ") + fillValue));
                bar.Items.Add(new ToolStripButton(Lang.T("Copiar enlace", "Copy link", "Copiar link"), null, (s, e) => Clipboard.SetText(fillValue)));
            }
            bar.Items.Add(new ToolStripButton(Lang.T("Abrir en el navegador", "Open in browser", "Abrir no navegador"), null, (s, e) =>
            {
                var current = web.Source != null ? web.Source.ToString() : startUrl;
                if (fillValue != null) Clipboard.SetText(fillValue);
                OpenExternal(current);
            }));
            if (fillValue != null)
                bar.Items.Add(new ToolStripLabel(Lang.T(
                    "El enlace se pega solo. Revisa que sea la partida correcta y pulsa el botón de la página.",
                    "The link is pasted for you. Check it's the right game, then press the button on the page.",
                    "O link já é colado. Confira se é a partida certa e aperte o botão da página."))
                    { ForeColor = SystemColors.GrayText });

            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = Color.Black;
            Controls.Add(web);
            Controls.Add(bar);

            Load += async (s, e) =>
            {
                await web.EnsureCoreWebView2Async(env);
                web.CoreWebView2.Settings.IsStatusBarEnabled = false;
                web.CoreWebView2.DocumentTitleChanged += (s2, e2) =>
                {
                    if (!string.IsNullOrEmpty(web.CoreWebView2.DocumentTitle)) Text = web.CoreWebView2.DocumentTitle;
                };
                // Keep popups (e.g. a login window) inside the app, linked to their opener so logins complete
                web.CoreWebView2.NewWindowRequested += async (s2, e2) =>
                {
                    var deferral = e2.GetDeferral();
                    var popup = new BrowserWindow(env, Text, null, null, Icon);
                    popup.Show();
                    await popup.ready.Task;
                    e2.NewWindow = popup.web.CoreWebView2;
                    deferral.Complete();
                };
                web.CoreWebView2.WindowCloseRequested += (s2, e2) => Close();
                web.CoreWebView2.NavigationCompleted += (s2, e2) => FillLogField();
                ready.SetResult(true);
                if (startUrl != null) web.CoreWebView2.Navigate(startUrl);
            };
        }

        readonly System.Threading.Tasks.TaskCompletionSource<bool> ready = new System.Threading.Tasks.TaskCompletionSource<bool>();

        void FillLogField()
        {
            if (fillValue == null) return;
            var arg = new JavaScriptSerializer().Serialize(fillValue);
            web.CoreWebView2.ExecuteScriptAsync(FillScript + "(" + arg + ");");
        }

        public static void OpenExternal(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        }
    }
}
