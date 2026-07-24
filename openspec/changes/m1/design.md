# Design: PodcastSync Core Engine (m1)

## Problem framing

PodcastSync's value is its engine: subscribe to feeds, download episodes into a
clean folder layout, and sync them onto external hardware. Before any GUI, the
engine must be correct and provably complete. The hard goal constraint is
**100% line coverage on every project**, which forces an architecture where all
behavior lives in pure, injectable .NET libraries rather than in UI or live-IO
shells.

## Current constraints

- .NET 10 (net10.0), C# 14.
- TDD mode is `strict`: RED → GREEN → REFACTOR, evidence per task.
- No GUI, no live USB detection in m1 (deferred to later milestones).
- EF Core 10 + SQLite for storage; `System.ServiceModel.Syndication` for feeds.
- Every project must be able to reach 100% line coverage.

## Proposed approach

### Solution layout

```
PodcastSync.sln
src/
  PodcastSync.Domain/          # entities, value objects, enums (no deps)
  PodcastSync.Storage/         # sanitization, library path resolution
  PodcastSync.Data/            # EF Core DbContext + repository
  PodcastSync.Feeds/           # RSS/Atom parsing, duration parsing
  PodcastSync.Downloads/       # state machine + Channels queue + resumable client
  PodcastSync.DeviceSync/      # delta sync, space check, duplicate detection
  PodcastSync.PathTemplate/    # device path rendering
  PodcastSync.Opml/            # import/export
tests/
  PodcastSync.*.Tests/         # one xUnit project per src project (parity for 100%)
```

### Architectural principles that make 100% coverage achievable

1. **Side effects behind interfaces.** All file IO (`IFileSystem`), HTTP
   (`IHttpDownloader`), clock (`ISystemClock`), and volume-info
   (`IVolumeInfo`) live behind small interfaces. Production code wraps the real
   BCL types; tests inject fakes. This removes the "untestable IO" coverage gap.
2. **Pure functions where possible.** Sanitization, path templating, feed
   parsing, delta computation, and duration parsing are pure functions over
   inputs — trivially covered.
3. **Guarded state machine.** `DownloadState` transitions are validated in one
   place, so every branch (legal + illegal) is a cheap unit test.
4. **No static singletons, no hidden `DateTime.Now`.** Everything time-related
   goes through `ISystemClock`, so timestamps are deterministic in tests.

### Data flow

```
RSS feed --(Feeds)--> normalized records --(Data)--> SQLite
                                                    |
                  user request --(Downloads)--> Channel queue --> IHttpDownloader --> IFileSystem
                                                    |
                              downloaded episodes --(DeviceSync)--> target volume (delta + space check)
                                                    |
                                            (PathTemplate) renders destination path
```

### Module responsibilities

- **Domain**: `Subscription`, `Episode`, `DeviceSettings` entities; `DownloadState`
  enum + `DownloadStateTransition` guard; value objects. Zero external deps.
- **Storage**: `FileNameSanitizer` (invalid chars, reserved names, trailing-dot
  collapse, 240-char truncation with extension preserved, optional space→`_`),
  `LibraryPathResolver`.
- **Data**: `PodcastSyncDbContext` (entities, unique index on
  `(SubscriptionId, Guid)`, cascade delete), `SubscriptionRepository` (add,
  unique-URL enforcement, two-mode remove, upsert episodes by GUID).
- **Feeds**: `FeedParser` wrapping `SyndicationFeed.Load`, `DurationParser`
  (plain seconds, `H:MM:SS`, default 0), enclosure filtering, GUID fallback.
- **Downloads**: `DownloadQueue` (`Channel<byte>` bounded, `maxConcurrency`
  workers), pause/resume, `ResumableDownloader` (HEAD-less `Range: bytes=N-`
  append).
- **DeviceSync**: `DeviceSyncService` (send-selected, subscription delta, space
  check, duplicate detection by name+size).
- **PathTemplate**: `DevicePathRenderer` token substitution with date format
  suffix, unknown-token passthrough.
- **Opml**: `OpmlExporter` / `OpmlImporter` (XML via `XDocument`).

## Data and API considerations

- SQLite connection string points at a file under the app data dir in production;
  tests use `Microsoft.EntityFrameworkCore.Sqlite` with `:memory:` (shared
  connection kept open for the context lifetime) so the data layer is exercised
  against the real provider without touching disk.
- Unique constraint on `Subscriptions.FeedUrl` is enforced both in the model and
  in the repository (throws `DuplicateFeedUrlException`).
- `Episodes` unique index on `(SubscriptionId, Guid)` enforces dedup at the DB;
  repository upserts by that key.

## Risks and tradeoffs

- **Tradeoff: interface proliferation vs. coverage.** Injecting every side effect
  adds seams, but it is the only way to hit 100% on IO-adjacent code. Accepted.
- **Tradeoff: SQLite in-memory vs. EF InMemory provider.** EF InMemory does not
  enforce unique constraints or cascades, so we use the real SQLite provider
  in-memory to actually exercise the schema rules.
- **Risk: `System.ServiceModel.Syndication` quirks.** It is lenient; we pin
  behavior with focused fixture feeds (RSS 2.0, Atom, duration variants,
  enclosure-less items).
- **Risk: coverage tools under-count generated code.** We exclude nothing by
  default and keep all generated/designer code out of the libraries.

## Verification notes

- Each src project has a 1:1 tests project; the solution test run is the single
  coverage source of truth.
- Coverage collected via `coverlet.msbuild` (`dotnet test
  --collect:"XPlat Code Coverage"`), rendered by ReportGenerator.
- The verify gate is: all projects pass, aggregate line coverage == 100%.
- Strict TDD: for every requirement, a failing test (RED) is committed before the
  implementation (GREEN), with a per-test commit showing the transition.
