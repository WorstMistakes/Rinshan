<p align="center">
  <img src="assets/discord-logo.png" width="96" alt="Tenhou Desktop">
</p>

<h1 align="center">Tenhou Desktop</h1>

<p align="center">
  Juega <a href="https://tenhou.net/3/">Tenhou</a> en su propia ventana de escritorio, sin abrir el navegador.<br>
  <i>Play <a href="https://tenhou.net/3/">Tenhou</a> in its own desktop window, no browser needed.</i>
</p>

<p align="center">
  <b><a href="#español">Español</a></b> · <b><a href="#english">English</a></b>
</p>

> [!IMPORTANT]
> **Proyecto no oficial.** No está afiliado a Tenhou / C-EGG, NAGA ni Mortal. La app solo muestra la web oficial de Tenhou en una ventana: **no automatiza jugadas ni da ningún tipo de ayuda durante la partida.**
>
> **Unofficial project.** Not affiliated with Tenhou / C-EGG, NAGA or Mortal. The app only shows the official Tenhou website in a window: **it does not automate play or give any kind of in-game assistance.**

![Menú F2](docs/menu-games.png)

---

## Español

### ¿Qué hace?

- **Ventana propia**: Tenhou sin pestañas ni barra de direcciones, con su ícono en la barra de tareas. Recuerda tamaño, posición y zoom.
- **Menú con F2**: una ventana aparte, que puedes mover incluso a otro monitor, con:
  - **Tus últimas partidas** (las 40 que guarda Tenhou): ver el replay, copiar el enlace o mandarla a analizar.
  - **Analizar con Mortal o NAGA**: abre la página con el enlace de la partida **ya pegado y resaltado**, pero **nunca lo envía solo**. Tú revisas que sea la partida correcta y pulsas el botón, así no gastas créditos de NAGA por un clic de más.
  - Silenciar, pantalla completa, zoom y opciones.
- **Discord Rich Presence**: tus amigos ven en qué andas, como con osu!

  | Momento | Discord muestra |
  |---|---|
  | Lobby | En el lobby |
  | Cola | Buscando partida · Houou 鳳凰 · Hanchan · 4p |
  | Partida | Jugando · Tokujou 特上 · Hanchan · 4p — Sur 2 · 1 honba · 2.º (28.300) |
  | Final | Terminó una partida — Quedó 1.º |

- **Bandeja del sistema**: al minimizar, se esconde junto al reloj y sigue corriendo a velocidad normal.
- **Avisos**: si Tenhou suena mientras estás en otra ventana (por ejemplo, al empezar una partida), parpadea la barra de tareas.
- **Sin sustos**: confirma antes de cerrar para no desconectarte de una partida, y mantiene la pantalla encendida mientras juegas.
- **Una sola instancia**: abrir el acceso directo otra vez trae la ventana que ya está abierta.
- **Portable**: no se instala; es una carpeta que puedes llevar a donde quieras.

### Descargar

1. Ve a **[Releases](../../releases)** y descarga el `.zip` de la última versión.
2. Descomprímelo en cualquier carpeta y abre `Tenhou.exe`.

Necesita **Windows 10 u 11**. Usa WebView2, el motor de Microsoft Edge que ya viene con Windows.

### Atajos

| Tecla | Acción |
|---|---|
| `F2` | Abrir / cerrar el menú |
| `F11` | Pantalla completa (`Esc` para salir) |
| `F5` | Recargar Tenhou |
| `Ctrl` + `M` | Silenciar |
| `Ctrl` + rueda / `Ctrl` + `0` | Zoom / zoom normal |

### Preguntas frecuentes

**¿Es seguro para mi cuenta?** La app carga la misma página que tu navegador (`tenhou.net/3/`). Tu sesión se guarda solo en tu PC, en `%LOCALAPPDATA%\TenhouDesktop`.

**¿Cómo sabe en qué ronda estoy, para Discord?** Escucha los mensajes que Tenhou recibe de su servidor (inicio de partida, cada ronda, final) sin modificarlos. Solo usa información que ya ves en pantalla. Se puede apagar en **F2 → Opciones → Mostrar en Discord**.

**La ventana queda en negro.** Tenhou puede estar en mantenimiento o no responder. Tras 20 segundos la app muestra un aviso con un botón para reintentar.

### Compilar desde el código

No hace falta instalar Visual Studio ni nada más: usa el compilador de .NET Framework que trae Windows.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

La primera vez descarga la librería de WebView2 desde NuGet. El resultado queda en `dist\`.

| Carpeta | Contenido |
|---|---|
| `src/` | Código C# (WinForms + WebView2) y el menú (`panel.html`) |
| `assets/` | Ícono e imagen para Discord |
| `tools/` | Scripts para regenerar el ícono y las capturas |
| `docs/` | Capturas del README |

---

## English

### What it does

- **Its own window**: Tenhou without tabs or an address bar, with its own taskbar icon. Remembers size, position and zoom.
- **F2 menu**: a separate window (you can even move it to another monitor) with:
  - **Your recent games** (the 40 Tenhou keeps): watch the replay, copy the link or send it for review.
  - **Review with Mortal or NAGA**: opens the site with the game link **already pasted and highlighted**, but **never submits it for you**. You check it's the right game and press the button, so a misclick never costs you NAGA credits.
  - Mute, fullscreen, zoom and settings.
- **Discord Rich Presence**: friends see what you're doing, like with osu!: lobby, searching for a game, room, round and placement.
- **System tray**: minimizing hides it next to the clock while it keeps running at full speed.
- **Notifications**: if Tenhou plays a sound while you're in another window (e.g. a match starts), the taskbar button flashes.
- **No accidents**: asks before closing so you don't disconnect from a game, and keeps the screen awake while you play.
- **Single instance**: opening the shortcut again brings back the existing window.
- **Portable**: nothing to install; it's just a folder.

The interface is currently in Spanish; English is on the roadmap.

### Download

1. Go to **[Releases](../../releases)** and download the latest `.zip`.
2. Extract it anywhere and run `Tenhou.exe`.

Requires **Windows 10 or 11**. It uses WebView2, the Microsoft Edge engine that ships with Windows.

### Shortcuts

| Key | Action |
|---|---|
| `F2` | Open / close the menu |
| `F11` | Fullscreen (`Esc` to leave) |
| `F5` | Reload Tenhou |
| `Ctrl` + `M` | Mute |
| `Ctrl` + wheel / `Ctrl` + `0` | Zoom / reset zoom |

### Build from source

No Visual Studio needed: it uses the .NET Framework compiler included with Windows.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

The first run downloads the WebView2 library from NuGet. Output goes to `dist\`.

---

## Licencia / License

[MIT](LICENSE). Tenhou (天鳳) es un servicio de C-EGG; este proyecto no incluye ninguno de sus recursos gráficos. *Tenhou (天鳳) is a service by C-EGG; this project does not include any of its artwork.*
