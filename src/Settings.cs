using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Rinshan
{
    // Simple key=value settings stored in %LOCALAPPDATA%\Rinshan\settings.ini
    class Settings
    {
        public string Url = "https://tenhou.net/3/";
        public Rectangle Bounds = new Rectangle(100, 100, 1280, 800);
        public bool Maximized = false;
        public double Zoom = 1.0;
        public bool Muted = false;
        public bool MinimizeToTray = true;
        public bool ConfirmClose = true;
        public bool KeepAwake = true;
        public Rectangle PanelBounds = Rectangle.Empty;
        public bool PanelOnTop = false;
        public bool DiscordEnabled = true;
        public string DiscordAppId = "";
        public string Language = "auto";    // auto, es, en or pt
        public bool TranslateTenhou = true;
        public bool CheckUpdates = true;

        readonly string path;

        public Settings(string path)
        {
            this.path = path;
        }

        public void Load()
        {
            if (!File.Exists(path)) return;
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadAllLines(path))
            {
                int eq = line.IndexOf('=');
                if (eq > 0) values[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }
            string s;
            if (values.TryGetValue("url", out s) && s.Length > 0) Url = s;
            Rectangle r;
            if (ReadRect(values, "", out r)) Bounds = new Rectangle(r.X, r.Y, Math.Max(r.Width, 400), Math.Max(r.Height, 300));
            if (ReadRect(values, "panel", out r)) PanelBounds = r;
            double z;
            if (values.TryGetValue("zoom", out s) && double.TryParse(s, NumberStyles.Float,
                CultureInfo.InvariantCulture, out z) && z > 0.2 && z < 5) Zoom = z;
            Maximized = ReadBool(values, "maximized", Maximized);
            Muted = ReadBool(values, "muted", Muted);
            MinimizeToTray = ReadBool(values, "minimizetotray", MinimizeToTray);
            ConfirmClose = ReadBool(values, "confirmclose", ConfirmClose);
            KeepAwake = ReadBool(values, "keepawake", KeepAwake);
            PanelOnTop = ReadBool(values, "panelontop", PanelOnTop);
            DiscordEnabled = ReadBool(values, "discord", DiscordEnabled);
            if (values.TryGetValue("discordappid", out s)) DiscordAppId = s;
            if (values.TryGetValue("language", out s) && (s == "auto" || Lang.IsSupported(s))) Language = s;
            // Settings from before 1.2 have no language: those versions were Spanish only, so stay in Spanish
            else if (!values.ContainsKey("language")) Language = "es";
            TranslateTenhou = ReadBool(values, "translatetenhou", TranslateTenhou);
            CheckUpdates = ReadBool(values, "checkupdates", CheckUpdates);
        }

        static bool ReadRect(Dictionary<string, string> values, string prefix, out Rectangle rect)
        {
            string s;
            int x, y, w, h;
            rect = Rectangle.Empty;
            if (!(values.TryGetValue(prefix + "x", out s) && int.TryParse(s, out x) &&
                  values.TryGetValue(prefix + "y", out s) && int.TryParse(s, out y) &&
                  values.TryGetValue(prefix + "width", out s) && int.TryParse(s, out w) &&
                  values.TryGetValue(prefix + "height", out s) && int.TryParse(s, out h))) return false;
            rect = new Rectangle(x, y, w, h);
            return true;
        }

        static bool ReadBool(Dictionary<string, string> values, string key, bool fallback)
        {
            string s;
            bool b;
            return values.TryGetValue(key, out s) && bool.TryParse(s, out b) ? b : fallback;
        }

        public void Save()
        {
            File.WriteAllLines(path, new[] {
                "url=" + Url,
                "x=" + Bounds.X, "y=" + Bounds.Y,
                "width=" + Bounds.Width, "height=" + Bounds.Height,
                "maximized=" + Maximized,
                "zoom=" + Zoom.ToString(CultureInfo.InvariantCulture),
                "muted=" + Muted,
                "minimizetotray=" + MinimizeToTray,
                "confirmclose=" + ConfirmClose,
                "keepawake=" + KeepAwake,
                "panelx=" + PanelBounds.X, "panely=" + PanelBounds.Y,
                "panelwidth=" + PanelBounds.Width, "panelheight=" + PanelBounds.Height,
                "panelontop=" + PanelOnTop,
                "discord=" + DiscordEnabled,
                "discordappid=" + DiscordAppId,
                "language=" + Language,
                "translatetenhou=" + TranslateTenhou,
                "checkupdates=" + CheckUpdates
            });
        }
    }
}
