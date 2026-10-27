// Translates the Japanese text that Tenhou's web client leaves untranslated (rooms, rules, ranks, yaku,
// draws, menus). Tenhou's own language files already cover its settings and buttons; this only fills
// the gaps. It only rewrites text on screen: it never touches the game connection or the page's logic.
// Written from scratch for Rinshan. Each entry is [Japanese, English, Spanish, Portuguese].
// Mahjong terms (yaku, calls, draws, hand values, kan and dora kinds) stay in romaji in every language,
// so all players see the same words: Tanyao, Chiitoitsu, Ryuukyoku, Minkan... Only the rest is translated.
(function () {
  if (!/(^|\.)(tenhou\.net|ron2\.jp)$/.test(location.hostname)) return;
  if ((window.__tdk || {}).translate === false) return;

  // Same language Tenhou uses for its own strings: its "lang" setting, else the system language
  var code = 'ja';
  try { code = localStorage.getItem('lang') || navigator.language || 'ja'; } catch (e) {}
  code = String(code).split('-')[0].toLowerCase();
  if (code == 'ja' || code == 'jp') return;
  var col = code == 'es' ? 2 : code == 'pt' ? 3 : 1;

  var WORDS = [
    // Rooms and lobbies
    ['般', 'Ippan', 'Ippan', 'Ippan'], ['上', 'Joukyuu', 'Joukyuu', 'Joukyuu'], ['特', 'Tokujou', 'Tokujou', 'Tokujou'],
    ['鳳', 'Houou', 'Houou', 'Houou'], ['若', 'Wakaba', 'Wakaba', 'Wakaba'], ['銀', 'Gin', 'Gin', 'Gin'],
    ['琥', 'Kohaku', 'Kohaku', 'Kohaku'], ['孔', 'Kujaku', 'Kujaku', 'Kujaku'],
    ['一般', 'Ippan', 'Ippan', 'Ippan'], ['上級', 'Joukyuu', 'Joukyuu', 'Joukyuu'], ['特上', 'Tokujou', 'Tokujou', 'Tokujou'],
    ['鳳凰', 'Houou', 'Houou', 'Houou'], ['上級卓', 'Joukyuu', 'Joukyuu', 'Joukyuu'], ['特上卓', 'Tokujou', 'Tokujou', 'Tokujou'],
    ['鳳凰卓', 'Houou', 'Houou', 'Houou'],
    ['雀荘戦', 'Parlor match', 'Partida de salón', 'Partida de salão'],
    ['三麻上級卓', '3P Joukyuu', '3P Joukyuu', '3P Joukyuu'], ['三麻特上卓', '3P Tokujou', '3P Tokujou', '3P Tokujou'],
    ['三麻鳳凰卓', '3P Houou', '3P Houou', '3P Houou'], ['三麻雀荘戦', '3P parlor match', '3P partida de salón', '3P partida de salão'],
    ['四麻上級卓', '4P Joukyuu', '4P Joukyuu', '4P Joukyuu'], ['四麻特上卓', '4P Tokujou', '4P Tokujou', '4P Tokujou'],
    ['四麻鳳凰卓', '4P Houou', '4P Houou', '4P Houou'], ['四麻雀荘戦', '4P parlor match', '4P partida de salón', '4P partida de salão'],
    ['三麻', '3P', '3P', '3P'], ['四麻', '4P', '4P', '4P'], ['三麻R', '3P R', '3P R', '3P R'], ['四麻R', '4P R', '4P R', '4P R'],
    ['３人', '3P', '3P', '3P'], ['４人', '4P', '4P', '4P'], ['サンマ', 'Sanma', 'Sanma', 'Sanma'],
    ['３人打', '3-player', '3 jugadores', '3 jogadores'], ['４人打', '4-player', '4 jugadores', '4 jogadores'],
    ['公式卓', 'Official table', 'Mesa oficial', 'Mesa oficial'], ['公式ルール', 'official rules', 'reglas oficiales', 'regras oficiais'],
    ['個室', 'Lobby ', 'Sala ', 'Sala '], ['大会', 'Tournament ', 'Torneo ', 'Torneio '],
    ['イベント卓', 'Event table', 'Mesa de evento', 'Mesa de evento'], ['イベント', 'Event ', 'Evento ', 'Evento '],
    ['対戦', 'Play', 'Jugar', 'Jogar'], ['對局', 'Game', 'Partida', 'Partida'],
    ['対戦中', 'In a game', 'En partida', 'Em partida'], ['待機中', 'Waiting', 'Esperando', 'Aguardando'],
    ['準備中', 'Getting ready', 'Preparando', 'Preparando'], ['觀戰', 'Watch', 'Observar', 'Assistir'],
    ['再開', 'Resume', 'Reanudar', 'Retomar'], ['だれでもOK', 'Anyone', 'Cualquiera', 'Qualquer um'],

    // Rules
    ['東風戦', 'Tonpuu', 'Tonpuu', 'Tonpuu'], ['東南戦', 'Hanchan', 'Hanchan', 'Hanchan'],
    ['東風', 'Tonpuu', 'Tonpuu', 'Tonpuu'], ['東南', 'Hanchan', 'Hanchan', 'Hanchan'],
    ['東天紅', 'Tontenkou', 'Tontenkou', 'Tontenkou'], ['東西場', 'West round', 'Ronda Oeste', 'Rodada Oeste'],
    ['喰断アリ', 'kuitan on', 'con kuitan', 'com kuitan'], ['喰断ナシ', 'no kuitan', 'sin kuitan', 'sem kuitan'],
    ['喰断', 'Kuitan', 'Kuitan', 'Kuitan'], ['喰あり', 'Kuitan', 'Kuitan', 'Kuitan'],
    ['喰ナシ', 'No kuitan', 'Sin kuitan', 'Sem kuitan'], ['喰', 'Kuitan', 'Kuitan', 'Kuitan'],
    ['赤アリ', 'aka dora', 'con aka dora', 'com aka dora'], ['赤あり', 'Aka dora', 'Con aka dora', 'Com aka dora'],
    ['赤ナシ', 'No aka dora', 'Sin aka dora', 'Sem aka dora'], ['赤', 'Aka', 'Aka', 'Aka'],
    ['速', 'fast', 'rápido', 'rápido'], ['祝儀', 'Chips', 'Fichas', 'Fichas'], ['祝', 'Chips', 'Fichas', 'Fichas'],
    ['祝０', 'Chips 0', 'Fichas 0', 'Fichas 0'], ['祝２', 'Chips 2', 'Fichas 2', 'Fichas 2'], ['祝５', 'Chips 5', 'Fichas 5', 'Fichas 5'],
    ['サドンデス', ' sudden death', ' muerte súbita', ' morte súbita'],
    ['和了止め', 'Agari-yame', 'Agari-yame', 'Agari-yame'], ['聴牌止め', 'Tenpai-yame', 'Tenpai-yame', 'Tenpai-yame'],
    ['打ち切り', 'Bust ends game', 'Termina al quedar en negativo', 'Termina ao ficar negativo'],
    ['拡張', 'Extended', 'Extendido', 'Estendido'],
    ['段位変動', 'Rank change', 'Cambio de rango', 'Mudança de rank'],
    ['段位R指定なし', 'Any rank / R', 'Cualquier rango / R', 'Qualquer rank / R'],
    ['段位', 'Rank', 'Rango', 'Rank'], ['以上', 'or higher', 'o más', 'ou mais'], ['以下', 'or lower', 'o menos', 'ou menos'],
    ['限定', 'only', 'solo', 'apenas'], ['固定', 'Fixed', 'Fijo', 'Fixo'], ['ウマ', 'Uma', 'Uma', 'Uma'],
    ['なし', 'None', 'No', 'Não'], ['ナシ', 'None', 'No', 'Não'],

    // Table, scoring and seats
    ['東', 'E', 'E', 'E'], ['南', 'S', 'S', 'S'], ['西', 'W', 'O', 'O'], ['北', 'N', 'N', 'N'],
    ['局', 'Round', 'Ronda', 'Rodada'], ['本場', 'honba', 'honba', 'honba'],
    ['供託', 'Riichi sticks', 'Palitos de riichi', 'Palitos de riichi'],
    ['点数', 'Score', 'Puntos', 'Pontos'], ['点', 'pts', 'pts', 'pts'], ['符', 'fu', 'fu', 'fu'], ['飜', 'han', 'han', 'han'],
    ['枚', 'chips', 'fichas', 'fichas'],
    ['親', 'Dealer', 'Mano', 'Dealer'], ['自家', 'You', 'Tú', 'Você'], ['下家', 'Right', 'Derecha', 'Direita'],
    ['対面', 'Across', 'Enfrente', 'Em frente'], ['上家', 'Left', 'Izquierda', 'Esquerda'], ['私', 'Me', 'Yo', 'Eu'],
    ['手牌', 'Hand', 'Mano', 'Mão'], ['副露', 'Calls', 'Llamadas', 'Chamadas'], ['河', 'Discards', 'Descartes', 'Descartes'],
    ['ドラ', 'Dora', 'Dora', 'Dora'], ['裏ドラ', 'Ura dora', 'Ura dora', 'Ura dora'], ['赤ドラ', 'Aka dora', 'Aka dora', 'Aka dora'],
    ['明槓ドラ', 'Kan dora', 'Kan dora', 'Kan dora'],
    ['明槓ドラ後', 'Kan dora after discard', 'Kan dora tras descartar', 'Kan dora após o descarte'],
    ['後', ' after discard', ' tras descartar', ' após o descarte'],
    ['明槓', 'Minkan', 'Minkan', 'Minkan'], ['暗槓', 'Ankan', 'Ankan', 'Ankan'], ['暗', 'Closed', 'Cerrado', 'Fechado'],
    ['門前', 'Menzen', 'Menzen', 'Menzen'], ['榮和', 'Ron', 'Ron', 'Ron'],
    ['ツモ:', 'Tsumo:', 'Tsumo:', 'Tsumo:'], ['ロン:', 'Ron:', 'Ron:', 'Ron:'], ['放銃:', 'Dealt in:', 'Dio ron:', 'Deu ron:'],
    ['ポン', 'Pon', 'Pon', 'Pon'], ['チー', 'Chii', 'Chii', 'Chii'], ['カン', 'Kan', 'Kan', 'Kan'],
    ['リーチ', 'Riichi', 'Riichi', 'Riichi'], ['ツモ', 'Tsumo', 'Tsumo', 'Tsumo'], ['ロン', 'Ron', 'Ron', 'Ron'],
    ['流局', 'Ryuukyoku', 'Ryuukyoku', 'Ryuukyoku'], ['流局滿貫', 'Nagashi mangan', 'Nagashi mangan', 'Nagashi mangan'],
    ['三家和了', 'Sanchahou', 'Sanchahou', 'Sanchahou'], ['九種九牌', 'Kyuushu kyuuhai', 'Kyuushu kyuuhai', 'Kyuushu kyuuhai'],
    ['四家立直', 'Suucha riichi', 'Suucha riichi', 'Suucha riichi'], ['四槓散了', 'Suukaikan', 'Suukaikan', 'Suukaikan'],
    ['四風連打', 'Suufon renda', 'Suufon renda', 'Suufon renda'],
    ['滿貫', 'Mangan', 'Mangan', 'Mangan'], ['跳滿', 'Haneman', 'Haneman', 'Haneman'], ['倍滿', 'Baiman', 'Baiman', 'Baiman'],
    ['三倍滿', 'Sanbaiman', 'Sanbaiman', 'Sanbaiman'], ['役滿', 'Yakuman', 'Yakuman', 'Yakuman'],
    ['終局', 'Game over', 'Fin de la partida', 'Fim da partida'], ['認定', 'Promotion', 'Ascenso', 'Promoção'],
    ['■使用枚数', '■ Chips used', '■ Fichas usadas', '■ Fichas usadas'],
    ['■合計得点', '■ Total score', '■ Puntuación total', '■ Pontuação total'],
    ['1枚2000点', '1 chip = 2000 pts', '1 ficha = 2000 pts', '1 ficha = 2000 pts'],
    ['1枚5000点', '1 chip = 5000 pts', '1 ficha = 5000 pts', '1 ficha = 5000 pts'],

    // Yaku: romaji in every language
    ['門前清自摸和', 'Menzen tsumo'], ['立直', 'Riichi'], ['一發', 'Ippatsu'], ['槍槓', 'Chankan'],
    ['嶺上開花', 'Rinshan kaihou'], ['海底摸月', 'Haitei'], ['河底撈魚', 'Houtei'], ['平和', 'Pinfu'],
    ['斷么九', 'Tanyao'], ['一盃口', 'Iipeikou'],
    ['自風 東', 'Jikaze ton'], ['自風 南', 'Jikaze nan'], ['自風 西', 'Jikaze shaa'], ['自風 北', 'Jikaze pei'],
    ['場風 東', 'Bakaze ton'], ['場風 南', 'Bakaze nan'], ['場風 西', 'Bakaze shaa'], ['場風 北', 'Bakaze pei'],
    ['役牌 白', 'Haku'], ['役牌 發', 'Hatsu'], ['役牌 中', 'Chun'],
    ['兩立直', 'Daburu riichi'], ['七對子', 'Chiitoitsu'], ['混全帶么九', 'Chanta'], ['一氣通貫', 'Ittsu'],
    ['三色同順', 'Sanshoku'], ['三色同刻', 'Sanshoku doukou'], ['三槓子', 'Sankantsu'], ['對對和', 'Toitoi'],
    ['三暗刻', 'Sanankou'], ['小三元', 'Shousangen'], ['混老頭', 'Honroutou'], ['二盃口', 'Ryanpeikou'],
    ['純全帶么九', 'Junchan'], ['混一色', 'Honitsu'], ['清一色', 'Chinitsu'],
    ['人和', 'Renhou'], ['天和', 'Tenhou'], ['地和', 'Chiihou'], ['大三元', 'Daisangen'],
    ['四暗刻', 'Suuankou'], ['四暗刻單騎', 'Suuankou tanki'], ['字一色', 'Tsuuiisou'], ['綠一色', 'Ryuuiisou'],
    ['清老頭', 'Chinroutou'], ['九蓮寶燈', 'Chuuren poutou'], ['純正九蓮寶燈', 'Junsei chuuren poutou'],
    ['國士無雙', 'Kokushi musou'], ['國士無雙13面', 'Kokushi musou juusanmen'],
    ['大四喜', 'Daisuushii'], ['小四喜', 'Shousuushii'], ['四槓子', 'Suukantsu'],

    // Ranks
    ['新人', 'Novice', 'Novato', 'Novato'], ['初段', '1 dan'], ['二段', '2 dan'], ['三段', '3 dan'], ['四段', '4 dan'],
    ['五段', '5 dan'], ['六段', '6 dan'], ['七段', '7 dan'], ['八段', '8 dan'], ['九段', '9 dan'], ['十段', '10 dan'],
    ['天鳳', 'Tenhou'], ['レベル:', 'Level:', 'Nivel:', 'Nível:'],

    // Menus, account and messages
    ['ランキング', 'Ranking', 'Ranking', 'Ranking'], ['会員ページ', 'Member page', 'Página de socio', 'Página de membro'],
    ['詳細', 'Details', 'Detalles', 'Detalhes'], ['プロ情報', 'Pro info', 'Info de pros', 'Info de pros'],
    ['スキンβ', 'Skin (beta)', 'Skin (beta)', 'Skin (beta)'], ['卓表示', 'Table view', 'Ver mesa', 'Ver mesa'],
    ['高宮プロ(βテスト版)', 'Takamiya pro (beta)', 'Takamiya pro (beta)', 'Takamiya pro (beta)'],
    ['同意する', 'I agree', 'Acepto', 'Concordo'], ['アプリで開く', 'Open in the app', 'Abrir en la app', 'Abrir no app'],
    ['Desktop4K版で開く', 'Open Desktop 4K version', 'Abrir versión Desktop 4K', 'Abrir versão Desktop 4K'],
    ['パスワード', 'Password', 'Contraseña', 'Senha'], ['メールアドレス', 'Email', 'Correo', 'E-mail'],
    ['パスワードをもう一度確認してください', 'Please check your password again', 'Revisa tu contraseña', 'Confira sua senha'],
    ['メールアドレスを確認してください。+記号や大文字は使用できません。',
      'Please check your email address. "+" and capital letters are not allowed.',
      'Revisa tu correo. No se permiten "+" ni mayúsculas.',
      'Confira seu e-mail. Não são permitidos "+" nem letras maiúsculas.'],
    ['ログイン画面の「新規ID」からIDを取得してください', 'Get an ID with "New ID" on the login screen',
      'Consigue un ID con "Nuevo ID" en la pantalla de inicio', 'Crie um ID em "Novo ID" na tela de login'],
    ['このブラウザ環境(プライベートブラウズなど)では、牌譜の記録やIDの保存などが行えません。',
      'This browser (e.g. private mode) can\'t save your game logs or ID.',
      'Este navegador (p. ej. modo privado) no puede guardar tus partidas ni tu ID.',
      'Este navegador (ex.: modo privado) não consegue salvar suas partidas nem seu ID.'],
    ['上にゆっくりスクロールしてください', 'Scroll up slowly', 'Desliza hacia arriba despacio', 'Role para cima devagar'],
    ['●「長押し」または「右クリック」から別タブで開く/URLのコピーなどが行えます',
      '● Long-press or right-click to open in a new tab or copy the URL',
      '● Mantén pulsado o haz clic derecho para abrir en otra pestaña o copiar la URL',
      '● Pressione e segure ou clique com o botão direito para abrir em outra aba ou copiar a URL'],
    ['※予告なく仕様が変更される場合があります', '* May change without notice', '* Puede cambiar sin aviso', '* Pode mudar sem aviso'],
    ['本機能はクローズド公開中のため使用できません', 'This feature is in closed beta', 'Esta función está en beta cerrada',
      'Este recurso está em beta fechado'],
    ['毎月２～３回更新', 'Updated 2–3 times a month', 'Se actualiza 2–3 veces al mes', 'Atualizado 2–3 vezes por mês'],
    ['【会員無料】', '[Free for members]', '[Gratis para socios]', '[Grátis para membros]'], ['【無料】', '[Free]', '[Gratis]', '[Grátis]'],
    ['【購読中】', '[Subscribed]', '[Suscrito]', '[Assinado]'], ['無料', 'Free', 'Gratis', 'Grátis'],
    ['購読終了:', 'Subscription ends:', 'Suscripción termina:', 'Assinatura termina:'],
    ['購読期間と料金を選択してください。', 'Choose a subscription period and price.', 'Elige el período y el precio.',
      'Escolha o período e o preço.'],
    ['有効期限', 'Expiry', 'Vencimiento', 'Validade'],
    ['編集機能を利用するには有効期限の残り日数が90日以上必要です。', 'Editing needs 90+ days left on your membership.',
      'Para editar necesitas 90+ días de membresía.', 'Para editar, você precisa de 90+ dias de assinatura.'],

    // Training
    ['天鳳トレーニング', 'Tenhou Training', 'Entrenamiento Tenhou', 'Treino Tenhou'],
    ['トレーニング終了', 'End training', 'Terminar entrenamiento', 'Encerrar treino'],
    ['他の問題に移動', 'Go to another problem', 'Ir a otro problema', 'Ir para outro problema'],
    ['問題数:', 'Problems:', 'Problemas:', 'Problemas:'], ['問題', 'Problem', 'Problema', 'Problema'],
    ['表題', 'Title', 'Título', 'Título'], ['回答', 'Answer', 'Respuesta', 'Resposta'],
    ['解説', 'Explanation', 'Explicación', 'Explicação'], ['補足', 'Notes', 'Notas', 'Notas'],
    ['左下のメニューからトレーニングを終了して', 'End the training from the bottom-left menu',
      'Termina el entrenamiento desde el menú de abajo a la izquierda', 'Encerre o treino pelo menu no canto inferior esquerdo'],
    ['もう一度開いてください。', 'and open it again.', 'y vuelve a abrirlo.', 'e abra de novo.'],
    ['この問題は有料会員に公開されています。', 'This problem is for paid members.', 'Este problema es para socios de pago.',
      'Este problema é para membros pagos.'],
    ['会費のお支払いをお願いいたします。', 'Please pay the membership fee.', 'Por favor, paga la cuota.', 'Por favor, pague a mensalidade.'],
    ['問題ファイルを読み込めませんでした', 'Couldn\'t load the problem file', 'No se pudo cargar el archivo de problemas',
      'Não foi possível carregar o arquivo de problemas'],
    ['JSONに不具合があるため読み込めませんでした', 'Couldn\'t load: the JSON is invalid', 'No se pudo cargar: el JSON no es válido',
      'Não foi possível carregar: o JSON é inválido'],
    ['カンマ区切 例)', 'Comma-separated, e.g. ', 'Separado por comas, ej. ', 'Separado por vírgulas, ex.: ']
  ];

  var KANJI_NUM = { '一': 1, '二': 2, '三': 3, '四': 4, '五': 5, '六': 6, '七': 7, '八': 8, '九': 9, '十': 10 };
  var WIND = { '東': ['East', 'Este', 'Leste'], '南': ['South', 'Sur', 'Sul'], '西': ['West', 'Oeste', 'Oeste'], '北': ['North', 'Norte', 'Norte'] };
  function num(s) { return String(s).replace(/[０-９]/g, function (d) { return d.charCodeAt(0) - 0xFF10; }); }
  function pick(en, es, pt) { return col == 2 ? es : col == 3 ? pt : en; }
  function ordinal(n) {
    if (col == 2) return n + '.º';
    if (col == 3) return n + 'º';
    return n + (n % 10 == 1 && n % 100 != 11 ? 'st' : n % 10 == 2 && n % 100 != 12 ? 'nd' : n % 10 == 3 && n % 100 != 13 ? 'rd' : 'th');
  }

  // Patterns with numbers, applied before the word list
  var D = '([0-9０-９]+)';
  var ROOM = { '般': 'Ippan', '上': 'Joukyuu', '特': 'Tokujou', '鳳': 'Houou', '若': 'Wakaba', '銀': 'Gin', '琥': 'Kohaku', '孔': 'Kujaku' };

  // Lobby room codes, e.g. 四般東喰赤速: players, room, East/South, then kuitan, red fives, fast, chips.
  // In ranked rooms kuitan and red fives are the norm, so only their absence is spelled out.
  // The room buttons only fit about a dozen letters, so with extras East/South shrink to E/S
  function roomName(m, players, room, length, kui, aka, speed, chips) {
    var extras = [];
    if (!chips || chips.length == 1) {
      if (!kui) extras.push(pick('no-kui', 'sin kui', 'sem kui'));
      if (!aka) extras.push(pick('no-aka', 'sin aka', 'sem aka'));
    }
    if (speed == '速') extras.push(pick('Fast', 'Rápido', 'Rápido'));
    if (speed == '光') extras.push('Hikari');
    if (chips) extras.push(pick('Chips', 'Fichas', 'Fichas') + (chips.length > 1 ? ' ' + num(chips.charAt(1)) : ''));
    var south = length == '南';
    var parts = players == '三' ? ['3P'] : [];
    parts.push(ROOM[room], extras.length ? (south ? 'S' : 'E') : south ? pick('South', 'Sur', 'Sul') : pick('East', 'Este', 'Leste'));
    return parts.concat(extras).join(' ');
  }

  var RULES = [
    [/([三四])([般上特鳳若銀琥孔])([東南])(喰)?(赤)?(速|光)?(祝[０２５]?)?/g, roomName],
    [new RegExp('([東南西北])([1-4１-４])局', 'g'), function (m, w, n) { return WIND[w][col - 1] + ' ' + num(n); }],
    [new RegExp(D + '本場', 'g'), function (m, n) { return num(n) + ' honba'; }],
    [new RegExp(D + '巡目', 'g'), function (m, n) { return pick('Turn ', 'Turno ', 'Turno ') + num(n); }],
    [new RegExp(D + '局', 'g'), function (m, n) {
      n = num(n);
      return n + (n == 1 ? pick(' round', ' ronda', ' rodada') : pick(' rounds', ' rondas', ' rodadas'));
    }],
    [new RegExp(D + ' ?名', 'g'), function (m, n) { return num(n) + pick(' players', ' jugadores', ' jogadores'); }],
    [new RegExp('([1-4１-４])位', 'g'), function (m, n) { return ordinal(+num(n)); }],
    [new RegExp('有効期限' + D + '日以上', 'g'), function (m, n) { return num(n) + pick('+ days left', '+ días restantes', '+ dias restantes'); }],
    [new RegExp('有効期限' + D + '日分', 'g'), function (m, n) {
      return pick('valid ', 'válido ', 'válido por ') + num(n) + pick(' days', ' días', ' dias');
    }],
    [new RegExp(D + '日(間|分)', 'g'), function (m, n) { return num(n) + pick(' days', ' días', ' dias'); }],
    [new RegExp(D + ' ?級', 'g'), function (m, n) { return num(n) + ' kyu'; }],
    [new RegExp('([一二三四五六七八九十]) ?段', 'g'), function (m, k) { return KANJI_NUM[k] + ' dan'; }],
    [new RegExp('([+\\-]?[0-9０-９]+)枚', 'g'), function (m, n) { return num(n) + pick(' chips', ' fichas', ' fichas'); }],
    [new RegExp('([+\\-]?[0-9０-９.]+)点', 'g'), function (m, n) { return num(n) + ' pts'; }],
    [new RegExp(D + '符', 'g'), function (m, n) { return num(n) + ' fu'; }],
    [new RegExp(D + '飜', 'g'), function (m, n) { return num(n) + ' han'; }]
  ];

  // Entries with a single translation are romaji, the same in every language
  var dict = {}, phrases = [];
  WORDS.forEach(function (w) {
    dict[w[0]] = w.length > 2 ? w[col] : w[1];
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
    var v = node.nodeValue, t = translate(v);
    if (t !== v) node.nodeValue = t;
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
