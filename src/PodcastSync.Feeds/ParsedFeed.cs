using System;
using System.Collections.Generic;

namespace PodcastSync.Feeds;

/// <summary>
/// A normalized episode extracted from a feed item, ready to be persisted.
/// </summary>
public sealed class FeedEpisode
{
    public string Guid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public int DurationSeconds { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
}

/// <summary>
/// A normalized subscription plus its episodes parsed from a feed.
/// </summary>
public sealed class ParsedFeed
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<FeedEpisode> Episodes { get; set; } = Array.Empty<FeedEpisode>();
}
