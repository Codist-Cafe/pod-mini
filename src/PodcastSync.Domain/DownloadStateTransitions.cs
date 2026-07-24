using System.Collections.Generic;

namespace PodcastSync.Domain;

/// <summary>
/// Thrown when an episode attempts a download-state transition that is not
/// permitted by the finite state machine.
/// </summary>
public sealed class IllegalDownloadTransitionException : InvalidOperationException
{
    public IllegalDownloadTransitionException(DownloadState from, DownloadState to)
        : base($"Illegal download state transition: {from} -> {to}.")
    {
        From = from;
        To = to;
    }

    public DownloadState From { get; }

    public DownloadState To { get; }
}

/// <summary>
/// Guards the <see cref="DownloadState"/> state machine. All legal edges are
/// enumerated explicitly so the rules live in one place.
/// </summary>
public static class DownloadStateTransitions
{
    private static readonly IReadOnlyDictionary<DownloadState, IReadOnlySet<DownloadState>> Allowed =
        new Dictionary<DownloadState, IReadOnlySet<DownloadState>>
        {
            [DownloadState.Pending] = new HashSet<DownloadState> { DownloadState.Downloading, DownloadState.Failed },
            [DownloadState.Downloading] = new HashSet<DownloadState> { DownloadState.Downloaded, DownloadState.Failed },
            [DownloadState.Failed] = new HashSet<DownloadState> { DownloadState.Pending },
            [DownloadState.Downloaded] = new HashSet<DownloadState>(),
        };

    public static bool CanTransition(DownloadState from, DownloadState to)
    {
        return Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public static DownloadState Transition(DownloadState from, DownloadState to)
    {
        if (!CanTransition(from, to))
        {
            throw new IllegalDownloadTransitionException(from, to);
        }

        return to;
    }
}
