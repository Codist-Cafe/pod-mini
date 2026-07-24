namespace PodcastSync.Domain;

/// <summary>
/// Finite download state machine for an episode.
/// Pending (0) -> Downloading (1) -> Downloaded (2), with Failed (3) as a
/// recoverable terminal state that can be reset to Pending for retry.
/// </summary>
public enum DownloadState
{
    Pending = 0,
    Downloading = 1,
    Downloaded = 2,
    Failed = 3
}
