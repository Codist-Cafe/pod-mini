namespace PodcastSync.Downloads;

/// <summary>
/// Thrown when enqueueing work into a paused <see cref="DownloadQueue"/>.
/// </summary>
public sealed class QueuePausedException : InvalidOperationException
{
    public QueuePausedException()
        : base("The download queue is paused; resume it before enqueuing more work.")
    {
    }
}
