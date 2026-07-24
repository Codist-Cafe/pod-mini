using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PodcastSync.Cli.Infrastructure;
using PodcastSync.Data;
using PodcastSync.DeviceSync;
using PodcastSync.Downloads;
using PodcastSync.Feeds;
using PodcastSync.Opml;
using PodcastSync.PathTemplate;
using PodcastSync.Storage;

// ── Bootstrap ────────────────────────────────────────────────────────────────

var configDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "podcastsync");
Directory.CreateDirectory(configDir);

var dbPath = Path.Combine(configDir, "podcastsync.db");
var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();

using var pragma = connection.CreateCommand();
pragma.CommandText = "PRAGMA foreign_keys = ON;";
pragma.ExecuteNonQuery();

var options = new DbContextOptionsBuilder<PodcastSyncDbContext>()
    .UseSqlite(connection)
    .Options;

using var db = new PodcastSyncDbContext(options);
db.Database.EnsureCreated();

var fileSystem = new SystemFileSystem();
var httpDownloader = new SystemHttpDownloader();
var downloader = new ResumableDownloader(httpDownloader, fileSystem);

var libraryRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Podcasts");
var pathResolver = new LibraryPathResolver(libraryRoot);
var pathRenderer = new DevicePathRenderer();
var volumeInfo = new SystemVolumeInfo();

var repo = new SubscriptionRepository(db, fileSystem);
var feedParser = new FeedParser();
var deviceSync = new DeviceSyncService(volumeInfo, fileSystem, pathRenderer);

// ── Command dispatcher ───────────────────────────────────────────────────────

var cliArgs = Environment.GetCommandLineArgs()[1..];

if (cliArgs.Length == 0)
{
    PrintUsage();
    return;
}

var command = cliArgs[0];

