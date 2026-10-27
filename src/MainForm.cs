using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Rinshan
{
    class MainForm : Form
    {
        // Keep Tenhou running at full speed while minimized or in the tray, so the match start and
        // turn timers are never delayed by Chromium's background throttling.
        // Tenhou paints the custom table background once into a cached canvas and never repaints it;
        // GPU canvases can be discarded while the window is minimized, so keep 2D canvases in software.
        const string BrowserArgs =
            "--disable-background-timer-throttling --disable-renderer-backgrounding --disable-backgrounding-occluded-windows" +
            " --disable-accelerated-2d-canvas";

        readonly WebView2 web = new WebView2();
        readonly Settings settings;
        readonly string dataDir;
        readonly NotifyIcon tray = new NotifyIcon();
        readonly ContextMenuStrip trayMenu = new ContextMenuStrip();
        readonly ToolStripMenuItem trayShow = new ToolStripMenuItem();
        readonly ToolStripMenuItem trayOpenMenu = new ToolStripMenuItem();
        readonly ToolStripMenuItem trayExit = new ToolStripMenuItem();
        string translateConfigScriptId;
        bool tenhouReloadNeeded;   // a language or translation change waits for Tenhou to reload
        CoreWebView2Environment env;
        MenuPanel panel;
        readonly DiscordPresence discord = new DiscordPresence();
        readonly TenhouPresence presence = new TenhouPresence();
        List<GameLog> logs = new List<GameLog>();

        bool fullscreen;
        FormWindowState stateBeforeFullscreen;
        Rectangle boundsBeforeFullscreen;
        FormWindowState stateBeforeTray = FormWindowState.Normal;
        bool confirmedExit;
        bool trayHintShown;
        bool balloonIsUpdate;   // clicking the update balloon opens the menu

        UpdateInfo update;
        string updateStatus = "idle";   // idle, checking, latest, available, installing, failed
        // The first check waits until Tenhou has loaded, then once every 6 hours
        readonly Timer updateTimer = new Timer { Interval = 15000 };

        public MainForm()
        {
            dataDir = DataDirectory();
            settings = new Settings(Path.Combine(dataDir, "settings.ini"));
            try { settings.Load(); } catch { }
            Lang.Set(settings.Language);

            Text = "Rinshan";
            BackColor = Color.Black;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            StartPosition = FormStartPosition.Manual;
            Bounds = IsOnScreen(settings.Bounds) ? settings.Bounds : new Rectangle(100, 100, 1280, 800);
            if (settings.Maximized) WindowState = FormWindowState.Maximized;

            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = Color.Black;
            Controls.Add(web);

            trayShow.Click += (s, e) => ShowFromTray();
            trayShow.Font = new Font(trayShow.Font, FontStyle.Bold);
            trayOpenMenu.Click += (s, e) => { ShowFromTray(); TogglePanel(); };
            trayOpenMenu.ShortcutKeyDisplayString = "F2";
            trayExit.Click += (s, e) => Close();
            trayMenu.Items.AddRange(new ToolStripItem[] { trayShow, trayOpenMenu, new ToolStripSeparator(), trayExit });
            ApplyLanguageTexts();
            tray.Icon = Icon;
            tray.Text = "Rinshan";
            tray.ContextMenuStrip = trayMenu;
            tray.Visible = true;
            tray.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) ShowFromTray(); };
            // Clicks on the game don't reach WinForms, so close the tray menu when the window is activated
            Activated += (s, e) => trayMenu.Close();
            tray.BalloonTipClicked += (s, e) => { if (balloonIsUpdate) { ShowFromTray(); if (panel == null || !panel.Visible) TogglePanel(); } };

            Updater.CleanUp();
            updateTimer.Tick += (s, e) =>
            {
                updateTimer.Interval = 6 * 60 * 60 * 1000;
                if (settings.CheckUpdates) CheckForUpdates(false);
            };
            updateTimer.Start();

            Load += async (s, e) => await InitWebView();
            Resize += (s, e) => OnWindowStateChanged();
            VisibleChanged += (s, e) => UpdateKeepAwake();
            FormClosing += OnClosing;
            FormClosed += (s, e) =>
            {
                tray.Visible = false;
                tray.Dispose();
                Native.KeepDisplayOn(false);
                discord.Dispose();
                if (panel != null) { panel.AllowClose = true; panel.Close(); }
            };
        }

        // %LOCALAPPDATA%\Rinshan holds the settings and the WebView2 profile (Tenhou login, its settings and
        // game history). Versions before 1.3 were called Tenhou Desktop, so their folder is moved over once.
        static string DataDirectory()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(local, "Rinshan");
            var old = Path.Combine(local, "TenhouDesktop");
            if (!Directory.Exists(dir) && Directory.Exists(old))
            {
                try { Directory.Move(old, dir); }
                catch { return old; }   // still in use (e.g. an old version is running): keep using it
            }
            Directory.CreateDirectory(dir);
            return dir;
        }

        static bool IsOnScreen(Rectangle r)
        {
            foreach (var screen in Screen.AllScreens)
                if (screen.WorkingArea.IntersectsWith(r)) return true;
            return false;
        }

        async System.Threading.Tasks.Task InitWebView()
        {
            try
            {
                // Own profile folder so login and Tenhou settings persist independently of Edge/Chrome
                var options = new CoreWebView2EnvironmentOptions(BrowserArgs);
                env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(dataDir, "WebView2"), options);
                await web.EnsureCoreWebView2Async(env);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Lang.T(
                    "No se pudo iniciar WebView2.\n\nInstala el \"Microsoft Edge WebView2 Runtime\" e inténtalo de nuevo.\n\n",
                    "WebView2 could not be started.\n\nInstall the \"Microsoft Edge WebView2 Runtime\" and try again.\n\n",
                    "Não foi possível iniciar o WebView2.\n\nInstale o \"Microsoft Edge WebView2 Runtime\" e tente de novo.\n\n") + ex.Message,
                    "Rinshan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                confirmedExit = true;
                Close();
                return;
            }

            var core = web.CoreWebView2;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsZoomControlEnabled = true;
            core.IsMuted = settings.Muted;

            // Tenhou replays and the log editor open in an app window; anything else in the default browser
            core.NewWindowRequested += (s, e) =>
            {
                e.Handled = true;
                if (IsTenhouViewer(e.Uri)) OpenReplay(e.Uri);
                else BrowserWindow.OpenExternal(e.Uri);
            };
            core.DocumentTitleChanged += (s, e) =>
            {
                var t = core.DocumentTitle;
                Text = string.IsNullOrEmpty(t) ? "Rinshan" : t;
            };
            // Respect the page's own fullscreen requests
            core.ContainsFullScreenElementChanged += (s, e) => SetFullscreen(core.ContainsFullScreenElement);
            // Tenhou plays a sound when a match starts; flash the taskbar if we're in the background
            core.IsDocumentPlayingAudioChanged += (s, e) => { if (core.IsDocumentPlayingAudio) NotifyActivity(); };

            // While the page has focus, WebView2 swallows most keys before WinForms sees them,
            // so the page itself reports our shortcuts back to the app.
            await core.AddScriptToExecuteOnDocumentCreatedAsync(ShortcutScript);
            await core.AddScriptToExecuteOnDocumentCreatedAsync(TenhouPresence.PageScript);
            await UpdateTranslateConfig();
            await core.AddScriptToExecuteOnDocumentCreatedAsync(Program.LoadResource("Rinshan.translate.js"));
            core.WebMessageReceived += (s, e) =>
            {
                Uri source;
                if (!Uri.TryCreate(e.Source, UriKind.Absolute, out source) || source.Host != "tenhou.net") return;
                var message = e.TryGetWebMessageAsString();
                if (message.StartsWith("presence:")) OnPresenceEvent(message.Substring(9));
                else RunShortcut(message);
            };
            ApplyDiscordSettings();

            // Started on NavigationStarting: when Tenhou's scripts hang, the page never finishes loading
            core.NavigationStarting += (s, e) =>
            {
                WatchTenhouLoad();
                if (tenhouReloadNeeded) { tenhouReloadNeeded = false; UpdatePanel(); }
            };

            web.ZoomFactor = settings.Zoom;
            web.ZoomFactorChanged += (s, e) => { settings.Zoom = web.ZoomFactor; UpdatePanel(); };

            core.Navigate(settings.Url);
        }

        // Tenhou draws everything on canvases. If none exist a while after loading, its scripts never
        // arrived (maintenance or a network problem), so show a message instead of a black window.
        const string LoadFailedOverlay = @"
