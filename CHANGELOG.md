# Changelog

What changed in each version of Rinshan. Downloads are on the [Releases](https://github.com/WorstMistakes/Rinshan/releases) page.

## Unreleased

- **App log**: Rinshan keeps a small `log.txt` (start, errors, updates, Tenhou not loading) to attach to bug reports. It never records your Tenhou ID. Open it from **F2 → Options → Help**.
- **Report a problem** button in **F2 → Options → Help**, with GitHub forms for bugs and translations.
- Unexpected errors no longer close the app silently: they are logged and Rinshan keeps running when it can.

## v1.4.0 — 2026-10-27

- **Updates itself**: when a new version is out, the F2 menu offers to install it in a few seconds and restarts the app. It never updates without asking, and the check can be turned off in **F2 → Options → Updates**.
- **Português**: the app, the menu, Discord and Tenhou's leftover Japanese are translated into Portuguese too.
- **Romaji everywhere**: yaku and mahjong terms stay in romaji in every language (Tanyao, Chiitoitsu, Ryuukyoku, Minkan…), so everyone sees the same words.

## v1.3.0 — 2026-10-27

- **Tenhou Desktop is now Rinshan**, to avoid confusion with Tenhou's own paid Windows and Desktop 4K clients. Same app, same features.
- New name and icon (嶺, from rinshan kaihou); the app is now `Rinshan.exe`.
- Settings and the Tenhou login move over automatically on the first start.

## v1.2.0 — 2026-10-27

- **English and Spanish**: F2 → Options → Language (Automatic follows Windows). Tenhou's own interface switches to match.
- **Fills in Tenhou's missing translations**: lobby rooms, rules, ranks, yaku, draws, results and menus. Only text on screen changes, never the game, and it can be turned off.
- Discord status follows the chosen language.

## v1.1.0 — 2026-10-27

First public release, as Tenhou Desktop.

- Tenhou in its own desktop window, no browser needed.
- F2 menu: recent games, replays and review with Mortal / NAGA (the link is pasted, never submitted for you).
- Discord Rich Presence: lobby, queue, room, round and placement.
- System tray, taskbar flash, confirm before closing. Portable (Windows 10/11).
