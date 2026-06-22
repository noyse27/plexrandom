# Plex Randomizer

A WPF desktop application for Windows that picks random movies from your Plex library and creates playlists — with smart year-balanced randomization and flexible filters.

---

## Features

- **Random playlist generation** — picks movies from your Plex movie library
- **Year-balanced algorithm** — selects a random year first, then a random movie from that year, so no decade dominates
- **Filters** — genre, year range (from/to), unwatched only, maximum duration
- **Direct Plex integration** — creates playlists on your Plex server; plays selected movies on any active Plex client via remote control
- **Recent playlists** — view and delete previously created random playlists
- **DE/EN language switching** — switch between German and English in the settings tab; preference is saved
- **Dark theme** — Plex-inspired color scheme
- **No installation required** — self-contained single `.exe`

---

## Download

Get the latest release from [GitHub Releases](https://github.com/noyse27/plexrandom/releases):

1. Download `plexrandom-v1.0-win-x64.zip`
2. Extract anywhere
3. Run `plexrandom.exe`

**Requirements:** Windows 10/11 x64 — no .NET runtime needed.

---

## Setup

1. Open the **Settings** tab
2. Enter your Plex server **URL** (e.g. `192.168.1.10`) and **Port** (default `32400`)
3. Enter your **Plex Token** — [how to find your token](https://support.plex.tv/articles/204059436-finding-an-authentication-token-x-plex-token/)
4. Click **Check Connection** — status turns green if successful
5. Click **Load Plex Libraries** — your movie libraries appear in the Randomize tab

Configuration is saved automatically to `config.json` next to the executable.

> **Note:** `config.json` contains your Plex token — do not share or commit it.

---

## Usage

### Randomize tab
1. Select a **Library**
2. Optionally set a **Genre**, **year range**, **unwatched only**, or **max duration**
3. Adjust the playlist size with the slider
4. Click **Randomize!** — the movie list is filled
5. Click **Send to Plex** to create the playlist on your Plex server
6. Double-click a movie or select it and click **Open in Plex** to start playback on the active Plex client

### Recent Playlists tab
- Shows all playlists created by this app
- Click the trash icon to delete a playlist from Plex and from the list

### Settings tab
- Configure server, token, defaults
- Switch language with **Deutsch / English** buttons

---

## Build from Source

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (WPF is Windows-only)

```bash
git clone https://github.com/noyse27/plexrandom.git
cd plexrandom
dotnet build plexrandom/plexrandom.csproj
```

### Run (debug)
```bash
dotnet run --project plexrandom/plexrandom.csproj
```

### Publish (self-contained, single file)
```bash
dotnet publish plexrandom/plexrandom.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/
```

---

## License

© PolzeSoft 2026 — [https://polze.net](https://polze.net)  
Contact: [plexrandom@polze.net](mailto:plexrandom@polze.net)
