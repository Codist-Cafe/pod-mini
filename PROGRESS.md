# PodcastSync — SDD Progress Log (milestone m1)

> Resilience log. If the context window resets, a fresh session reads this file
> to resume exactly where the last session stopped. Update after every committed
> test pass and every phase transition.

**Goal:** Implement `docs/prd.md` (PodcastSync core engine) via the pi-sdd-stack
strict-TDD apply loop (`/sdd-stack:continue m1`), committing each green test,
until the full suite passes at 100% coverage on all projects.

**SDD settings:** TDD mode = `strict` (RED → GREEN → REFACTOR, explicit evidence).
**Stack:** .NET 10 (net10.0), xUnit, coverlet.msbuild, ReportGenerator.
**Drive mode:** inline orchestration (write/read/bash/edit) with per-test git commits.

## SDD phase tracker

| Phase | Artifact | Status |
| --- | --- | --- |
| PRD | `openspec/changes/m1/prd.md` | ✅ done |
| PROPOSAL | `openspec/changes/m1/proposal.md` | ✅ done |
| SPEC | `openspec/changes/m1/specs/**/spec.md` (7 deltas) | ✅ done (valid) |
| DESIGN | `openspec/changes/m1/design.md` | ✅ done |
| TASKS | `openspec/changes/m1/tasks.md` (49 tasks) | ✅ done |
| APPLY | source under `src/`, tests under `tests/` | 🔄 in progress |
| VERIFY | `openspec/changes/m1/verify.md` | ⬜ pending |

## Milestone scope (m1)

m1 delivers the **core engine** (the PRD's "download engine and library manager"),
structured as pure, fully unit-testable .NET class libraries so that 100% line
coverage is achievable on every project:

- Domain model & value objects (Subscription, Episode, DeviceSettings, DownloadState)
- Data layer: EF Core 10 model + repository (SQLite, tested via in-memory/shared provider)
- Feed engine: RSS/Atom parsing via System.ServiceModel.Syndication
- Filesystem: cross-platform filename sanitization + library path resolution
- Path templating: `{ShowTitle}`, `{PublishDate:yyyy-MM-dd}`, `{Title}` device patterns
- Download engine: state machine + System.Threading.Channels worker queue + resumable range logic
- Device sync: delta calculation, free-space check, duplicate detection (size + name)
- OPML import/export

Out of m1 scope (later milestones): Avalonia GUI, OS file-launch integration,
live hardware mount detection. These are thin shells over the engine; the engine
itself reaches 100% coverage.

## Latest checkpoint

- [boot] git initialized; .NET10+xUnit+coverlet+reportgenerator verified working.
- [plan] openspec initialized (schema=spec-driven, pi profile). change `m1` created.
- [plan] proposal + 7 spec deltas (subscriptions, feed-ingestion, library-storage,
  download-engine, device-sync, path-templating, opml) written; `openspec validate m1` → valid.
- [plan] design.md + tasks.md (49 strict-TDD tasks) written.
- [next] APPLY: scaffold solution, then RED→GREEN per task with per-test commits.
- [apply] solution + Domain (10 tests, 100%) committed.
- [apply] Storage: sanitization + library path resolution (21 tests, 100%) committed.
- [apply] PathTemplate: device path rendering (5 tests, 100%) committed.
- [apply] Feeds: RSS/Atom parsing + duration (16 tests, 100%) committed.
- [apply] Storage: IFileSystem + SystemFileSystem (24 tests, 100%) committed.
- [apply] Data: EF Core Sqlite dbcontext + repository (7 tests, 100%) committed.
