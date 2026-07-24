using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PodcastSync.Downloads;

/// <summary>
/// Bounded <see cref="Channel{T}"/>-backed download queue drained by up to
/// <c>maxConcurrency</c> workers (default 3). Supports pause/resume of enqueueing.
/// </summary>
public sealed class DownloadQueue : IDisposable
{
    private readonly ResumableDownloader _downloader;
    private readonly int _maxConcurrency;
    private readonly Channel<DownloadWork> _channel;
    private readonly List<Task> _workers = new();
    private int _paused;

    public DownloadQueue(ResumableDownloader downloader, int maxConcurrency = 3)
    {
        _downloader = downloader;
        _maxConcurrency = Math.Max(1, maxConcurrency);
        _channel = Channel.CreateBounded<DownloadWork>(128);
    }

    public void Start()
    {
        for (var i = 0; i < _maxConcurrency; i++)
        {
            _workers.Add(Task.Run(WorkerLoop));
        }
    }

    public async Task EnqueueAsync(DownloadWork work)
    {
        if (Volatile.Read(ref _paused) != 0)
        {
            throw new QueuePausedException();
        }

        await _channel.Writer.WriteAsync(work);
    }

    public void Pause() => Volatile.Write(ref _paused, 1);

    public void Resume() => Volatile.Write(ref _paused, 0);

    public void Complete() => _channel.Writer.TryComplete();

    public Task Completion => Task.WhenAll(_workers);

    private async Task WorkerLoop()
    {
        await foreach (var work in _channel.Reader.ReadAllAsync())
        {
            try
            {
                var total = await _downloader.DownloadAsync(work.Url, work.Destination);
                work.CompleteSuccess(total);
            }
            catch (Exception exception)
            {
                work.CompleteFailure(exception);
            }
        }
    }

    public void Dispose() => _channel.Writer.TryComplete();
}
