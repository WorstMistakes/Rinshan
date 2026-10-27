// Translates the Japanese text that Tenhou's web client leaves untranslated (rooms, rules, ranks, yaku,
// draws, menus). Tenhou's own language files already cover its settings and buttons; this only fills
// the gaps. It only rewrites text on screen: it never touches the game connection or the page's logic.
// Written from scratch for Tenhou Desktop. Each entry is [Japanese, English, Spanish].
(function () {
  if (!/(^|\.)(tenhou\.net|ron2\.jp)$/.test(location.hostname)) return;
  if ((window.__tdk || {}).translate === false) return;

  // Same language Tenhou uses for its own strings: its "lang" setting, else the system language
  var code = 'ja';
  try { code = localStorage.getItem('lang') || navigator.language || 'ja'; } catch (e) {}
  code = String(code).split('-')[0].toLowerCase();
  if (code == 'ja' || code == 'jp') return;
  var col = code == 'es' ? 2 : 1;

  var WORDS = [
    // Rooms and lobbies
    ['般', 'Ippan', 'Ippan'], ['上', 'Joukyuu', 'Joukyuu'], ['特', 'Tokujou', 'Tokujou'], ['鳳', 'Houou', 'Houou'],
    ['若', 'Wakaba', 'Wakaba'], ['銀', 'Gin', 'Gin'], ['琥', 'Kohaku', 'Kohaku'], ['孔', 'Kujaku', 'Kujaku'],
    ['一般', 'Ippan', 'Ippan'], ['上級', 'Joukyuu', 'Joukyuu'], ['特上', 'Tokujou', 'Tokujou'], ['鳳凰', 'Houou', 'Houou'],
    ['上級卓', 'Joukyuu', 'Joukyuu'], ['特上卓', 'Tokujou', 'Tokujou'], ['鳳凰卓', 'Houou', 'Houou'],
    ['雀荘戦', 'Parlor match', 'Partida de salón'],
    ['三麻上級卓', '3P Joukyuu', '3P Joukyuu'], ['三麻特上卓', '3P Tokujou', '3P Tokujou'],
    ['三麻鳳凰卓', '3P Houou', '3P Houou'], ['三麻雀荘戦', '3P parlor match', '3P partida de salón'],
    ['四麻上級卓', '4P Joukyuu', '4P Joukyuu'], ['四麻特上卓', '4P Tokujou', '4P Tokujou'],
    ['四麻鳳凰卓', '4P Houou', '4P Houou'], ['四麻雀荘戦', '4P parlor match', '4P partida de salón'],
    ['三麻', '3P', '3P'], ['四麻', '4P', '4P'], ['三麻R', '3P R', '3P R'], ['四麻R', '4P R', '4P R'],
    ['３人', '3P', '3P'], ['４人', '4P', '4P'], ['サンマ', 'Sanma', 'Sanma'],
    ['３人打', '3-player', '3 jugadores'], ['４人打', '4-player', '4 jugadores'],
    ['公式卓', 'Official table', 'Mesa oficial'], ['公式ルール', 'official rules', 'reglas oficiales'],
    ['個室', 'Lobby ', 'Sala '], ['大会', 'Tournament ', 'Torneo '], ['イベント卓', 'Event table', 'Mesa de evento'],
    ['イベント', 'Event ', 'Evento '], ['対戦', 'Play', 'Jugar'], ['對局', 'Game', 'Partida'],
    ['対戦中', 'In a game', 'En partida'], ['待機中', 'Waiting', 'Esperando'], ['準備中', 'Getting ready', 'Preparando'],
    ['觀戰', 'Watch', 'Observar'], ['再開', 'Resume', 'Reanudar'], ['だれでもOK', 'Anyone', 'Cualquiera'],

    // Rules
    ['東風戦', 'Tonpuu', 'Tonpuu'], ['東南戦', 'Hanchan', 'Hanchan'], ['東風', 'Tonpuu', 'Tonpuu'], ['東南', 'Hanchan', 'Hanchan'],
    ['東天紅', 'Tontenkou', 'Tontenkou'], ['東西場', 'West round', 'Ronda Oeste'],
    ['喰断アリ', 'open tanyao', 'tanyao abierto'], ['喰断ナシ', 'no open tanyao', 'sin tanyao abierto'],
    ['喰断', 'Kuitan', 'Kuitan'], ['喰あり', 'Kuitan', 'Kuitan'], ['喰ナシ', 'No kuitan', 'Sin kuitan'], ['喰', 'Kuitan', 'Kuitan'],
    ['赤アリ', 'red fives', 'con rojos'], ['赤あり', 'Red fives', 'Con rojos'], ['赤ナシ', 'No red fives', 'Sin rojos'],
    ['赤', 'Red', 'Rojos'], ['速', 'fast', 'rápido'], ['祝儀', 'Chips', 'Fichas'], ['祝', 'Chips', 'Fichas'],
    ['祝０', 'Chips 0', 'Fichas 0'], ['祝２', 'Chips 2', 'Fichas 2'], ['祝５', 'Chips 5', 'Fichas 5'],
    ['サドンデス', ' sudden death', ' muerte súbita'], ['和了止め', 'Agari-yame', 'Agari-yame'], ['聴牌止め', 'Tenpai-yame', 'Tenpai-yame'],
    ['打ち切り', 'Bust ends game', 'Termina al quedar en negativo'], ['拡張', 'Extended', 'Extendido'],
    ['段位変動', 'Rank change', 'Cambio de rango'], ['段位R指定なし', 'Any rank / R', 'Cualquier rango / R'],
    ['段位', 'Rank', 'Rango'], ['以上', 'or higher', 'o más'], ['以下', 'or lower', 'o menos'], ['限定', 'only', 'solo'],
    ['固定', 'Fixed', 'Fijo'], ['ウマ', 'Uma', 'Uma'], ['なし', 'None', 'No'], ['ナシ', 'None', 'No'],

    // Table, scoring and seats
    ['東', 'E', 'E'], ['南', 'S', 'S'], ['西', 'W', 'O'], ['北', 'N', 'N'],
    ['局', 'Round', 'Ronda'], ['本場', 'honba', 'honba'], ['供託', 'Riichi sticks', 'Palitos de riichi'],
    ['点数', 'Score', 'Puntos'], ['点', 'pts', 'pts'], ['符', 'fu', 'fu'], ['飜', 'han', 'han'], ['枚', 'chips', 'fichas'],
    ['親', 'Dealer', 'Mano'], ['自家', 'You', 'Tú'], ['下家', 'Right', 'Derecha'], ['対面', 'Across', 'Enfrente'],
    ['上家', 'Left', 'Izquierda'], ['私', 'Me', 'Yo'], ['手牌', 'Hand', 'Mano'], ['副露', 'Calls', 'Llamadas'],
    ['河', 'Discards', 'Descartes'], ['ドラ', 'Dora', 'Dora'], ['裏ドラ', 'Ura dora', 'Ura dora'], ['赤ドラ', 'Red dora', 'Dora roja'],
    ['明槓ドラ', 'Kan dora', 'Dora de kan'], ['明槓', 'Open kan', 'Kan abierto'], ['暗槓', 'Closed kan', 'Kan cerrado'],
    ['暗', 'Closed', 'Cerrado'], ['門前', 'Closed hand', 'Mano cerrada'], ['榮和', 'Ron', 'Ron'],
    ['ツモ:', 'Tsumo:', 'Tsumo:'], ['ロン:', 'Ron:', 'Ron:'], ['放銃:', 'Dealt in:', 'Dio ron:'],
    ['ポン', 'Pon', 'Pon'], ['チー', 'Chii', 'Chii'], ['カン', 'Kan', 'Kan'], ['リーチ', 'Riichi', 'Riichi'],
    ['ツモ', 'Tsumo', 'Tsumo'], ['ロン', 'Ron', 'Ron'],
    ['流局', 'Draw', 'Empate'], ['流局滿貫', 'Nagashi mangan', 'Nagashi mangan'], ['三家和了', 'Triple ron', 'Triple ron'],
    ['九種九牌', 'Nine terminals', 'Nueve terminales'], ['四家立直', 'Four riichi', 'Cuatro riichis'],
    ['四槓散了', 'Four kans', 'Cuatro kans'], ['四風連打', 'Four winds', 'Cuatro vientos'],
    ['滿貫', 'Mangan', 'Mangan'], ['跳滿', 'Haneman', 'Haneman'], ['倍滿', 'Baiman', 'Baiman'],
    ['三倍滿', 'Sanbaiman', 'Sanbaiman'], ['役滿', 'Yakuman', 'Yakuman'],
    ['終局', 'Game over', 'Fin de la partida'], ['認定', 'Promotion', 'Ascenso'],
    ['■使用枚数', '■ Chips used', '■ Fichas usadas'], ['■合計得点', '■ Total score', '■ Puntuación total'],
    ['1枚2000点', '1 chip = 2000 pts', '1 ficha = 2000 pts'], ['1枚5000点', '1 chip = 5000 pts', '1 ficha = 5000 pts'],

    // Yaku
    ['門前清自摸和', 'Menzen tsumo', 'Menzen tsumo'], ['立直', 'Riichi', 'Riichi'], ['一發', 'Ippatsu', 'Ippatsu'],
    ['槍槓', 'Chankan', 'Chankan'], ['嶺上開花', 'Rinshan kaihou', 'Rinshan kaihou'], ['海底摸月', 'Haitei', 'Haitei'],
    ['河底撈魚', 'Houtei', 'Houtei'], ['平和', 'Pinfu', 'Pinfu'], ['斷么九', 'Tanyao', 'Tanyao'], ['一盃口', 'Iipeikou', 'Iipeikou'],
    ['自風 東', 'Seat wind East', 'Viento propio Este'], ['自風 南', 'Seat wind South', 'Viento propio Sur'],
    ['自風 西', 'Seat wind West', 'Viento propio Oeste'], ['自風 北', 'Seat wind North', 'Viento propio Norte'],
    ['場風 東', 'Round wind East', 'Viento de ronda Este'], ['場風 南', 'Round wind South', 'Viento de ronda Sur'],
    ['場風 西', 'Round wind West', 'Viento de ronda Oeste'], ['場風 北', 'Round wind North', 'Viento de ronda Norte'],
    ['役牌 白', 'Haku', 'Haku'], ['役牌 發', 'Hatsu', 'Hatsu'], ['役牌 中', 'Chun', 'Chun'],
    ['兩立直', 'Double riichi', 'Doble riichi'], ['七對子', 'Chiitoitsu', 'Chiitoitsu'], ['混全帶么九', 'Chanta', 'Chanta'],
    ['一氣通貫', 'Ittsu', 'Ittsu'], ['三色同順', 'Sanshoku', 'Sanshoku'], ['三色同刻', 'Sanshoku doukou', 'Sanshoku doukou'],
    ['三槓子', 'Sankantsu', 'Sankantsu'], ['對對和', 'Toitoi', 'Toitoi'], ['三暗刻', 'Sanankou', 'Sanankou'],
    ['小三元', 'Shousangen', 'Shousangen'], ['混老頭', 'Honroutou', 'Honroutou'], ['二盃口', 'Ryanpeikou', 'Ryanpeikou'],
    ['純全帶么九', 'Junchan', 'Junchan'], ['混一色', 'Honitsu', 'Honitsu'], ['清一色', 'Chinitsu', 'Chinitsu'],
    ['人和', 'Renhou', 'Renhou'], ['天和', 'Tenhou', 'Tenhou'], ['地和', 'Chiihou', 'Chiihou'], ['大三元', 'Daisangen', 'Daisangen'],
    ['四暗刻', 'Suuankou', 'Suuankou'], ['四暗刻單騎', 'Suuankou tanki', 'Suuankou tanki'], ['字一色', 'Tsuuiisou', 'Tsuuiisou'],
    ['綠一色', 'Ryuuiisou', 'Ryuuiisou'], ['清老頭', 'Chinroutou', 'Chinroutou'], ['九蓮寶燈', 'Chuuren poutou', 'Chuuren poutou'],
    ['純正九蓮寶燈', 'Junsei chuuren poutou', 'Junsei chuuren poutou'], ['國士無雙', 'Kokushi musou', 'Kokushi musou'],
    ['國士無雙13面', 'Kokushi musou 13-wait', 'Kokushi musou 13 esperas'], ['大四喜', 'Daisuushii', 'Daisuushii'],
    ['小四喜', 'Shousuushii', 'Shousuushii'], ['四槓子', 'Suukantsu', 'Suukantsu'],

    // Ranks
    ['新人', 'Novice', 'Novato'], ['初段', '1 dan', '1 dan'], ['二段', '2 dan', '2 dan'], ['三段', '3 dan', '3 dan'],
    ['四段', '4 dan', '4 dan'], ['五段', '5 dan', '5 dan'], ['六段', '6 dan', '6 dan'], ['七段', '7 dan', '7 dan'],
    ['八段', '8 dan', '8 dan'], ['九段', '9 dan', '9 dan'], ['十段', '10 dan', '10 dan'], ['天鳳', 'Tenhou', 'Tenhou'],
    ['レベル:', 'Level:', 'Nivel:'],

    // Menus, account and messages
    ['ランキング', 'Ranking', 'Ranking'], ['会員ページ', 'Member page', 'Página de socio'], ['詳細', 'Details', 'Detalles'],
    ['プロ情報', 'Pro info', 'Info de pros'], ['スキンβ', 'Skin (beta)', 'Skin (beta)'], ['卓表示', 'Table view', 'Ver mesa'],
    ['高宮プロ(βテスト版)', 'Takamiya pro (beta)', 'Takamiya pro (beta)'], ['同意する', 'I agree', 'Acepto'],
    ['アプリで開く', 'Open in the app', 'Abrir en la app'], ['Desktop4K版で開く', 'Open Desktop 4K version', 'Abrir versión Desktop 4K'],
    ['パスワード', 'Password', 'Contraseña'], ['メールアドレス', 'Email', 'Correo'],
    ['パスワードをもう一度確認してください', 'Please check your password again', 'Revisa tu contraseña'],
    ['メールアドレスを確認してください。+記号や大文字は使用できません。',
      'Please check your email address. "+" and capital letters are not allowed.',
      'Revisa tu correo. No se permiten "+" ni mayúsculas.'],
    ['ログイン画面の「新規ID」からIDを取得してください', 'Get an ID with "New ID" on the login screen',
      'Consigue un ID con "Nuevo ID" en la pantalla de inicio'],
    ['このブラウザ環境(プライベートブラウズなど)では、牌譜の記録やIDの保存などが行えません。',
      'This browser (e.g. private mode) can\'t save your game logs or ID.',
      'Este navegador (p. ej. modo privado) no puede guardar tus partidas ni tu ID.'],
    ['上にゆっくりスクロールしてください', 'Scroll up slowly', 'Desliza hacia arriba despacio'],
    ['●「長押し」または「右クリック」から別タブで開く/URLのコピーなどが行えます',
      '● Long-press or right-click to open in a new tab or copy the URL',
      '● Mantén pulsado o haz clic derecho para abrir en otra pestaña o copiar la URL'],
    ['※予告なく仕様が変更される場合があります', '* May change without notice', '* Puede cambiar sin aviso'],
    ['本機能はクローズド公開中のため使用できません', 'This feature is in closed beta', 'Esta función está en beta cerrada'],
    ['毎月２～３回更新', 'Updated 2–3 times a month', 'Se actualiza 2–3 veces al mes'],
    ['【会員無料】', '[Free for members]', '[Gratis para socios]'], ['【無料】', '[Free]', '[Gratis]'],
    ['【購読中】', '[Subscribed]', '[Suscrito]'], ['無料', 'Free', 'Gratis'], ['購読終了:', 'Subscription ends:', 'Suscripción termina:'],
    ['購読期間と料金を選択してください。', 'Choose a subscription period and price.', 'Elige el período y el precio.'],
    ['有効期限', 'Expiry', 'Vencimiento'],
    ['編集機能を利用するには有効期限の残り日数が90日以上必要です。', 'Editing needs 90+ days left on your membership.',
      'Para editar necesitas 90+ días de membresía.'],

    // Training
    ['天鳳トレーニング', 'Tenhou Training', 'Entrenamiento Tenhou'], ['トレーニング終了', 'End training', 'Terminar entrenamiento'],
    ['他の問題に移動', 'Go to another problem', 'Ir a otro problema'], ['問題数:', 'Problems:', 'Problemas:'],
    ['問題', 'Problem', 'Problema'], ['表題', 'Title', 'Título'], ['回答', 'Answer', 'Respuesta'], ['解説', 'Explanation', 'Explicación'],
    ['補足', 'Notes', 'Notas'], ['左下のメニューからトレーニングを終了して', 'End the training from the bottom-left menu',
      'Termina el entrenamiento desde el menú de abajo a la izquierda'], ['もう一度開いてください。', 'and open it again.', 'y vuelve a abrirlo.'],
    ['この問題は有料会員に公開されています。', 'This problem is for paid members.', 'Este problema es para socios de pago.'],
    ['会費のお支払いをお願いいたします。', 'Please pay the membership fee.', 'Por favor, paga la cuota.'],
    ['問題ファイルを読み込めませんでした', 'Couldn\'t load the problem file', 'No se pudo cargar el archivo de problemas'],
    ['JSONに不具合があるため読み込めませんでした', 'Couldn\'t load: the JSON is invalid', 'No se pudo cargar: el JSON no es válido'],
    ['カンマ区切 例)', 'Comma-separated, e.g. ', 'Separado por comas, ej. ']
  ];

  var KANJI_NUM = { '一': 1, '二': 2, '三': 3, '四': 4, '五': 5, '六': 6, '七': 7, '八': 8, '九': 9, '十': 10 };
  var WIND = { '東': ['East', 'Este'], '南': ['South', 'Sur'], '西': ['West', 'Oeste'], '北': ['North', 'Norte'] };
  function num(s) { return String(s).replace(/[０-９]/g, function (d) { return d.charCodeAt(0) - 0xFF10; }); }
  function pick(en, es) { return col == 2 ? es : en; }
  function ordinal(n) {
    if (col == 2) return n + '.º';
    return n + (n % 10 == 1 && n % 100 != 11 ? 'st' : n % 10 == 2 && n % 100 != 12 ? 'nd' : n % 10 == 3 && n % 100 != 13 ? 'rd' : 'th');
  }

  // Patterns with numbers, applied before the word list
  var D = '([0-9０-９]+)';
  var ROOM = { '般': 'Ippan', '上': 'Joukyuu', '特': 'Tokujou', '鳳': 'Houou', '若': 'Wakaba', '銀': 'Gin', '琥': 'Kohaku', '孔': 'Kujaku' };

  // Lobby room codes, e.g. 四般東喰赤速: players, room, East/South, then kuitan, red fives, fast, chips.
  // In ranked rooms kuitan and red fives are the norm, so only their absence is spelled out.
  var roomTranslated = false;
  function roomName(m, players, room, length, kui, aka, speed, chips) {
    roomTranslated = true;
    var parts = [];
    if (players == '三') parts.push('3P');
    parts.push(ROOM[room], length == '南' ? pick('South', 'Sur') : pick('East', 'Este'));
    if (!chips || chips.length == 1) {
      if (!kui) parts.push(pick('no-kui', 'sin kui'));
      if (!aka) parts.push(pick('no-red', 'sin rojos'));
    }
    if (speed == '速') parts.push(pick('Fast', 'Rápido'));
    if (speed == '光') parts.push('Hikari');
    if (chips) parts.push(pick('Chips', 'Fichas') + (chips.length > 1 ? ' ' + num(chips.charAt(1)) : ''));
    return parts.join(' ');
  }

  var RULES = [
    [/([三四])([般上特鳳若銀琥孔])([東南])(喰)?(赤)?(速|光)?(祝[０２５]?)?/g, roomName],
    [new RegExp('([東南西北])([1-4１-４])局', 'g'), function (m, w, n) { return WIND[w][col - 1] + ' ' + num(n); }],
    [new RegExp(D + '本場', 'g'), function (m, n) { return num(n) + ' honba'; }],
    [new RegExp(D + '巡目', 'g'), function (m, n) { return pick('Turn ', 'Turno ') + num(n); }],
    [new RegExp(D + '局', 'g'), function (m, n) { n = num(n); return n + (n == 1 ? pick(' round', ' ronda') : pick(' rounds', ' rondas')); }],
    [new RegExp(D + ' ?名', 'g'), function (m, n) { return num(n) + pick(' players', ' jugadores'); }],
    [new RegExp('([1-4１-４])位', 'g'), function (m, n) { return ordinal(+num(n)); }],
    [new RegExp('有効期限' + D + '日以上', 'g'), function (m, n) { return num(n) + pick('+ days left', '+ días restantes'); }],
    [new RegExp('有効期限' + D + '日分', 'g'), function (m, n) { return pick('valid ', 'válido ') + num(n) + pick(' days', ' días'); }],
    [new RegExp(D + '日(間|分)', 'g'), function (m, n) { return num(n) + pick(' days', ' días'); }],
    [new RegExp(D + ' ?級', 'g'), function (m, n) { return num(n) + ' kyu'; }],
    [new RegExp('([一二三四五六七八九十]) ?段', 'g'), function (m, k) { return KANJI_NUM[k] + ' dan'; }],
    [new RegExp('([+\\-]?[0-9０-９]+)枚', 'g'), function (m, n) { return num(n) + pick(' chips', ' fichas'); }],
    [new RegExp('([+\\-]?[0-9０-９.]+)点', 'g'), function (m, n) { return num(n) + ' pts'; }],
    [new RegExp(D + '符', 'g'), function (m, n) { return num(n) + ' fu'; }],
    [new RegExp(D + '飜', 'g'), function (m, n) { return num(n) + ' han'; }]
  ];

  var dict = {}, phrases = [];
  WORDS.forEach(function (w) {
    dict[w[0]] = w[col];
    if (w[0].length > 1) phrases.push(w[0]);
  });
  phrases.sort(function (a, b) { return b.length - a.length; });
  var PHRASE = new RegExp(phrases.map(function (p) { return p.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); }).join('|'), 'g');
  var JAPANESE = /[぀-ヿ㐀-鿿０-９]/;
  var RUN = /[぀-ヿ㐀-鿿]+/g;

  // Whole text first; then numbered patterns, known phrases, and finally single words left on their own
  function translate(s) {
    if (!s || !JAPANESE.test(s)) return s;
    var t = s.trim();
    if (dict.hasOwnProperty(t)) return s.replace(t, dict[t]);
    var out = s;
    RULES.forEach(function (r) { out = out.replace(r[0], r[1]); });
    out = out.replace(PHRASE, function (m) { return dict[m]; });
    out = out.replace(RUN, function (m) { return dict.hasOwnProperty(m) ? dict[m] : m; });
    return num(out);
  }
  window.__tdkTranslate = translate;

  var SKIP = { SCRIPT: 1, STYLE: 1, TEXTAREA: 1, INPUT: 1 };
  function fixText(node) {
    var p = node.parentNode;
    if (p && SKIP[p.nodeName]) return;
    roomTranslated = false;
    var v = node.nodeValue, t = translate(v);
    if (t !== v) node.nodeValue = t;
    if (roomTranslated && p) fit(p);
  }

  // Room names are longer in English than in kanji: keep them on one line, shrinking the text a little
  // if the room button is too narrow
  function fit(el) {
    requestAnimationFrame(function () {
      var box = el;
      while (box && box.nodeType == 1 && !box.clientWidth) box = box.parentElement;
      if (!box || box.nodeType != 1) return;
      box.style.whiteSpace = 'nowrap';
      var size = parseFloat(getComputedStyle(box).fontSize), min = size * 0.65;
      while (box.scrollWidth > box.clientWidth && size > min) {
        size -= 1;
        box.style.fontSize = size + 'px';
      }
    });
  }
  function fixAttrs(el) {
    ['placeholder', 'title'].forEach(function (a) {
      var v = el.getAttribute(a);
      if (v) { var t = translate(v); if (t !== v) el.setAttribute(a, t); }
    });
    if (el.nodeName == 'INPUT' && (el.type == 'button' || el.type == 'submit')) {
      var t = translate(el.value);
      if (t !== el.value) el.value = t;
    }
  }
  function walk(root) {
    if (root.nodeType == 3) { fixText(root); return; }
    if (root.nodeType != 1 && root.nodeType != 9) return;
    if (root.nodeType == 1) fixAttrs(root);
    var w = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT), n;
    while ((n = w.nextNode())) { if (n.nodeType == 3) fixText(n); else fixAttrs(n); }
  }

  new MutationObserver(function (list) {
    list.forEach(function (m) {
      if (m.type == 'characterData') fixText(m.target);
      else if (m.type == 'attributes') fixAttrs(m.target);
      else m.addedNodes.forEach(walk);
    });
  }).observe(document, { childList: true, subtree: true, characterData: true, attributes: true, attributeFilter: ['placeholder', 'title', 'value'] });
  document.addEventListener('DOMContentLoaded', function () { walk(document); });

  // The table is painted on canvases, piece by piece ("東" "1" "局", "5" "級"), with no room for longer
  // words. Only whole known pieces are swapped there, with short forms: the round reads "E1".
  var CANVAS = { '局': '', '本場': '', '級': 'kyu', '段': 'dan', '初': '1', '一': '1', '二': '2', '三': '3', '四': '4',
    '五': '5', '六': '6', '七': '7', '八': '8', '九': '9', '十': '10' };
  var RANK = new RegExp('^(' + D + ' ?級|[初一二三四五六七八九十] ?段)$');
  function canvasText(t) {
    if (CANVAS.hasOwnProperty(t)) return CANVAS[t];
    if (dict.hasOwnProperty(t)) return dict[t];
    if (RANK.test(t)) return translate(t.replace(/^初/, '一'));
    return null;
  }
  var C2D = window.CanvasRenderingContext2D && CanvasRenderingContext2D.prototype;
  if (C2D) ['fillText', 'strokeText', 'measureText'].forEach(function (name) {
    var original = C2D[name];
    C2D[name] = function (text) {
      if (typeof text == 'string' && JAPANESE.test(text)) {
        var t = canvasText(text.trim());
        if (t !== null) { var args = [].slice.call(arguments); args[0] = t; return original.apply(this, args); }
      }
      return original.apply(this, arguments);
    };
  });
})();
