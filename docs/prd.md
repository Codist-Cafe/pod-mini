# Product Requirement Document (PRD)

## Project Name: PodcastSync

**Version:** 1.0.0

**Target Platform:** .NET 10 (Long-Term Support)

**Document Status:** Approved for Engineering

---

## 1. Product Overview & Vision

**PodcastSync** is a lightweight, cross-platform desktop application designed to subscribe to podcast RSS feeds, download episode media files into clean local folder structures, and sync audio files directly to external hardware storage (e.g., USB thumb players, MP3 players, or wearable devices).

Unlike feature-bloated media suites, **PodcastSync is fundamentally a download engine and library manager rather than a daily audio player**. Its standout feature is a dedicated **"Send to Device"** workflow—directly modeled after the e-book transfer experience in Calibre—tailored for users who consume audio on standalone hardware devices.

---

## 2. Technical Stack & Architecture (.NET 10)

| Layer | Component / Framework | Notes |
| --- | --- | --- |
| **Runtime** | **.NET 10 LTS (C# 14)** | Cross-platform build target (Windows, Linux, macOS). Native AOT compilation supported for instant startup and low memory overhead. |
| **GUI Framework** | **Avalonia UI** | Native cross-platform XAML framework providing uniform look and feel across Linux (GNOME/KDE), macOS, and Windows. |
| **Database** | **SQLite 3 via EF Core 10** | Embedded lightweight relational storage using `Microsoft.EntityFrameworkCore.Sqlite`. |
| **Feed Engine** | `System.ServiceModel.Syndication` + `HttpClientFactory` | Asynchronous, resilient RSS/Atom feed ingestion pipeline with HTTP/3 support via .NET 10. |
| **Concurrently Engine** | `System.Threading.Channels` | High-performance background queue for non-blocking file downloads and IO. |

---

## 3. Data Model & Database Schema

The application uses an embedded SQLite database (`podcastsync.db`) stored in the user's local application data directory.

```
┌─────────────────┐       1:N       ┌─────────────────┐
│  Subscriptions  ├─────────────────┤    Episodes     │
└─────────────────┘                 └─────────────────┘
                                             │
                                             │ M:1 (Optional)
                                    ┌────────┴────────┐
                                    │ DeviceSyncLogs  │
                                    └─────────────────┘

```

### 3.1 `Subscriptions` Table

```sql
CREATE TABLE Subscriptions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    FeedUrl TEXT NOT NULL UNIQUE,
    SiteUrl TEXT NULL,
    Description TEXT NULL,
    ImageUrl TEXT NULL,
    LocalFolderName TEXT NOT NULL,
    LastFetchedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

```

### 3.2 `Episodes` Table

```sql
CREATE TABLE Episodes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SubscriptionId INTEGER NOT NULL,
    Guid TEXT NOT NULL,
    Title TEXT NOT NULL,
    PublishDate DATETIME NOT NULL,
    DurationSeconds INTEGER DEFAULT 0,
    AudioUrl TEXT NOT NULL,
    LocalFilePath TEXT NULL,
    DownloadState INTEGER NOT NULL DEFAULT 0, -- 0: Pending, 1: Downloading, 2: Downloaded, 3: Failed
    FileSize INTEGER DEFAULT 0,
    IsPlayed BOOLEAN DEFAULT 0,
    FOREIGN KEY(SubscriptionId) REFERENCES Subscriptions(Id) ON DELETE CASCADE
);

```

### 3.3 `DeviceSettings` Table

```sql
CREATE TABLE DeviceSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceName TEXT NOT NULL,
    TargetPath TEXT NOT NULL,
    PathPattern TEXT NOT NULL DEFAULT '{ShowTitle}/{PublishDate}_{Title}.mp3',
    AutoCleanAfterDays INTEGER DEFAULT 0
);

```

---

## 4. File System & Directory Architecture

PodcastSync stores downloaded media files on disk in a predictable folder layout.

### 4.1 Local Library Storage Pattern

By default, files are saved inside the application's root library path (`~/Podcasts` or custom user setting):

```text
[LibraryRoot]/
├── Software Engineering Daily/
│   ├── 2026-07-20_Architecture_Patterns_in_NET_10.mp3
│   └── 2026-07-24_High_Performance_CSharp_14.mp3
├── History Today/
│   └── 2026-06-15_The_Industrial_Revolution.mp3
└── .podcastsync/
    └── podcastsync.db

```

### 4.2 File Sanitization Rules

To ensure cross-platform compatibility across FAT32, exFAT, ext4, and NTFS filesystems, all show titles and episode filenames are automatically sanitized:

* Strips invalid path characters (`\`, `/`, `:`, `*`, `?`, `"`, `<`, `>`, `|`).
* Replaces spaces with underscores if configured in user settings.
* Truncates filenames exceeding 240 characters.

---

## 5. Functional Requirements

### 5.1 Subscription Management

* **Add Subscription:** User inputs an RSS feed URL.
* System fetches feed metadata asynchronously.
* System parses title, image art, and episode list into SQLite.
* System creates the corresponding folder on disk.


* **Remove Subscription:**
* Displays a modal confirmation offering two choices:
* *Unsubscribe only:* Keeps existing local audio files on disk; removes DB records.
* *Unsubscribe and delete files:* Purges database entries **and** deletes the corresponding podcast folder on disk.




* **OPML Support:** Standardized OPML Import/Export to sync podcast lists with other clients.

### 5.2 Download Engine

* **Single Episode Download:** Click individual download icons next to any episode.
* **Global Subscription Download ("Download All"):**
* One-click action triggers a batch refresh across all active RSS feeds.
* Evaluates new episodes against the SQLite database.
* Queues undownloaded items into a background .NET 10 `Channel` worker queue with configurable max concurrency (default: 3 concurrent downloads).


* **Download Controls:** Pause, cancel, retry failed downloads, and clear finished items. Supports resumable HTTP range requests (`Accept-Ranges`).

### 5.3 Episode Preview / Basic Player

> **Scope Reminder:** PodcastSync is *not* a full-fledged media player, but includes basic audio preview functionality so users can inspect audio files.

* Compact bottom audio bar with simple controls:
* Play / Pause toggle.
* Audio seek bar (scrubber).
* Volume control slider.


* **External Player Trigger:** Quick context menu option: *"Open in System Default Media Player"* (launches VLC, MPV, Rhythmbox, or Windows Media Player via OS file handler).

### 5.4 Calibre-Style "Send to Device" Sync Engine

This is the central feature for transferring audio content to hardware MP3/thumb players.

```text
┌─────────────────────────┐          ┌──────────────────────────┐
│  PodcastSync Library    │          │  External Hardware Drive │
│  (PC Storage / SSD)     ├─────────►│  (USB Thumb Player)      │
└─────────────────────────┘  Sync /  └──────────────────────────┘
                             Export

```

* **Target Device Configuration:**
* Configure mount path or drive letter (e.g., `/media/user/MP3PLAYER/` or `E:\`).
* Specify target folder structure using tokens:
* `{ShowTitle}`
* `{PublishDate:yyyy-MM-dd}`
* `{Title}`




* **Transfer Workflows:**
1. **Send Selected Episode(s):** Right-click item(s) $\rightarrow$ **Send to Device**. Copies selected downloaded audio files directly to the target drive structure.
2. **Sync Subscription to Device:** Select a podcast feed $\rightarrow$ Click **Sync to Device**. Calculates missing downloaded episodes on the target device and performs a delta transfer.


* **Smart Storage Management:**
* Space Check: Verifies destination flash volume capacity prior to initiating transfer.
* Existing File Detection: Skips duplicate files using size + name comparison to save flash memory wear.



---

## 6. User Interface Specification

The interface follows a modern two-pane desktop workspace layout.

```text
+----------------------------------------------------------------------------------------------+
| 🎙️ PodcastSync      [ + Add Feed ]  [ 🔄 Refresh All ]  [ ⬇️ Download All ]    [ ⚙️ Settings ]|
+-------------------+--------------------------------------------------------------------------+
| SUB-LIST          | EPISODES: Software Engineering Daily                                     |
| ----------------- | ------------------------------------------------------------------------ |
| 📻 All Episodes   | [⬇️] 2026-07-24 | Architecture Patterns in .NET 10       | 42m | 45 MB |
| 🎙️ Show A   (3)   | [✔️] 2026-07-20 | Async Channels & Performance in C#     | 38m | 40 MB |
| 🎙️ Show B   (0)   | [⬇️] 2026-07-15 | SQLite Optimizations with EF Core 10   | 51m | 55 MB |
| 🎙️ Show C  (12)   |                                                                          |
|                   |                                                                          |
|                   +--------------------------------------------------------------------------+
|                   | SELECTED ITEM ACTIONS:                                                   |
|                   | [▶️ Preview]   [⬇️ Download]   [📲 Send to Device]   [🗑️ Delete File]     |
+-------------------+--------------------------------------------------------------------------+
| Preview Player: ▶️ 00:04 / 42:10  ━━━━━━●━━━━━━━━━━━━━━━━━━━ 🔊  [ Device: USB Drive (E:\) ] |
+----------------------------------------------------------------------------------------------+

```

---

## 7. Non-Functional Requirements & Performance Targets

* **Resource Footprint:**
* Idle Memory: $< 80\text{ MB}$ RAM on Linux/Windows.
* Startup Time: $< 1.5\text{ seconds}$ (Cold start using .NET 10 compilation optimizations).


* **Database Efficiency:** Indexed queries on `(SubscriptionId, Guid)` to render episode lists with $> 10,000$ records instantly without UI stutter.
* **Fault Tolerance:** Robust handling of disconnected target devices during file writes (graceful abort without corrupting local library).
* **Cross-Platform Parity:** Feature complete across Linux (Ubuntu, Debian, Fedora), Windows 10/11, and macOS.