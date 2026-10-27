using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Rinshan
{
    // Turns what Tenhou's connection reports (lobby, matchmaking, rounds, results) into a Discord activity.
    // Only uses information the player already sees on screen.
    class TenhouPresence
    {
        // Runs in the Tenhou page before its own scripts. Watches the game server connection without
        // changing it and reports a few events to the app.
        public const string PageScript = @"
(function(){
  var NativeWS = window.WebSocket;
  if (!NativeWS) return;
  function post(o){ try { chrome.webview.postMessage('presence:' + JSON.stringify(o)); } catch (e) {} }
  function handle(m){
    switch (m && m.tag) {
      case 'HELO': post({s:'lobby'}); break;
      case 'GO': post({s:'game', type:m.type, lobby:m.lobby}); break;
      case 'INIT': post({s:'round', seed:m.seed, ten:m.ten}); break;
      case 'AGARI': case 'RYUUKYOKU': if (m.owari) post({s:'end', owari:m.owari}); break;
    }
  }
  function Hooked(url, protocols){
    var ws = protocols === undefined ? new NativeWS(url) : new NativeWS(url, protocols);
    if (/mjv\.jp|ron2\.jp/.test(String(url))) {
      ws.addEventListener('message', function(ev){ try { handle(JSON.parse(ev.data)); } catch (e) {} });
      ws.addEventListener('close', function(){ post({s:'offline'}); });
      var send = ws.send;
      ws.send = function(data){
        try { var m = JSON.parse(data); if (m.tag == 'JOIN') post(m.t ? {s:'search', t:m.t} : {s:'lobby'}); } catch (e) {}
        return send.apply(this, arguments);
      };
    }
    return ws;
  }
  Hooked.prototype = NativeWS.prototype;
  Hooked.CONNECTING = 0; Hooked.OPEN = 1; Hooked.CLOSING = 2; Hooked.CLOSED = 3;
  window.WebSocket = Hooked;
})();";

        const string LargeImage = "tenhou";
        static readonly string[] WindsEs = { "Este", "Sur", "Oeste", "Norte" };
        static readonly string[] WindsEn = { "East", "South", "West", "North" };

        string details, state;
        long? startedAt;
        int gameType;
        bool inGame;

        // Events since the last lobby/search/game start, replayed to redo the texts in another language
        readonly List<Dictionary<string, object>> events = new List<Dictionary<string, object>>();

        public Dictionary<string, object> Activity { get; private set; }

        // Applies one event from the page; returns true when the activity changed
        public bool Apply(Dictionary<string, object> e)
        {
            if (!ApplyEvent(e)) return false;
            var kind = Get(e, "s");
            if (kind != "round" && kind != "end") events.Clear();
            events.Add(e);
            return true;
        }

        // Rebuilds the activity in the current language, keeping its timer
        public void Refresh()
        {
            if (events.Count == 0) return;
            var start = startedAt;
            foreach (var e in events.ToList()) ApplyEvent(e);
            if (Activity != null) Show(details, state, start);
        }

        bool ApplyEvent(Dictionary<string, object> e)
        {
            var kind = Get(e, "s");
            switch (kind)
            {
                case "lobby":
                    inGame = false;
                    Show(Lang.T("En el lobby", "In the lobby"), null, null);
                    break;
                case "search":
                    var t = (Get(e, "t") ?? "").Split(',');
                    int searchType;
                    var room = t.Length > 1 && int.TryParse(t[1], out searchType) ? RoomLabel(searchType, t[0]) : null;
                    Show(Lang.T("Buscando partida", "Looking for a game"), room, Now());
                    break;
                case "game":
                    int.TryParse(Get(e, "type"), out gameType);
                    inGame = true;
                    Show(Lang.T("Jugando · ", "Playing · ") + RoomLabel(gameType, Get(e, "lobby")), Lang.T("Empezando", "Starting"), Now());
                    break;
                case "round":
                    if (!inGame) return false;
                    Show(details, RoundLabel(Get(e, "seed"), Get(e, "ten")), startedAt);
                    break;
                case "end":
                    if (!inGame) return false;
                    inGame = false;
                    Show(Lang.T("Terminó una partida · ", "Finished a game · ") + RoomLabel(gameType, null), ResultLabel(Get(e, "owari")), null);
                    break;
                case "offline":
                    inGame = false;
                    details = state = null;
                    startedAt = null;
                    Activity = null;
                    return true;
                default:
                    return false;
            }
            return true;
        }

        void Show(string newDetails, string newState, long? start)
        {
            details = newDetails;
            state = newState;
            startedAt = start;

            var activity = new Dictionary<string, object> {
                { "details", details },
                { "assets", new Dictionary<string, object> { { "large_image", LargeImage }, { "large_text", "Rinshan · for Tenhou" } } },
                { "buttons", new object[] { new Dictionary<string, object> { { "label", Lang.T("Jugar Tenhou", "Play Tenhou") }, { "url", "https://tenhou.net/3/" } } } }
            };
            if (state != null) activity["state"] = state;
            if (startedAt != null) activity["timestamps"] = new Dictionary<string, object> { { "start", startedAt.Value } };
            Activity = activity;
        }

        static long Now()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        static string Get(Dictionary<string, object> e, string key)
        {
            object v;
            return e.TryGetValue(key, out v) && v != null ? Convert.ToString(v, CultureInfo.InvariantCulture) : null;
        }

        // Tenhou game-type bits: 0x10 three players, 0x08 hanchan, 0x40 fast, room in 0x20|0x80
        public static string RoomLabel(int type, string lobby)
        {
            string room;
            if (!string.IsNullOrEmpty(lobby) && lobby != "0") room = Lang.T("Sala privada", "Private lobby");
            else
            {
                switch (type & 0xA0)
                {
                    case 0x80: room = "Joukyuu 上級"; break;
                    case 0x20: room = "Tokujou 特上"; break;
                    case 0xA0: room = "Houou 鳳凰"; break;
                    default: room = "Ippan 一般"; break;
                }
            }
            return room + " · " + ((type & 0x08) != 0 ? "Hanchan" : "Tonpuu") +
                ((type & 0x40) != 0 ? Lang.T(" rápido", " fast") : "") + " · " + ((type & 0x10) != 0 ? "3p" : "4p");
        }

        // seed starts with the round index (0 = East 1) and honba; ten holds the scores in hundreds, seat 0 is us
        string RoundLabel(string seed, string ten)
        {
            var s = (seed ?? "").Split(',');
            int round, honba;
            if (s.Length < 2 || !int.TryParse(s[0], out round) || !int.TryParse(s[1], out honba)) return null;
            var winds = Lang.English ? WindsEn : WindsEs;
            var label = winds[(round / 4) % 4] + " " + (round % 4 + 1) + (honba > 0 ? " · " + honba + " honba" : "");

            var scores = ParseInts(ten);
            if (scores.Count > 0)
            {
                var active = (gameType & 0x10) != 0 ? scores.Take(3).ToList() : scores;
                int place = 1 + active.Skip(1).Count(x => x > active[0]);
                var points = (active[0] * 100).ToString("N0", new CultureInfo(Lang.English ? "en-US" : "es-ES"));
                label += " · " + Place(place) + " (" + points + ")";
            }
            return label;
        }

        // 1.º / 1st
        static string Place(int n)
        {
            if (!Lang.English) return n + ".º";
            return n + (n == 1 ? "st" : n == 2 ? "nd" : n == 3 ? "rd" : "th");
        }

        // owari: final score, final points for each seat in order; seat 0 is us
        string ResultLabel(string owari)
        {
            var values = (owari ?? "").Split(',');
            var scores = new List<double>();
            int seats = (gameType & 0x10) != 0 ? 3 : 4;
            for (int i = 0; i + 1 < values.Length && scores.Count < seats; i += 2)
            {
                double v;
                if (double.TryParse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v)) scores.Add(v);
            }
            if (scores.Count == 0) return null;
            int place = 1 + scores.Skip(1).Count(x => x > scores[0]);
            return Lang.T("Quedó ", "Placed ") + Place(place);
        }

        static List<int> ParseInts(string csv)
        {
            var list = new List<int>();
            foreach (var part in (csv ?? "").Split(','))
            {
                int v;
                if (int.TryParse(part, out v)) list.Add(v);
            }
            return list;
        }
    }
}
