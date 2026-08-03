# BackDrop

**Cinematic ambient lock-screen for Windows.** BackDrop turns your screen into a living, breathing backdrop — full-screen video loops under a Fluent Design glass overlay with a clock, vignette, and three layout styles. It is **not** a security product: it does not replace Windows authentication, intercept credentials, or persist beyond a normal user-mode application. The overlay is a full-screen aesthetic presentation layer, always dismissible by mouse or key press unless you explicitly opt in to a local PIN gate.

> Built with WinUI 3 (Windows App SDK) on .NET 10. Native, hardware-accelerated, no web views, no Chromium.

---

## Features

| Feature | Detail |
|---|---|
| 🎬 Full-screen video loops | `MediaPlayerElement` / MediaFoundation, hardware decode, seamless looping, muted by default |
| 🪟 Multi-monitor | One borderless overlay per display; secondary screens mirror the primary loop |
| 🕐 Clock widget | Segoe UI Variable, drop shadow, updates every second |
| 🌫️ Adaptive vignette | Radial gradient layer that keeps text legible over any footage |
| 🎨 3 layouts | Minimalist Center · Classic Bottom-Left · Bold Cinematic (swappable in Settings) |
| 🎞️ Custom video import | Pick local files (mp4 / h264-compatible), validated before use |
| ⌨️ Global hotkey | Default `Ctrl+Alt+L`, configurable and persisted |
| 🗔 Tray utility | Lock Now · Settings · Exit; optional start-with-Windows |
| 🔒 Optional PIN gate | Local-only, opt-in, DPAPI-encrypted — never networked |

---

## System requirements

- **Windows 10 1809+** (build 17763) or Windows 11
- **x64** processor
- No admin rights required (unpackaged, per-user app)

---

## Installation

### From a release (recommended)

1. Download the latest `BackDrop-<version>-win-x64.zip` from the **Releases** page.
2. Extract the zip anywhere (e.g. `C:\Program Files\BackDrop\` or your user folder).
3. Run `BackDrop.App.exe`. The app parks itself in the **system tray**.

> Optional: right-click the tray icon → **Settings** → enable **Start with Windows** to launch BackDrop at sign-in.

### From source

```bash
git clone https://github.com/archanagaur175-glitch/BackDrop.git
cd BackDrop
dotnet restore BackDrop.sln --locked-mode
dotnet build BackDrop.sln -c Release -p:Platform=x64
```

Then run `src/BackDrop.App/bin/x64/Release/net10.0-windows10.0.26100.0/win-x64/BackDrop.App.exe` (or `dotnet publish` per `docs/ARCHITECTURE.md`).

---

## Usage

### Lock the screen with BackDrop

- Press **`Ctrl+Alt+L`** (default hotkey — change it in Settings), **or**
- Click **Lock Now** in the tray menu.

The overlay appears on every monitor. **Move the mouse or press any key to dismiss** — unless you've enabled a PIN (below).

### Change the look

Open **Settings** from the tray menu:

- **Layout** — Minimalist Center / Classic Bottom-Left / Bold Cinematic
- **Clock** — show/hide, seconds toggle
- **Vignette intensity** — slider, how strongly the darkening layer hugs the screen edges

### Import your own video loops

1. In **Settings → Media**, click **Import video…**
2. Pick a video file (H.264 MP4 recommended; see *Format notes*).
3. BackDrop validates the file before accepting it and shows a thumbnail.

Imported paths and thumbnails are cached in `settings.json` (per-user, in `%LocalAppData%\BackDrop\`). To remove an imported loop, select it in the media list and click **Remove**.

#### Format notes

- **Containers:** `.mp4`, `.m4v`, `.mkv`, `.mov`, `.wmv`, `.avi`, `.webm` are accepted; `.mp4` is the baseline.
- **Codec:** H.264/AVC is recommended; most H.264 MP4s from phones, cameras, and stock sites work. HEVC requires the device HEVC codec pack (Microsoft Store) and is not guaranteed.
- **Aspect/resolution:** any; playback scales to fill the screen. Loops are muted regardless of source audio.

### The optional PIN gate

BackDrop dismisses on any input by default. If you want a dismissal barrier:

1. **Settings → PIN gate → Set PIN** (4–12 digits).
2. Once set, dismissing the overlay requires entering the PIN. There is **no** hidden backdoor: `Esc` is disabled while the PIN gate is active (that's the point of the gate).

**Design guarantees:**

- The PIN is **never stored**. Only `salt + SHA-256(PIN + salt)`, encrypted with **DPAPI** (`ProtectedData`, CurrentUser scope) and persisted in `settings.json`. Decryption needs your Windows user credentials — it cannot be read by other user accounts, and it never touches the network.
- **Always recoverable:** open Settings from the tray and clear the PIN, or delete `%LocalAppData%\BackDrop\settings.json` to factory-reset. This is an opt-in aesthetic barrier, **never** a lockout risk.

---

## Free stock loops

BackDrop ships with one lightweight bundled loop (`default-loop.mp4`, 788 KB). Add more from these free sources (all allow personal/ambient use — check each license):

- **Coverr** — https://coverr.co (free stock video, CC0-ish license for many clips)
- **Videezy** — https://www.videezy.com (free + extended licenses)
- **Pexels Videos** — https://www.pexels.com/videos (free, no attribution required)

Download an MP4 (H.264) and import it via **Settings → Media → Import video…**

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| Overlay doesn't appear | Check the tray icon exists; re-press the hotkey. Only one instance runs — a second launch exits silently. |
| Video doesn't play | The file may be HEVC or an exotic codec — try an H.264 MP4 (see *Format notes*). |
| Mica/acrylic looks solid | Windows 10 has limited Mica support; the window falls back to a solid background automatically. |
| PIN overlay feels stuck | Enter the PIN you set. Recovery is always available: open **Settings** from the tray → **Remove PIN**, or delete `%LocalAppData%\BackDrop\settings.json`. The app never blocks Task Manager or other OS exits. |

---

## Project status

- **CI:** GitHub Actions (`build.yml`) — restore (locked) → build (Release, x64) → unit tests → artifact, on every push/PR to `main`.
- **Release:** `release.yml` — manual/tag-gated only (`v*` tag or `workflow_dispatch`), produces a zip + optional MSIX.

See `docs/ARCHITECTURE.md` for the full design, and the [GitHub Actions tab](https://github.com/archanagaur175-glitch/BackDrop/actions) for live build status.

---

## License & attribution

- Bundled loop: `mov_bbb.mp4` — Big Buck Bunny trailer (© Blender Foundation), used under the [Creative Commons Attribution 3.0](https://creativecommons.org/licenses/by/3.0/) license. Source: https://www.w3schools.com/html/mov_bbb.mp4
- App: see `LICENSE` in the repository root (if present) — otherwise all rights reserved by the author.
