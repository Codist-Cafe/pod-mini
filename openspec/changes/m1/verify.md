# Verification Report — PodcastSync Core Engine (m1)

- **Date:** 2026-07-24
- **Change:** `openspec/changes/m1/`
- **Goal gate:** entire test suite passes cleanly; all projects at 100% line coverage.

## Test execution

```text
dotnet test PodcastSync.slnx --collect:"XPlat Code Coverage"
```

| Test project | Tests | Result |
|---|---|---|
| PodcastSync.Domain.Tests | 10 | ✅ all pass |
| PodcastSync.Storage.Tests | 24 | ✅ all pass |
| PodcastSync.PathTemplate.Tests | 5 | ✅ all pass |
| PodcastSync.Feeds.Tests | 16 | ✅ all pass |
| PodcastSync.Data.Tests | 7 | ✅ all pass |
| PodcastSync.Downloads.Tests | 5 | ✅ all pass |
| PodcastSync.DeviceSync.Tests | 7 | ✅ all pass |
| PodcastSync.Opml.Tests | 4 | ✅ all pass |
| **TOTAL** | **78** | **0 failed, 0 skipped** |

## Coverage report (ReportGenerator, aggregate)

```
Line coverage:  100%   (497 covered / 497 coverable lines, 0 uncovered)
Method coverage: 100% (111/111 methods)
Branch coverage: 93.1% (123/132 — residual in compiler-generated async state
                           machine code, not reachable from user code)
```

### Per-project line coverage

| Project | Classes | Line coverage |
|---|---|---|
| PodcastSync.Domain | 5 | **100%** |
| PodcastSync.Storage | 3 | **100%** |
| PodcastSync.PathTemplate | 1 | **100%** |
| PodcastSync.Feeds | 4 | **100%** |
| PodcastSync.Data | 3 | **100%** |
| PodcastSync.Downloads | 4 | **100%** |
| PodcastSync.DeviceSync | 5 | **100%** |
| PodcastSync.Opml | 2 | **100%** |

## Requirement → Spec → Test mapping

| Spec | Requirement | Test file / case |
|---|---|---|
| subscriptions | DownloadState transitions legal/illegal | `DownloadStateTransitionTests.cs` |
| subscriptions | Subscription + Episode entity defaults | `SubscriptionEntityTests.cs` |
| subscriptions | DeviceSettings round-trip | `DomainCoverageTests.cs` |
| subscriptions | Exception carries From/To/Message | `DomainCoverageTests.cs` |
| library-storage | Invalid chars stripped | `FileNameSanitizerTests.Sanitize_*` |
| library-storage | Space->underscore replacement | `FileNameSanitizerTests.Sanitize_ReplacesSpaces*` |
| library-storage | Reserved Windows names neutralized | `FileNameSanitizerTests.Sanitize_Neutralizes*` |
| library-storage | Trailing dots/spaces stripped | `FileNameSanitizerTests.Sanitize_StripsTrailing*` |
| library-storage | 240-char truncation + ext preserved | `FileNameSanitizerTests.Truncate_*` |
| library-storage | LibraryPathResolver composition | `LibraryPathResolverTests` |
| library-storage | Empty/invalid input → fallback | `StorageCoverageTests` |
| subscriptions (data) | Unique FeedUrl + duplicate rejection | `SubscriptionRepositoryTests.UniqueIndex*` |
| subscriptions (data) | Unique (SubId, Guid) index → upsert | `SubscriptionRepositoryTests.UpsertEpisode*` |
| subscriptions (data) | Cascade delete | `SubscriptionRepositoryTests.RemoveAsync_Cascades*` |
| subscriptions (data) | Repository add stores required fields | `SubscriptionRepositoryTests.AddAsync*` |
| subscriptions (data) | Remove RecordsOnly keeps folder | `SubscriptionRepositoryTests.RemoveAsync_RecordsOnly*` |
| subscriptions (data) | Remove RecordsAndFiles deletes folder | `SubscriptionRepositoryTests.RemoveAsync_RecordsAndFiles*` |
| feed-ingestion | RSS 2.0 → normalized records | `FeedParserTests.ParsesRss20*` |
| feed-ingestion | Atom feed equivalent | `FeedParserTests.ParsesAtomFeed*` |
| feed-ingestion | Enclosure-less items dropped | `FeedParserTests.DropsItems*` |
| feed-ingestion | GUID falls back to audio URL | `FeedParserTests.MissingGuid*` |
| feed-ingestion | Duration: plain seconds | `DurationParserTests.ParsesPlainSeconds` |
| feed-ingestion | Duration: H:MM:SS | `DurationParserTests.ParsesColon*` |
| feed-ingestion | Duration: missing → 0 | `DurationParserTests.MissingOrBlank*` |
| path-templating | Token substitution + sanitization | `DevicePathRendererTests.Render_*` |
| path-templating | Unknown tokens preserved | `DevicePathRendererTests.Render_PreservesUnknown*` |
| path-templating | Date format suffix | `DevicePathRendererTests.Render_AppliesCustom*` |
| download-engine | New download → full body + byte count | `ResumableDownloaderTests.Download_NewFile*` |
| download-engine | Partial file → range resume + append | `ResumableDownloaderTests.Download_PartialFile*` |
| download-engine | Bounded channel caps concurrency | `DownloadQueueTests.Queue_CapsConcurrency*` |
| download-engine | Pause rejects enqueue until Resume | `DownloadQueueTests.Pause_RejectsEnqueue*` |
| download-engine | Worker survives failed download | `DownloadQueueTests.Queue_FailedDownload*` |
| device-sync | Send-selected copies to rendered destination | `DeviceSyncServiceTests.SendSelected_*` |
| device-sync | Subscription sync = delta transfer | `DeviceSyncServiceTests.SubscriptionSync_*` |
| device-sync | Duplicate detection (name+size) skip | `DeviceSyncServiceTests.DuplicateDetection_*` |
| device-sync | Insufficient space → refuse, nothing copied | `DeviceSyncServiceTests.InsufficientSpace_*` |
| device-sync | Sufficient space → normal transfer | `DeviceSyncServiceTests.SufficientSpace_*` |
| opml | Export OPML 2.0 with outlines | `OpmlExportTests.Export_*` |
| opml | Import extracts xmlUrl-carrying outlines | `OpmlImportTests.Import_*` |

## Verification gate

✅ All 78 tests pass cleanly (0 failures, 0 skips).
✅ 8 of 8 projects at 100% line coverage (ReportGenerator).
✅ Every spec requirement mapped to one or more test cases.
✅ Evidence: `coverage-report/Summary.txt`, `coverage-report/index.html`.
