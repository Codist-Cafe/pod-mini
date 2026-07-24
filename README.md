# PodcastSync

**Download engine and library manager for podcasts — with Calibre-style device sync.**

[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com)
[![tests](https://img.shields.io/badge/tests-78%20passed-brightgreen)](.)
[![coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)](.)

Subscribe to RSS feeds, download episodes into a clean local folder layout, and
sync MP3s to a USB thumb drive, MP3 player, or wearable device in one command.

## Install

### Option 1 — curl (terminal or AI agent)

```bash
curl -fsSL https://raw.githubusercontent.com/Codist-Cafe/pod-mini/master/scripts/install.sh | bash
```

Detects your OS, downloads the latest pre-built binary, and installs to
`~/.local/bin`.  Requires **.NET 10 SDK** if no pre-built binary matches your
platform (falls back to building from source).

Specify a version or custom path:

```bash
curl -fsSL https://raw.githubusercontent.com/Codist-Cafe/pod-mini/master/scripts/install.sh | bash -s -- --version 1.0.0 --install-dir /usr/local/bin
```

### Option 2 — pre-built binary

Download a single-file self-contained binary from
[Releases](https://github.com/Codist-Cafe/pod-mini/releases).  No .NET runtime
needed — just extract and drop into your `PATH`.

| Platform     | File |
|-------------|------|
| Linux x64   | `podcastsync-v1.0.0-linux-x64.tar.gz` |
| Windows x64 | `podcastsync-v1.0.0-win-x64.zip` |
| macOS x64   | `podcastsync-v1.0.0-osx-x64.tar.gz` |

```bash
tar xzf podcastsync-v1.0.0-linux-x64.tar.gz
sudo install linux-x64/podcastsync /usr/local/bin/
podcastsync info
```

### Option 3 — build from source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
git clone https://github.com/Codist-Cafe/pod-mini.git
cd pod-mini
dotnet run --project src/PodcastSync.Cli -- subscribe "https://feeds.example.com/podcast.xml"
```

## Quick start

```bash
podcastsync subscribe "https://feeds.example.com/podcast.xml"
podcastsync fetch
podcastsync download --all
podcastsync device sync /media/usb-player     # or E:\ on Windows
```

## Features

- **RSS/Atom feed ingestion** — parses standard podcast feeds, skips non‑audio items, handles missing GUIDs
- **Clean local library** — episodes stored as `{Show}/{YYYY-MM-DD}_{Title}.mp3` with cross‑platform filename sanitization
- **Background download queue** — bounded concurrency (`System.Threading.Channels`), pause/resume, resumable partial downloads
- **Send‑to‑device** — Calibre‑style delta sync to external USB/thumb players with free‑space guard and duplicate skipping
- **OPML import/export** — interoperates with other podcast clients
- **SQLite storage** — embedded database, no server required
- **Cross‑platform** — .NET 10, tested on Linux, builds for Windows and macOS

## Contributing

```bash
git clone https://github.com/Codist-Cafe/pod-mini.git
cd pod-mini

# Run tests (78 tests, 100% line coverage)
dotnet test

# Run the CLI during development
dotnet run --project src/PodcastSync.Cli --

# Package a release for all platforms
./scripts/publish.sh              # linux-x64, win-x64, osx-x64
./scripts/publish.sh linux-x64    # single platform

# Create a GitHub release
gh release create v1.0.0 publish/*.tar.gz publish/*.zip
```

## Architecture

```
src/
  PodcastSync.Cli/          # Console entry point (manual arg parser, zero extra deps)
  PodcastSync.Domain/       # Entities, enums, state machine
  PodcastSync.Storage/      # Filename sanitizer, IFileSystem abstraction
  PodcastSync.Data/         # EF Core 10 + SQLite, repository
  PodcastSync.Feeds/        # RSS/Atom parser, duration parser
  PodcastSync.Downloads/    # Channel-based download queue, resumable transfers
  PodcastSync.DeviceSync/   # Delta sync, space check, duplicate detection
  PodcastSync.PathTemplate/ # {ShowTitle}/{PublishDate}_{Title}.mp3 renderer
  PodcastSync.Opml/         # OPML 2.0 import/export
tests/                      # xUnit, 78 tests, 100% line coverage
```

## License

MIT
