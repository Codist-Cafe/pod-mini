using System.Threading.Tasks;

namespace PodcastSync.Downloads;

/// <summary>
/// A single unit of download work, tracked to completion via <see cref="Completion"/>.
/// </summary>
public sealed class DownloadWork
{
    private readonly TaskCompletionSource<long> _completion;

    public DownloadWork(string url, string destination)
    {
        Url = url;
        Destination = destination;
        _completion = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public string Url { get; }

    public string Destination { get; }

    public Task<long> Completion => _completion.Task;

    internal void CompleteSuccess(long totalBytes) => _completion.TrySetResult(totalBytes);

    internal void CompleteFailure(System.Exception exception) => _completion.TrySetException(exception);
}
