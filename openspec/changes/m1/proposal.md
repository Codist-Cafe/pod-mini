## Why

PodcastSync's reason for existing is a robust **download engine and library
manager** that can sync episodes onto external hardware players (Calibre-style).
Before any UI exists, the engine itself — feed parsing, the data model, the
download state machine, cross-platform filesystem layout, and the device-sync
delta logic — must be correct and fully covered by tests. This change delivers
that engine as pure, unit-testable .NET 10 libraries that reach 100% line
coverage, establishing the foundation every later milestone (GUI, live device
integration) depends on.

## What Changes

- Introduce the domain model (`Subscription`, `Episode`, `DeviceSettings`,
  `DownloadState`) and EF Core 10 data layer backed by SQLite.
- Add an asynchronous RSS/Atom feed-ingestion engine built on
  `System.ServiceModel.Syndication` + `HttpClientFactory`.
- Add cross-platform library storage: filename/path sanitization, library-root
  resolution, and folder layout rules.
- Add a `System.Threading.Channels`-based download engine with a finite download
  state machine (Pending → Downloading → Downloaded → Failed), configurable
  concurrency, and resumable HTTP range-request support.
- Add a Calibre-style device-sync engine: send-selected, subscription sync with
  delta calculation, free-space checks, and duplicate detection (size + name).
- Add OPML import/export so subscription lists interoperate with other clients.
- Add token-based device path templating (`{ShowTitle}`, `{PublishDate:yyyy-MM-dd}`,
  `{Title}`).

## Capabilities

### New Capabilities

- `subscriptions`: managing podcast subscriptions (add, remove with two delete
  modes, list) and persistence of subscription/episode records.
- `feed-ingestion`: fetching and parsing RSS/Atom feeds into normalized
  subscription + episode records.
- `library-storage`: cross-platform filename sanitization, library folder layout,
  and on-disk file placement.
- `download-engine`: the background download queue, state machine, concurrency,
  and resumable transfers.
- `device-sync`: sending/syncing downloaded episodes to an external device path
  with space and duplicate handling.
- `path-templating`: rendering device destination paths from tokens.
- `opml`: importing and exporting subscription lists in OPML 2.0.

### Modified Capabilities

(None — this is the first change; there are no existing specs in `openspec/specs/`.)

## Impact

- **Code**: new `src/` class libraries (`PodcastSync.Domain`,
  `PodcastSync.Data`, `PodcastSync.Feeds`, `PodcastSync.Downloads`,
  `PodcastSync.DeviceSync`, `PodcastSync.Storage`, `PodcastSync.Opml`) and
  matching xUnit test projects under `tests/`.
- **Dependencies**: `Microsoft.EntityFrameworkCore.Sqlite`,
  `System.ServiceModel.Syndication`. Test-only: `xunit`, `coverlet.msbuild`,
  `Microsoft.EntityFrameworkCore.InMemory`.
- **Out of scope (later milestones)**: Avalonia GUI, OS "open in default player"
  shell integration, live USB mount detection, actual audio playback. These are
  thin shells over the engine and are intentionally excluded so every project in
  m1 can reach 100% line coverage.
