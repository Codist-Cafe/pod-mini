# PodcastSync

**Download engine and library manager for podcasts — with Calibre-style device sync.**

[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com)
[![tests](https://img.shields.io/badge/tests-78%20passed-brightgreen)](.)
[![coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)](.)

Subscribe to RSS feeds, download episodes into a clean local folder layout, and
sync MP3s to a USB thumb drive, MP3 player, or wearable device in one command.

## Quick start

```bash
# Install .NET 10 SDK if needed: https://dotnet.microsoft.com/download/dotnet/10.0

git clone https://github.com/Codist-Cafe/pod-mini.git
cd pod-mini
dotnet run --project src/PodcastSync.Cli -- subscribe "https://feeds.example.com/podcast.xml"
dotnet run --project src/PodcastSync.Cli -- fetch
dotnet run --project src/PodcastSync.Cli -- download --all
dotnet run --project src/PodcastSync.Cli -- device sync /media/usb-player
```

## Download

Pre-built single-file binaries for Linux, Windows, and macOS are on the
[Releases](https://github.com/Codist-Cafe/pod-mini/releases) page.

| Platform | Download |
|----------|----------|
| Linux x64 | `podcastsync-v1.0.0-linux-x64.tar.gz` |
| Windows x64 | `podcastsync-v1.0.0-win-x64.zip` |
| macOS x64 | `podcastsync-v1.0.0-osx-x64.tar.gz` |

Extract the archive and drop `podcastsync` (or `podcastsync.exe`) anywhere on your `PATH`.
No runtime required — the binary is self-contained.

## Features

- **RSS/Atom feed ingestion** — parses standard podcast feeds, skips non‑audio items, handles missing GUIDs
- **Clean local library** — episodes stored as `{Show}/{YYYY-MM-DD}_{Title}.mp3` with cross‑platform filename sanitization
- **Background download queue** — bounded concurrency (`System.Threading.Channels`), pause/resume, resumable partial downloads
- **Send‑to‑device** — Calibre‑style delta sync to external USB/thumb players with free‑space guard and duplicate skipping
- **OPML import/export** — interoperates with other podcast clients
- **SQLite storage** — embedded database, no server required
- **Cross‑platform** — .NET 10, tested on Linux, builds for Windows and macOS

## Build from source

```bash
git clone https://github.com/Codist-Cafe/pod-mini.git
cd pod-mini

# Run tests (78 tests, 100% line coverage)
dotnet test

# Run the CLI
dotnet run --project src/PodcastSync.Cli --

# Create release binaries for all platforms
./scripts/publish.sh
# Or package one platform:
./scripts/publish.sh linux-x64
```

## Architecture

```
src/
  PodcastSync.Cli/          # Console entry point (System.CommandLine)
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