try
{
    switch (command)
    {
        case "subscribe":
            await SubscribeAsync(cliArgs[1..]);
            break;
        case "list":
            await ListAsync();
            break;
        case "fetch":
            await FetchAsync(cliArgs[1..]);
            break;
        case "download":
            await DownloadAsync(cliArgs[1..]);
            break;
        case "device" when cliArgs.Length > 1 && cliArgs[1] == "sync":
            await DeviceSyncCommand(cliArgs[2..]);
            break;
        case "opml" when cliArgs.Length > 1 && cliArgs[1] == "import":
            await OpmlImportCommand(cliArgs[2..]);
            break;
        case "opml" when cliArgs.Length > 1 && cliArgs[1] == "export":
            await OpmlExportCommand(cliArgs[2..]);
            break;
        case "info":
            PrintInfo();
            break;
        default:
            Console.Error.WriteLine($"Unknown command: {command}");
            PrintUsage();
            break;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

// ── Commands ─────────────────────────────────────────────────────────────────

async Task SubscribeAsync(string[] a)
{
    if (a.Length < 1) { Console.Error.WriteLine("Usage: podcastsync subscribe <feed-url>"); return; }
    var url = a[0];
    Console.Write($"Fetching {url}... ");
    using var client = new HttpClient();
    var xml = await client.GetStringAsync(url);
    var feed = feedParser.Parse(xml);
    var sub = new PodcastSync.Domain.Subscription
    {
        Title = feed.Title,
        FeedUrl = url,
        Description = feed.Description,
        LocalFolderName = FileNameSanitizer.Sanitize(feed.Title),
    };
    var folder = pathResolver.FolderNameFor(feed.Title);
    var fullFolder = Path.Combine(libraryRoot, folder);
    var subId = await repo.AddAsync(sub, localFolderToCreate: fullFolder);
    Console.WriteLine($"added (id={subId}, {feed.Episodes.Count} episodes)");
}

async Task ListAsync()
{
    var subs = await db.Subscriptions.OrderBy(s => s.Title).ToListAsync();
    if (subs.Count == 0) { Console.WriteLine("No subscriptions."); return; }
    foreach (var s in subs)
    {
        var epCount = await db.Episodes.CountAsync(e => e.SubscriptionId == s.Id);
        var dlCount = await db.Episodes.CountAsync(e => e.SubscriptionId == s.Id && e.DownloadState == PodcastSync.Domain.DownloadState.Downloaded);
        Console.WriteLine($"  [{s.Id}] {s.Title}  ({dlCount}/{epCount} downloaded)  {s.FeedUrl}");
    }
}

async Task FetchAsync(string[] fetchArgs)
{
    List<PodcastSync.Domain.Subscription> subs;
    if (fetchArgs.Length > 0 && int.TryParse(fetchArgs[0], out var id))
    {
        var sub = await db.Subscriptions.FindAsync(id);
        subs = sub is not null ? new List<PodcastSync.Domain.Subscription> { sub } : new();
    }
    else
    {
        subs = await db.Subscriptions.ToListAsync();
    }

    using var client = new HttpClient();
    foreach (var sub in subs)
    {
        if (sub is null) continue;
        Console.WriteLine($"Fetching {sub.Title}...");
        var xml = await client.GetStringAsync(sub.FeedUrl);
        var feed = feedParser.Parse(xml);
        foreach (var ep in feed.Episodes)
        {
            await repo.UpsertEpisodeAsync(new PodcastSync.Domain.Episode
            {
                SubscriptionId = sub.Id,
                Guid = ep.Guid,
                Title = ep.Title,
                PublishDate = ep.PublishDate,
                DurationSeconds = ep.DurationSeconds,
                AudioUrl = ep.AudioUrl,
            });
        }
        sub.LastFetchedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        Console.WriteLine($"  updated ({feed.Episodes.Count} episodes)");
    }
}

async Task DownloadAsync(string[] dlArgs)
{
    var all = dlArgs.FirstOrDefault() == "--all" || dlArgs.Length == 0;
    var episodes = all
        ? await db.Episodes.Where(e => e.DownloadState == PodcastSync.Domain.DownloadState.Pending).ToListAsync()
        : dlArgs.Length > 0 && int.TryParse(dlArgs[0], out var id)
            ? await db.Episodes.Where(e => e.SubscriptionId == id && e.DownloadState == PodcastSync.Domain.DownloadState.Pending).ToListAsync()
            : new();

    if (episodes.Count == 0) { Console.WriteLine("Nothing to download."); return; }

    using var queue = new DownloadQueue(downloader, maxConcurrency: 3);
    queue.Start();

    var subs = (await db.Subscriptions.ToListAsync()).ToDictionary(s => s.Id);
    foreach (var ep in episodes)
    {
        var sub = subs.GetValueOrDefault(ep.SubscriptionId);
        var show = sub?.LocalFolderName ?? "unknown";
        var file = $"{ep.PublishDate:yyyy-MM-dd}_{FileNameSanitizer.Sanitize(ep.Title)}.mp3";
        var dest = pathResolver.ResolveLocalFilePath(show, file);
        var dir = Path.GetDirectoryName(dest);
        if (dir is not null) fileSystem.CreateDirectory(dir);

        var work = new DownloadWork(ep.AudioUrl, dest);
        _ = work.Completion.ContinueWith(async _ =>
        {
            ep.DownloadState = work.Completion.IsCompletedSuccessfully ? PodcastSync.Domain.DownloadState.Downloaded : PodcastSync.Domain.DownloadState.Failed;
            ep.FileSize = work.Completion.IsCompletedSuccessfully ? fileSystem.GetFileSize(dest) : 0;
            ep.LocalFilePath = dest;
            await db.SaveChangesAsync();
        });
        await queue.EnqueueAsync(work);
        Console.WriteLine($"  enqueued: {ep.Title}");
    }

    queue.Complete();
    await queue.Completion;
    Console.WriteLine("Downloads finished.");
}

async Task DeviceSyncCommand(string[] a)
{
    if (a.Length < 1) { Console.Error.WriteLine("Usage: podcastsync device sync <device-path> [--pattern <pattern>]"); return; }
    var deviceRoot = a[0];
    var pattern = a.SkipWhile(x => x != "--pattern").Skip(1).FirstOrDefault() ?? "{ShowTitle}/{Title}.mp3";

    var episodes = await db.Episodes
        .Where(e => e.DownloadState == PodcastSync.Domain.DownloadState.Downloaded && e.LocalFilePath != null)
        .ToListAsync();

    if (episodes.Count == 0) { Console.WriteLine("No downloaded episodes to sync."); return; }

    var items = new List<DeviceTransferItem>();
    var subs = (await db.Subscriptions.ToListAsync()).ToDictionary(s => s.Id);
    foreach (var ep in episodes)
    {
        var sub = subs.GetValueOrDefault(ep.SubscriptionId);
        items.Add(new DeviceTransferItem
        {
            ShowTitle = sub?.Title ?? "unknown",
            PublishDate = ep.PublishDate,
            Title = ep.Title,
            SourceFilePath = ep.LocalFilePath!,
            SizeBytes = ep.FileSize,
        });
    }

    var result = deviceSync.TransferAsync(items, deviceRoot, pattern);
    Console.WriteLine($"Synced: {result.Copied} copied, {result.Skipped} skipped ({result.BytesCopied} bytes).");
}

async Task OpmlImportCommand(string[] a)
{
    if (a.Length < 1) { Console.Error.WriteLine("Usage: podcastsync opml import <file>"); return; }
    var xml = await File.ReadAllTextAsync(a[0]);
    var urls = OpmlImporter.ImportFeedUrls(xml);
    Console.WriteLine($"Found {urls.Count} feed(s).");
    foreach (var url in urls)
    {
        Console.Write($"  Subscribing to {url}... ");
        var raw = await new HttpClient().GetStringAsync(url);
        var feed = feedParser.Parse(raw);
        var sub = new PodcastSync.Domain.Subscription
        {
            Title = feed.Title, FeedUrl = url, Description = feed.Description,
            LocalFolderName = FileNameSanitizer.Sanitize(feed.Title),
        };
        var id = await repo.AddAsync(sub);
        Console.WriteLine($"done (id={id})");
    }
}

async Task OpmlExportCommand(string[] a)
{
    if (a.Length < 1) { Console.Error.WriteLine("Usage: podcastsync opml export <file>"); return; }
    var subs = await db.Subscriptions.ToListAsync();
    var xml = OpmlExporter.Export(subs);
    await File.WriteAllTextAsync(a[0], xml);
    Console.WriteLine($"Exported {subs.Count} subscription(s) to {a[0]}.");
}

void PrintInfo()
{
    var subs = db.Subscriptions.Count();
    var eps = db.Episodes.Count();
    var dl = db.Episodes.Count(e => e.DownloadState == PodcastSync.Domain.DownloadState.Downloaded);
    Console.WriteLine($"PodcastSync 1.0.0");
    Console.WriteLine($"  DB: {dbPath}");
    Console.WriteLine($"  Library: {libraryRoot}");
    Console.WriteLine($"  Subscriptions: {subs}  Episodes: {eps}  Downloaded: {dl}");
}

static void PrintUsage()
{
    Console.WriteLine(@"PodcastSync 1.0.0 — podcast download engine & device sync

Usage:
  podcastsync subscribe <feed-url>    Add a podcast subscription
  podcastsync list                    List subscriptions
  podcastsync fetch [<sub-id>]        Refresh feeds (all or one)
  podcastsync download [--all]        Download pending episodes
  podcastsync device sync <path>      Sync downloaded episodes to device
  podcastsync opml import <file>      Import OPML subscription list
  podcastsync opml export <file>      Export OPML subscription list
  podcastsync info                    Show library stats");
}