(function(){
  if (document.querySelector('canvas') || document.getElementById('tdk-offline')) return 'ok';
  var d = document.createElement('div');
  d.id = 'tdk-offline';
  d.style.cssText = 'position:fixed;inset:0;z-index:99999;display:flex;align-items:center;justify-content:center;' +
    'background:radial-gradient(800px 400px at 50% 0%,#2b1f44,#141019);color:#f3eef9;font:15px Segoe UI,sans-serif;';
  d.innerHTML = '<div style=""text-align:center;max-width:460px;padding:28px 32px;border-radius:16px;background:#211a2b;' +
    'border:1px solid rgba(230,195,106,.3)""><div style=""font-size:40px;color:#e6c36a"">天</div>' +
    '<h2 style=""margin:8px 0 6px"">%TITLE%</h2>' +
    '<p style=""color:#a99cbd;margin:0 0 18px"">%TEXT%</p>' +
    '<button onclick=""location.reload()"" style=""padding:10px 22px;border:0;border-radius:10px;font:600 14px Segoe UI;' +
    'cursor:pointer;color:#2a1d06;background:linear-gradient(#e6c36a,#c9973b)"">%RETRY%</button></div>';
  document.body.appendChild(d);
  return 'shown';
})()";

        readonly Timer loadWatchdog = new Timer { Interval = 20000 };

        void WatchTenhouLoad()
        {
            loadWatchdog.Stop();
            loadWatchdog.Tick -= OnLoadWatchdog;
            loadWatchdog.Tick += OnLoadWatchdog;
            loadWatchdog.Start();
        }

        void OnLoadWatchdog(object sender, EventArgs e)
        {
            loadWatchdog.Stop();
            if (web.CoreWebView2 != null && web.CoreWebView2.Source.StartsWith("https://tenhou.net/"))
                web.CoreWebView2.ExecuteScriptAsync(LoadFailedOverlay
                    .Replace("%TITLE%", Lang.T("No se puede conectar con Tenhou", "Cannot connect to Tenhou", "Não foi possível conectar ao Tenhou"))
                    .Replace("%TEXT%", Lang.T(
                        "Puede estar en mantenimiento o haber un problema de conexión con su servidor. Inténtalo de nuevo en unos minutos.",
                        "It may be under maintenance, or there is a problem reaching its server. Try again in a few minutes.",
                        "Pode estar em manutenção ou com problemas de conexão com o servidor. Tente de novo em alguns minutos."))
                    .Replace("%RETRY%", Lang.T("Reintentar", "Retry", "Tentar de novo")));
        }

        static bool IsTenhouViewer(string url)
        {
            Uri u;
            return Uri.TryCreate(url, UriKind.Absolute, out u) && u.Host == "tenhou.net" &&
                (u.AbsolutePath.StartsWith("/0/") || u.AbsolutePath.StartsWith("/6/"));
        }

        // ---- Window, tray and keyboard ----

        const string ShortcutScript = @"
