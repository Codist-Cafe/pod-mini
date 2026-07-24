using PodcastSync.Domain;
using Xunit;

namespace PodcastSync.Domain.Tests;

public class DownloadStateTransitionTests
{
    [Fact]
    public void LegalTransitions_AdvanceFromPendingToDownloaded()
    {
        var afterStart = DownloadStateTransitions.Transition(DownloadState.Pending, DownloadState.Downloading);
        var afterDone = DownloadStateTransitions.Transition(afterStart, DownloadState.Downloaded);

        Assert.Equal(DownloadState.Downloaded, afterDone);
    }

    [Fact]
    public void IllegalTransition_FromDownloadedToDownloading_IsRejected()
    {
        Assert.Throws<IllegalDownloadTransitionException>(() =>
            DownloadStateTransitions.Transition(DownloadState.Downloaded, DownloadState.Downloading));
    }

    [Fact]
    public void CanTransition_ReturnsTrueForLegalAndFalseForIllegal()
    {
        Assert.True(DownloadStateTransitions.CanTransition(DownloadState.Pending, DownloadState.Downloading));
        Assert.False(DownloadStateTransitions.CanTransition(DownloadState.Downloaded, DownloadState.Downloading));
    }

    [Fact]
    public void Failed_CanBeResetToPendingForRetry()
    {
        var failed = DownloadStateTransitions.Transition(DownloadState.Downloading, DownloadState.Failed);
        var retried = DownloadStateTransitions.Transition(failed, DownloadState.Pending);

        Assert.Equal(DownloadState.Pending, retried);
    }
}
