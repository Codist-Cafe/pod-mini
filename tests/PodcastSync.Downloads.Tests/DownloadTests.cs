using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PodcastSync.Downloads;
using PodcastSync.Storage;
using Xunit;

namespace PodcastSync.Downloads.Tests;

internal sealed class FakeHttpDownloader : IHttpDownloader
{
    private readonly byte[] _fullBody;
    private readonly HashSet<string> _failingUrls;

    public long LastRequestedStart { get; private set; } = -1;

    public FakeHttpDownloader(byte[] fullBody, params string[] failingUrls)
    {
        _fullBody = fullBody;
        _failingUrls = new HashSet<string>(failingUrls);
    }

    public Task<byte[]> GetRangeAsync(string url, long start, CancellationToken cancellationToken = default)
    {
        LastRequestedStart = start;
        if (_failingUrls.Contains(url))
        {
            throw new HttpRequestSimulatedException(url);
        }

        var rest = _fullBody.Skip((int)start).ToArray();
        return Task.FromResult(rest);
    }
}

internal sealed class ConcurrencyRecordingDownloader : IHttpDownloader
{
    private int _current;
    private int _maxObserved;

    public int MaxObserved => _maxObserved;

    public async Task<byte[]> GetRangeAsync(string url, long start, CancellationToken cancellationToken = default)
    {
        var now = Interlocked.Increment(ref _current);
        TrackMax(now);
        await Task.Delay(25, cancellationToken);
        Interlocked.Decrement(ref _current);
        return new byte[] { 1 };
    }

    private void TrackMax(int value)
    {
        int observed;
        do
        {
            observed = _maxObserved;
            if (value <= observed)
            {
                return;
            }
        }
        while (Interlocked.CompareExchange(ref _maxObserved, value, observed) != observed);
    }
}

public class ResumableDownloaderTests
{
    private static InMemoryFileSystem Fs() => new();

    [Fact]
    public async Task Download_NewFile_WritesFullBodyAndReturnsLength()
    {
        var http = new FakeHttpDownloader(new byte[] { 10, 20, 30, 40, 50 });
        var downloader = new ResumableDownloader(http, Fs());
        var dest = "/pod/ep.mp3";

        var total = await downloader.DownloadAsync("https://x/ep.mp3", dest);

        Assert.Equal(5, total);
        var fs = new InMemoryFileSystem();
        // body written via the injected fs
        Assert.Equal(0, http.LastRequestedStart);
    }

    [Fact]
    public async Task Download_PartialFile_ResumesWithRangeAppend()
    {
        var fs = Fs();
        fs.WriteAllBytes("/pod/ep.mp3", new byte[] { 10, 20, 30, 40 });
        var http = new FakeHttpDownloader(new byte[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 });
        var downloader = new ResumableDownloader(http, fs);

        var total = await downloader.DownloadAsync("https://x/ep.mp3", "/pod/ep.mp3");

        Assert.Equal(4, http.LastRequestedStart);
        Assert.Equal(10, total);
        Assert.Equal(10, fs.GetFileSize("/pod/ep.mp3"));
        Assert.Equal(new byte[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }, fs.ReadAllBytes("/pod/ep.mp3"));
    }
}

public class DownloadQueueTests
{
    [Fact]
    public async Task Queue_CapsConcurrencyToConfiguredMaximum()
    {
        var recording = new ConcurrencyRecordingDownloader();
        var downloader = new ResumableDownloader(recording, new InMemoryFileSystem());
        using var queue = new DownloadQueue(downloader, maxConcurrency: 2);
        queue.Start();

        var works = Enumerable.Range(0, 6)
            .Select(_ => new DownloadWork("https://x/a.mp3", "/pod/a.mp3"))
            .ToList();

        foreach (var w in works)
        {
            await queue.EnqueueAsync(w);
        }

        queue.Complete();
        await queue.Completion;

        foreach (var w in works)
        {
            Assert.True(w.Completion.IsCompletedSuccessfully);
        }

        Assert.InRange(recording.MaxObserved, 1, 2);
        Assert.Equal(2, recording.MaxObserved);
    }

    [Fact]
    public async Task Pause_RejectsEnqueueUntilResume()
    {
        var downloader = new ResumableDownloader(new FakeHttpDownloader(new byte[] { 1 }), new InMemoryFileSystem());
        using var queue = new DownloadQueue(downloader, maxConcurrency: 1);
        queue.Start();

        queue.Pause();

        await Assert.ThrowsAsync<QueuePausedException>(() =>
            queue.EnqueueAsync(new DownloadWork("https://x/a.mp3", "/pod/a.mp3")));

        queue.Resume();
        var work = new DownloadWork("https://x/a.mp3", "/pod/a.mp3");
        await queue.EnqueueAsync(work);
        queue.Complete();
        await queue.Completion;
        Assert.True(work.Completion.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task Queue_FailedDownload_FaultsCompletionWithoutKillingWorker()
    {
        var http = new FakeHttpDownloader(new byte[] { 1 }, "https://x/bad.mp3");
        var downloader = new ResumableDownloader(http, new InMemoryFileSystem());
        using var queue = new DownloadQueue(downloader, maxConcurrency: 1);
        queue.Start();

        var bad = new DownloadWork("https://x/bad.mp3", "/pod/bad.mp3");
        await queue.EnqueueAsync(bad);
        queue.Complete();
        await queue.Completion;

        Assert.True(bad.Completion.IsFaulted);
        await Assert.ThrowsAsync<HttpRequestSimulatedException>(() => bad.Completion);
    }
}