window.addEventListener('keydown', function(e){
  var k = null;
  if (e.key == 'F2' && !e.ctrlKey && !e.altKey) k = 'menu';
  else if (e.key == 'F11') k = 'fullscreen';
  else if (e.key == 'Escape') k = 'escape';
  else if (e.ctrlKey && !e.altKey && (e.key == 'm' || e.key == 'M')) k = 'mute';
  else if (e.ctrlKey && e.key == '0') k = 'zoomreset';
  if (!k || !window.chrome || !chrome.webview) return;
  chrome.webview.postMessage(k);
  if (k != 'escape') { e.preventDefault(); e.stopPropagation(); }
}, true);";

        // Used when the keys arrive through WinForms (focus outside the page)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            string shortcut = null;
            switch (keyData)
            {
                case Keys.F2: shortcut = "menu"; break;
                case Keys.F11: shortcut = "fullscreen"; break;
                case Keys.Escape: if (fullscreen) shortcut = "escape"; break;
                case Keys.F5: shortcut = "reload"; break;
                case Keys.Control | Keys.M: shortcut = "mute"; break;
                case Keys.Control | Keys.D0:
                case Keys.Control | Keys.NumPad0: shortcut = "zoomreset"; break;
            }
            if (shortcut == null) return base.ProcessCmdKey(ref msg, keyData);
            RunShortcut(shortcut);
            return true;
        }

        void RunShortcut(string shortcut)
        {
            switch (shortcut)
            {
                case "menu": TogglePanel(); break;
                case "fullscreen": SetFullscreen(!fullscreen); break;
                case "escape": if (fullscreen) SetFullscreen(false); break;
                case "reload": if (web.CoreWebView2 != null) web.CoreWebView2.Reload(); break;
                case "mute": ToggleMute(); break;
                case "zoomreset": web.ZoomFactor = 1.0; break;
            }
        }

        void OnWindowStateChanged()
        {
            if (WindowState == FormWindowState.Minimized && settings.MinimizeToTray && Visible)
            {
                Hide();
                if (!trayHintShown)
                {
                    trayHintShown = true;
                    ShowBalloon(Lang.T("Tenhou sigue abierto", "Tenhou is still running", "O Tenhou continua aberto"),
                        Lang.T("Haz clic en el ícono de la bandeja para volver.", "Click the tray icon to bring it back.",
                            "Clique no ícone da bandeja para voltar."));
                }
            }
            else if (WindowState != FormWindowState.Minimized)
            {
                stateBeforeTray = WindowState;
            }
            UpdateKeepAwake();
        }

        public void ShowFromTray()
        {
            if (!Visible) Show();
            if (WindowState == FormWindowState.Minimized) WindowState = stateBeforeTray;
            Activate();
            BringToFront();
        }

        void NotifyActivity()
        {
            if (ActiveForm == this) return;
            if (Visible) Native.FlashUntilFocused(Handle);
            else ShowBalloon("Rinshan", Lang.T("Hay sonido en Tenhou: ¿empezó la partida?",
                "Tenhou is playing a sound: has your game started?", "O Tenhou está tocando um som: sua partida começou?"));
        }

        void ShowBalloon(string title, string text, bool isUpdate = false)
        {
            balloonIsUpdate = isUpdate;
            tray.ShowBalloonTip(5000, title, text, ToolTipIcon.Info);
        }

        void UpdateKeepAwake()
        {
            Native.KeepDisplayOn(settings.KeepAwake && Visible && WindowState != FormWindowState.Minimized);
        }

        void ToggleMute()
        {
            settings.Muted = !settings.Muted;
            if (web.CoreWebView2 != null) web.CoreWebView2.IsMuted = settings.Muted;
            UpdatePanel();
        }

        void SetFullscreen(bool on)
        {
            if (on == fullscreen) return;
            fullscreen = on;
            if (on)
            {
                stateBeforeFullscreen = WindowState;
                boundsBeforeFullscreen = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Bounds = Screen.FromControl(this).Bounds;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                Bounds = boundsBeforeFullscreen;
                WindowState = stateBeforeFullscreen;
            }
            UpdatePanel();
        }

        void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && settings.ConfirmClose && !confirmedExit)
            {
                ShowFromTray();
                var answer = MessageBox.Show(this, Lang.T("¿Seguro que quieres cerrar Tenhou?\n\nSi estás en una partida, te desconectarás.",
                    "Close Tenhou?\n\nIf you're in a game, you'll be disconnected.",
                    "Fechar o Tenhou?\n\nSe estiver em uma partida, você será desconectado."),
                    "Rinshan", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes) { e.Cancel = true; return; }
            }
            confirmedExit = true;
            SaveSettings();
        }

        void SaveSettings()
        {
            if (fullscreen)
            {
                settings.Bounds = boundsBeforeFullscreen;
                settings.Maximized = stateBeforeFullscreen == FormWindowState.Maximized;
            }
            else
            {
                settings.Bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                settings.Maximized = WindowState == FormWindowState.Maximized ||
                    (WindowState == FormWindowState.Minimized && stateBeforeTray == FormWindowState.Maximized);
            }
            if (panel != null)
                settings.PanelBounds = panel.WindowState == FormWindowState.Normal ? panel.Bounds : panel.RestoreBounds;
            try { settings.Save(); } catch { }
        }

        // ---- Language ----

        void ApplyLanguageTexts()
        {
            trayShow.Text = Lang.T("Mostrar Tenhou", "Show Tenhou", "Mostrar Tenhou");
            trayOpenMenu.Text = Lang.T("Abrir menú", "Open menu", "Abrir menu");
            trayExit.Text = Lang.T("Salir", "Exit", "Sair");
            if (panel != null) panel.Text = Lang.T("Rinshan — Menú", "Rinshan — Menu", "Rinshan — Menu");
        }

        // Tells translate.js whether to run. Scripts added on document creation only run on the next load.
        async System.Threading.Tasks.Task UpdateTranslateConfig()
        {
            var core = web.CoreWebView2;
            if (core == null) return;
            if (translateConfigScriptId != null) core.RemoveScriptToExecuteOnDocumentCreated(translateConfigScriptId);
            translateConfigScriptId = await core.AddScriptToExecuteOnDocumentCreatedAsync(
                "window.__tdk = { translate: " + (settings.TranslateTenhou ? "true" : "false") + " };");
        }

        void SetLanguage(string language)
        {
            if (language != "auto" && !Lang.IsSupported(language)) return;
            settings.Language = language;
            Lang.Set(language);
            ApplyLanguageTexts();
            // Tenhou keeps its own language in localStorage "lang"; without it, it follows the system language
            if (web.CoreWebView2 != null && web.CoreWebView2.Source.StartsWith("https://tenhou.net/"))
            {
                web.CoreWebView2.ExecuteScriptAsync(language == "auto"
                    ? "localStorage.removeItem('lang')"
                    : "localStorage.setItem('lang', '" + Lang.Code + "')");
                tenhouReloadNeeded = true;
            }
            presence.Refresh();
            if (settings.DiscordEnabled) discord.Set(presence.Activity);
        }

        // ---- Updates ----

        async void CheckForUpdates(bool manual)
        {
            if (updateStatus == "checking" || updateStatus == "installing") return;
            var known = update;
            updateStatus = "checking";
            UpdatePanel();
            try
            {
                update = await Updater.CheckAsync();
                updateStatus = update != null ? "available" : "latest";
            }
            catch
            {
                // A failed background check (offline, GitHub down) just waits for the next one
                updateStatus = manual ? "failed" : update != null ? "available" : "idle";
            }
            UpdatePanel();
            bool isNew = update != null && (known == null || known.Version != update.Version);
            if (isNew && !manual && (panel == null || !panel.Visible))
                ShowBalloon(Lang.T("Hay una nueva versión de Rinshan", "A new version of Rinshan is available", "Há uma nova versão do Rinshan"),
                    Lang.T("Abre el menú (F2) para actualizar a la v", "Open the menu (F2) to update to v", "Abra o menu (F2) para atualizar para a v") +
                    Updater.Format(update.Version) + ".", true);
        }

        async void InstallUpdate()
        {
            if (update == null || updateStatus == "installing") return;
            var answer = MessageBox.Show(panel != null && panel.Visible ? (IWin32Window)panel : this, Lang.T(
                "Rinshan se cerrará para instalar la v" + Updater.Format(update.Version) + " y se abrirá de nuevo.\n\nSi estás en una partida, te desconectarás.",
                "Rinshan will close to install v" + Updater.Format(update.Version) + " and open again.\n\nIf you're in a game, you'll be disconnected.",
                "O Rinshan vai fechar para instalar a v" + Updater.Format(update.Version) + " e abrir de novo.\n\nSe estiver em uma partida, você será desconectado."),
                "Rinshan", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.OK) return;

            updateStatus = "installing";
            UpdatePanel();
            try
            {
                Program.RestartPath = await Updater.InstallAsync(update);
                confirmedExit = true;
                Close();
            }
            catch (Exception ex)
            {
                updateStatus = "failed";
                UpdatePanel();
                MessageBox.Show(panel != null && panel.Visible ? (IWin32Window)panel : this, Lang.T(
                    "No se pudo actualizar. Puedes descargar la nueva versión a mano desde GitHub.\n\n",
                    "The update failed. You can download the new version by hand from GitHub.\n\n",
                    "Não foi possível atualizar. Você pode baixar a nova versão manualmente no GitHub.\n\n") + ex.Message,
                    "Rinshan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---- Discord ----

        void OnPresenceEvent(string payload)
        {
            Dictionary<string, object> e;
            try { e = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, object>>(payload); }
            catch { return; }
            if (e != null && presence.Apply(e) && settings.DiscordEnabled) discord.Set(presence.Activity);
        }

        // The project's Discord application. Application ids are public, so shipping it means
        // players don't need to set anything up; settings.ini can still override it.
        const string BuiltInDiscordAppId = "1564349110952398849";

        string DiscordAppId
        {
            get { return settings.DiscordAppId.Length > 0 ? settings.DiscordAppId : BuiltInDiscordAppId; }
        }

        // Disconnecting from Discord also clears the presence, so "off" simply drops the connection
        void ApplyDiscordSettings()
        {
            bool on = settings.DiscordEnabled && DiscordAppId.Length > 0;
            discord.Configure(on ? DiscordAppId : null);
            if (on) discord.Set(presence.Activity);
        }

        // ---- F2 panel ----

        async System.Threading.Tasks.Task RefreshLogs()
        {
            if (web.CoreWebView2 == null) return;
            try
            {
                logs = TenhouLogs.Parse(await web.CoreWebView2.ExecuteScriptAsync(TenhouLogs.ReadScript));
            }
            catch { }
        }

        // F2: open the menu window, bring it forward if it's behind, or hide it if it's already in front
        void TogglePanel()
        {
            if (env == null) return;
            if (panel == null)
            {
                panel = new MenuPanel(env);
                panel.Icon = Icon;
                panel.Bounds = settings.PanelBounds.Width > 0 && IsOnScreen(settings.PanelBounds)
                    ? settings.PanelBounds
                    : CenteredOver(RectangleToScreen(ClientRectangle), new Size(940, 600));
                panel.TopMost = settings.PanelOnTop;
                panel.Command += OnPanelCommand;
                panel.VisibleChanged += (s, e) => { if (panel.Visible) RefreshPanel(); };
                panelTimer.Tick += (s, e) => { if (panel.Visible) RefreshPanel(); };
                panelTimer.Start();
            }
            if (panel.Visible && ActiveForm == panel)
            {
                ClosePanel();
                return;
            }
            UpdatePanel();
            panel.ShowAndFocus();
        }

        static Rectangle CenteredOver(Rectangle area, Size size)
        {
            return new Rectangle(area.X + (area.Width - size.Width) / 2, area.Y + (area.Height - size.Height) / 2, size.Width, size.Height);
        }

        // New games show up in the menu while it's open
        readonly Timer panelTimer = new Timer { Interval = 10000 };

        async void RefreshPanel()
        {
            await RefreshLogs();
            UpdatePanel();
        }

        void UpdatePanel()
        {
            if (panel != null) panel.Render(PanelState());
        }

        void ClosePanel()
        {
            panel.Hide();
            if (Visible) Activate();
        }

        Dictionary<string, object> PanelState()
        {
            var games = new List<object>();
            foreach (var log in logs)
            {
                games.Add(new Dictionary<string, object> {
                    { "id", log.Id }, { "when", log.When }, { "sanma", log.IsSanma },
                    { "players", log.Players }
                });
            }
            return new Dictionary<string, object> {
                { "muted", settings.Muted }, { "fullscreen", fullscreen }, { "zoom", web.ZoomFactor },
                { "minimizeToTray", settings.MinimizeToTray }, { "confirmClose", settings.ConfirmClose },
                { "keepAwake", settings.KeepAwake }, { "panelOnTop", settings.PanelOnTop },
                { "discord", settings.DiscordEnabled }, { "discordAppId", settings.DiscordAppId },
                { "discordBuiltIn", BuiltInDiscordAppId.Length > 0 }, { "games", games },
                { "lang", Lang.Code }, { "language", settings.Language }, { "translateTenhou", settings.TranslateTenhou },
                { "reloadNeeded", tenhouReloadNeeded },
                { "version", Updater.CurrentText }, { "checkUpdates", settings.CheckUpdates }, { "updateStatus", updateStatus },
                { "updateVersion", update != null ? Updater.Format(update.Version) : null }
            };
        }

        void OnPanelCommand(Dictionary<string, object> msg)
        {
            object value;
            string cmd = msg.TryGetValue("cmd", out value) ? value as string : null;
            GameLog game = null;
            if (msg.TryGetValue("id", out value)) game = logs.Find(l => l.Id == value as string);

            switch (cmd)
            {
                case "close": ClosePanel(); return;
                case "exit": Close(); return;
                case "fullscreen": SetFullscreen(!fullscreen); break;
                case "reload": if (web.CoreWebView2 != null) web.CoreWebView2.Reload(); return;
                case "mute": ToggleMute(); break;
                case "zoom":
                    if (msg.TryGetValue("value", out value)) web.ZoomFactor = Convert.ToDouble(value);
                    break;
                case "option":
                    msg.TryGetValue("name", out value);
                    if ((string)value == "minimizeToTray") settings.MinimizeToTray = !settings.MinimizeToTray;
                    else if ((string)value == "confirmClose") settings.ConfirmClose = !settings.ConfirmClose;
                    else if ((string)value == "keepAwake") { settings.KeepAwake = !settings.KeepAwake; UpdateKeepAwake(); }
                    else if ((string)value == "panelOnTop") { settings.PanelOnTop = !settings.PanelOnTop; panel.TopMost = settings.PanelOnTop; }
                    else if ((string)value == "discord") { settings.DiscordEnabled = !settings.DiscordEnabled; ApplyDiscordSettings(); }
                    else if ((string)value == "translateTenhou")
                    {
                        settings.TranslateTenhou = !settings.TranslateTenhou;
                        var _ = UpdateTranslateConfig();
                        tenhouReloadNeeded = true;
                    }
                    else if ((string)value == "checkUpdates")
                    {
                        settings.CheckUpdates = !settings.CheckUpdates;
                        if (settings.CheckUpdates) CheckForUpdates(false);
                    }
                    break;
                case "checkUpdates": CheckForUpdates(true); return;
                case "installUpdate": InstallUpdate(); return;
                case "releaseNotes":
                    if (update != null) BrowserWindow.OpenExternal(update.PageUrl);
                    return;
                case "language":
                    msg.TryGetValue("value", out value);
                    SetLanguage(value as string);
                    break;
                case "discordAppId":
                    msg.TryGetValue("value", out value);
                    // Discord application ids are numeric snowflakes
                    var id = new string(Convert.ToString(value).Where(char.IsDigit).ToArray());
                    settings.DiscordAppId = id;
                    ApplyDiscordSettings();
                    break;
                case "copy":
                    if (game != null) Clipboard.SetText(game.ReplayUrl);
                    return;
                case "replay":
                    if (game != null) OpenReplay(game.ReplayUrl);
                    return;
                case "review":
                    if (game == null) return;
                    msg.TryGetValue("service", out value);
                    if ((string)value == "mortal") OpenReview("Mortal", "https://mjai.ekyu.moe/", game);
                    // NAGA only analyses four-player games
                    else if ((string)value == "naga" && !game.IsSanma)
                        OpenReview("NAGA", "https://naga.dmv.nico/naga_report/order_form/", game);
                    return;
                case "clipboardReplay":
                    OpenReplayFromClipboard();
                    return;
            }
            panel.Render(PanelState());
        }

        void OpenReplay(string url)
        {
            if (env == null) return;
            new BrowserWindow(env, Lang.T("Replay de Tenhou", "Tenhou replay", "Replay do Tenhou"), url, null, Icon).Show();
        }

        void OpenReplayFromClipboard()
        {
            string url = null;
            try { url = TenhouLogs.FindReplayUrl(Clipboard.GetText()); } catch { }
            if (url == null)
            {
                MessageBox.Show(this, Lang.T("Copia primero un enlace de replay de Tenhou (tenhou.net/0/?log=...).",
                    "Copy a Tenhou replay link first (tenhou.net/0/?log=...).",
                    "Copie primeiro um link de replay do Tenhou (tenhou.net/0/?log=...)."),
                    "Rinshan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            OpenReplay(url);
        }

        void OpenReview(string service, string url, GameLog game)
        {
            if (env == null) return;
            new BrowserWindow(env, service + " — " + game.Label, url, game.ReplayUrl, Icon).Show();
        }
    }
}
