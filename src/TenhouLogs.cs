using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TenhouDesktop
{
    // A finished game as stored by the Tenhou web client in localStorage ("log0".."log39", newest at "lognext"-1)
    class GameLog
    {
        public string Id;   // e.g. 2024102215gm-00a9-0000-1234abcd
        public int Oya;     // seat of the first dealer relative to us, used for the replay's &tw=

        public string ReplayUrl
        {
            get { return "https://tenhou.net/0/?log=" + Id + "&tw=" + ((4 - Oya) % 4); }
        }

        // Tenhou's game-type bit 0x10 marks three-player games
        public bool IsSanma
        {
            get
            {
                var parts = Id.Split('-');
                int type;
                return parts.Length > 1 && int.TryParse(parts[1], NumberStyles.HexNumber, null, out type) && (type & 0x10) != 0;
            }
        }

        // The id starts with the game's hour in Japan time (yyyyMMddHH); shown in local time
        public string When
        {
            get
            {
                string when = Id.Length >= 10 ? Id.Substring(0, 10) : Id;
                DateTime jst;
                if (DateTime.TryParseExact(when, "yyyyMMddHH", CultureInfo.InvariantCulture, DateTimeStyles.None, out jst))
                    when = TimeZoneInfo.ConvertTimeFromUtc(jst.AddHours(-9), TimeZoneInfo.Local).ToString("dd/MM HH:00");
                return when;
            }
        }

        public string Label
        {
            get { return When + "  ·  " + (IsSanma ? "3 jugadores" : "4 jugadores"); }
        }
    }

    static class TenhouLogs
    {
        static readonly Regex IdPattern = new Regex(@"^[0-9a-z\-]+$");
        static readonly Regex ReplayUrlPattern = new Regex(@"https?://tenhou\.net/\d/\?log=[0-9a-z\-]+(&tw=\d)?");

        // Runs inside the tenhou.net page and returns "id,oya;id,oya;..." newest first
        public const string ReadScript =
            "(function(){try{var n=~~localStorage.getItem('lognext'),r=[];" +
            "for(var b=0;b<40;b++){var v=localStorage.getItem('log'+((n+39-b)%40));if(!v)continue;" +
            "try{var d=JSON.parse(v);if(d&&d.log)r.push(d.log+','+(~~d.oya));}catch(e){}}" +
            "return r.join(';');}catch(e){return '';}})()";

        // ExecuteScriptAsync returns the result JSON-encoded; ids only contain [0-9a-z-] so stripping quotes is enough
        public static List<GameLog> Parse(string scriptResult)
        {
            var list = new List<GameLog>();
            if (string.IsNullOrEmpty(scriptResult)) return list;
            foreach (var entry in scriptResult.Trim('"').Split(';'))
            {
                var parts = entry.Split(',');
                int oya;
                if (parts.Length == 2 && IdPattern.IsMatch(parts[0]) && int.TryParse(parts[1], out oya))
                    list.Add(new GameLog { Id = parts[0], Oya = oya });
            }
            return list;
        }

        public static string FindReplayUrl(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var m = ReplayUrlPattern.Match(text);
            return m.Success ? m.Value : null;
        }
    }
}
