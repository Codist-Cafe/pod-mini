using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PodcastSync.Domain;
using PodcastSync.Storage;

namespace PodcastSync.Data;

/// <summary>
/// Persists subscriptions and episodes. Enforces a unique feed URL, upserts
/// episodes by (SubscriptionId, Guid), cascades episode deletion, and optionally
/// removes the on-disk library folder via an injected <see cref="IFileSystem"/>.
/// </summary>
public sealed class SubscriptionRepository
{
    private readonly PodcastSyncDbContext _db;
    private readonly IFileSystem _fileSystem;

    public SubscriptionRepository(PodcastSyncDbContext db, IFileSystem fileSystem)
    {
        _db = db;
        _fileSystem = fileSystem;
    }

    public async Task<int> AddAsync(Subscription subscription, string? localFolderToCreate = null)
    {
        if (subscription.CreatedAt == default)
        {
            subscription.CreatedAt = DateTime.UtcNow;
        }

        _db.Subscriptions.Add(subscription);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateFeedUrlException(subscription.FeedUrl);
        }

        if (localFolderToCreate is not null)
        {
            _fileSystem.CreateDirectory(localFolderToCreate);
        }

        return subscription.Id;
    }

    public async Task UpsertEpisodeAsync(Episode episode)
    {
        var existing = await _db.Episodes.FirstOrDefaultAsync(
            e => e.SubscriptionId == episode.SubscriptionId && e.Guid == episode.Guid);

        if (existing is null)
        {
            _db.Episodes.Add(episode);
        }
        else
        {
            existing.Title = episode.Title;
            existing.PublishDate = episode.PublishDate;
            existing.DurationSeconds = episode.DurationSeconds;
            existing.AudioUrl = episode.AudioUrl;
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(int subscriptionId, SubscriptionRemovalMode mode, string? localFolder)
    {
        var subscription = await _db.Subscriptions.FindAsync(subscriptionId);
        if (subscription is null)
        {
            return;
        }

        _db.Subscriptions.Remove(subscription);
        await _db.SaveChangesAsync();

        if (mode == SubscriptionRemovalMode.RecordsAndFiles
            && localFolder is not null
            && _fileSystem.DirectoryExists(localFolder))
        {
            _fileSystem.DeleteDirectory(localFolder, recursive: true);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqliteException { SqliteErrorCode: 19 };
    }
}
