namespace PodcastSync.Data;

/// <summary>
/// Thrown when adding a subscription whose feed URL is already stored.
/// </summary>
public sealed class DuplicateFeedUrlException : Exception
{
    public DuplicateFeedUrlException(string feedUrl)
        : base($"A subscription for feed URL '{feedUrl}' already exists.")
    {
        FeedUrl = feedUrl;
    }

    public string FeedUrl { get; }
}
