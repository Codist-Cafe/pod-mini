using System;

namespace PodcastSync.Domain;

/// <summary>
/// A podcast subscription. Persists to the Subscriptions table; its feed URL is
/// globally unique and its episodes cascade-delete with it.
/// </summary>
public sealed class Subscription
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FeedUrl { get; set; } = string.Empty;
    public string? SiteUrl { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string LocalFolderName { get; set; } = string.Empty;
    public DateTime? LastFetchedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
