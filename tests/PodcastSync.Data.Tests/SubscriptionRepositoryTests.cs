using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PodcastSync.Data;
using PodcastSync.Domain;
using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.Data.Tests;

internal sealed class InMemoryFileSystem : IFileSystem
{
    public HashSet<string> Directories { get; } = new();
    public Dictionary<string, byte[]> Files { get; } = new();

    private static string Norm(string path) => path.Replace('\\', '/');

    public bool DirectoryExists(string path) => Directories.Contains(Norm(path));
    public void CreateDirectory(string path) => Directories.Add(Norm(path));
    public void DeleteDirectory(string path, bool recursive) => Directories.Remove(Norm(path));
    public bool FileExists(string path) => Files.ContainsKey(Norm(path));
    public void WriteAllBytes(string path, byte[] bytes) => Files[Norm(path)] = bytes;
    public void AppendAllBytes(string path, byte[] bytes)
    {
        var key = Norm(path);
        Files.TryGetValue(key, out var existing);
        var buffer = new byte[(existing?.Length ?? 0) + bytes.Length];
        if (existing != null) System.Array.Copy(existing, 0, buffer, 0, existing.Length);
        System.Array.Copy(bytes, 0, buffer, existing?.Length ?? 0, bytes.Length);
        Files[key] = buffer;
    }
    public long GetFileSize(string path) => Files[Norm(path)].Length;
    public void CopyFile(string sourceFile, string destFile, bool overwrite) =>
        Files[Norm(destFile)] = Files[Norm(sourceFile)];
    public byte[] ReadAllBytes(string path) => Files[Norm(path)];
}

public abstract class DataTestBase
{
    protected static PodcastSyncDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys=ON;";
            pragma.ExecuteNonQuery();
        }

        var options = new DbContextOptionsBuilder<PodcastSyncDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new PodcastSyncDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    protected static Subscription NewSubscription(string title = "Show", string feed = "https://x/feed.xml") => new()
    {
        Title = title,
        FeedUrl = feed,
        LocalFolderName = title,
    };
}

public class SubscriptionRepositoryTests : DataTestBase
{
    [Fact]
    public async Task AddAsync_PersistsRequiredFields()
    {
        using var db = CreateContext();
        var fs = new InMemoryFileSystem();
        var repo = new SubscriptionRepository(db, fs);

        var id = await repo.AddAsync(NewSubscription("Software Engineering Daily", "https://x/sed.xml"));

        Assert.True(id > 0);
        var loaded = await db.Subscriptions.SingleAsync();
        Assert.Equal("Software Engineering Daily", loaded.Title);
        Assert.Equal("https://x/sed.xml", loaded.FeedUrl);
        Assert.True(loaded.CreatedAt > new System.DateTime(2020, 1, 1));
        Assert.Null(loaded.LastFetchedAt);
    }

    [Fact]
    public async Task UniqueIndexOnFeedUrl_RejectsDuplicate()
    {
        using var db = CreateContext();
        var repo = new SubscriptionRepository(db, new InMemoryFileSystem());
        await repo.AddAsync(NewSubscription(feed: "https://x/dup.xml"));

        var ex = await Assert.ThrowsAsync<DuplicateFeedUrlException>(() =>
            repo.AddAsync(NewSubscription("Other", "https://x/dup.xml")));

        Assert.Equal("https://x/dup.xml", ex.FeedUrl);
        Assert.Single(await db.Subscriptions.ToListAsync());
    }

    [Fact]
    public async Task UpsertEpisode_UpdatesExisting_WithoutDuplicating()
    {
        using var db = CreateContext();
        var repo = new SubscriptionRepository(db, new InMemoryFileSystem());
        var subId = await repo.AddAsync(NewSubscription());

        await repo.UpsertEpisodeAsync(new Episode { SubscriptionId = subId, Guid = "g1", Title = "Old", PublishDate = new System.DateTime(2026, 1, 1), AudioUrl = "https://x/a.mp3" });
        await repo.UpsertEpisodeAsync(new Episode { SubscriptionId = subId, Guid = "g1", Title = "New Title", PublishDate = new System.DateTime(2026, 1, 1), AudioUrl = "https://x/a.mp3" });

        var episodes = await db.Episodes.ToListAsync();
        Assert.Single(episodes);
        Assert.Equal("New Title", episodes[0].Title);
    }

    [Fact]
    public async Task RemoveAsync_CascadesEpisodes()
    {
        using var db = CreateContext();
        var repo = new SubscriptionRepository(db, new InMemoryFileSystem());
        var subId = await repo.AddAsync(NewSubscription());
        await repo.UpsertEpisodeAsync(new Episode { SubscriptionId = subId, Guid = "g", Title = "t", PublishDate = System.DateTime.UtcNow, AudioUrl = "u" });
        await repo.UpsertEpisodeAsync(new Episode { SubscriptionId = subId, Guid = "g2", Title = "t2", PublishDate = System.DateTime.UtcNow, AudioUrl = "u2" });

        await repo.RemoveAsync(subId, SubscriptionRemovalMode.RecordsOnly, localFolder: null);

        Assert.Empty(await db.Subscriptions.ToListAsync());
        Assert.Empty(await db.Episodes.ToListAsync());
    }

    [Fact]
    public async Task RemoveAsync_RecordsOnly_KeepsFolderOnDisk()
    {
        using var db = CreateContext();
        var fs = new InMemoryFileSystem();
        var repo = new SubscriptionRepository(db, fs);
        var subId = await repo.AddAsync(NewSubscription(), localFolderToCreate: "/pod/Show");

        await repo.RemoveAsync(subId, SubscriptionRemovalMode.RecordsOnly, localFolder: "/pod/Show");

        Assert.True(fs.DirectoryExists("/pod/Show"));
        Assert.Empty(await db.Subscriptions.ToListAsync());
    }

    [Fact]
    public async Task RemoveAsync_RecordsAndFiles_DeletesFolderOnDisk()
    {
        using var db = CreateContext();
        var fs = new InMemoryFileSystem();
        var repo = new SubscriptionRepository(db, fs);
        var subId = await repo.AddAsync(NewSubscription(), localFolderToCreate: "/pod/Show");

        await repo.RemoveAsync(subId, SubscriptionRemovalMode.RecordsAndFiles, localFolder: "/pod/Show");

        Assert.False(fs.DirectoryExists("/pod/Show"));
    }

    [Fact]
    public async Task RemoveAsync_OnMissingSubscription_IsNoop()
    {
        using var db = CreateContext();
        var repo = new SubscriptionRepository(db, new InMemoryFileSystem());

        await repo.RemoveAsync(9999, SubscriptionRemovalMode.RecordsOnly, localFolder: null);

        Assert.Empty(await db.Subscriptions.ToListAsync());
    }
}
