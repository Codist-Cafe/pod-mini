# Tasks: PodcastSync Core Engine (m1)

Strict-TDD checklist. For every requirement: write a failing test (RED), commit
it, make it pass with the minimum implementation (GREEN), commit, then refactor
(GREEN stays green), commit. Evidence = per-test commit + final coverage == 100%.

## 1. Solution scaffolding

- [ ] 1.1 Create `PodcastSync.sln` + Directory.Build.props (net10.0, nullable, treat warnings as errors).
- [ ] 1.2 Add `src/PodcastSync.Domain` + `tests/PodcastSync.Domain.Tests` (xUnit + coverlet).
- [ ] 1.3 Verify `dotnet test` runs green with coverage on the empty scaffold.

## 2. Domain model (specs/subscriptions, design)

- [ ] 2.1 RED: `Subscription` entity round-trips required fields + null LastFetchedAt.
- [ ] 2.2 RED: `DownloadState` legal transition Pending→Downloading→Downloaded.
- [ ] 2.3 RED: `DownloadState` illegal transition Downloaded→Downloading rejected.
- [ ] 2.4 RED: `DownloadState` Downloading→Failed→Pending (retry reset).
- [ ] 2.5 GREEN all of the above; commit per test.

## 3. Library storage (specs/library-storage)

- [ ] 3.1 RED: sanitizer strips invalid path chars.
- [ ] 3.2 RED: optional space→underscore replacement.
- [ ] 3.3 RED: reserved Windows names + trailing dot/space neutralized.
- [ ] 3.4 RED: filename >240 chars truncated, extension preserved.
- [ ] 3.5 RED: `LibraryPathResolver` composes root/show/episode with sanitization.
- [ ] 3.6 GREEN all; commit per test.

## 4. Data layer (specs/subscriptions)

- [ ] 4.1 RED: `PodcastSyncDbContext` has unique index on FeedUrl (duplicate rejected).
- [ ] 4.2 RED: unique index on (SubscriptionId, Guid) — upsert updates, no dup row.
- [ ] 4.3 RED: cascade delete removes episodes with subscription.
- [ ] 4.4 RED: repository add stores required fields.
- [ ] 4.5 RED: repository remove "records only" keeps folder on disk.
- [ ] 4.6 RED: repository remove "records and files" deletes folder on disk.
- [ ] 4.7 GREEN all (SQLite in-memory provider); commit per test.

## 5. Feed ingestion (specs/feed-ingestion)

- [ ] 5.1 RED: parse RSS 2.0 → subscription + episodes with title/date/GUID/audioUrl.
- [ ] 5.2 RED: parse Atom feed equivalently.
- [ ] 5.3 RED: enclosure-less items dropped.
- [ ] 5.4 RED: missing GUID falls back to audio URL.
- [ ] 5.5 RED: duration parses plain seconds.
- [ ] 5.6 RED: duration parses H:MM:SS.
- [ ] 5.7 RED: missing duration defaults to 0.
- [ ] 5.8 GREEN all; commit per test.

## 6. Path templating (specs/path-templating)

- [ ] 6.1 RED: tokens substituted + sanitized; unknown token preserved.
- [ ] 6.2 RED: date format suffix honored; default ISO date when absent.
- [ ] 6.3 GREEN all; commit per test.

## 7. Download engine (specs/download-engine)

- [ ] 7.1 RED: new download writes full body, records byte count.
- [ ] 7.2 RED: partial file resumes via `Range: bytes=N-` append.
- [ ] 7.3 RED: bounded channel caps concurrency.
- [ ] 7.4 RED: pause rejects enqueue until resume.
- [ ] 7.5 GREEN all (fake `IHttpDownloader` + `IFileSystem`); commit per test.

## 8. Device sync (specs/device-sync)

- [ ] 8.1 RED: send-selected copies chosen episodes to rendered destinations.
- [ ] 8.2 RED: subscription sync transfers only the delta.
- [ ] 8.3 RED: duplicate detection (name+size) skips copying.
- [ ] 8.4 RED: insufficient free space refuses transfer (nothing copied).
- [ ] 8.5 RED: sufficient free space proceeds.
- [ ] 8.6 GREEN all (fake `IFileSystem` + `IVolumeInfo`); commit per test.

## 9. OPML (specs/opml)

- [ ] 9.1 RED: export produces valid OPML 2.0 with one outline per subscription.
- [ ] 9.2 RED: import extracts only feed URLs that carry xmlUrl.
- [ ] 9.3 GREEN all; commit per test.

## 10. Verification

- [ ] 10.1 Full `dotnet test` clean across all projects.
- [ ] 10.2 Aggregate line coverage == 100% (ReportGenerator).
- [ ] 10.3 Write `openspec/changes/m1/verify.md` mapping requirements → evidence.

## Evidence required

- Per-task git commit showing RED→GREEN (commit message references task id).
- Final `dotnet test` output: 0 failed.
- Coverage report showing 100% line coverage on every src project.
- File paths changed under `src/` and `tests/`.
