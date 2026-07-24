using System;

namespace PodcastSync.Domain;

/// <summary>
/// A single episode belonging to a subscription. Deduplicated by
/// (SubscriptionId, Guid). Carries download progress and playback state.
/// </summary>
public sealed class Episode
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string Guid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public int DurationSeconds { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string? LocalFilePath { get; set; }
    public DownloadState DownloadState { get; set; } = DownloadState.Pending;
    public long FileSize { get; set; }
    public bool IsPlayed { get; set; }
}
