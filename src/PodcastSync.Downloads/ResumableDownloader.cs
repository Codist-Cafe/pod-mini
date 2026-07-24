using System.Threading;
using System.Threading.Tasks;
using PodcastSync.Storage;

namespace PodcastSync.Downloads;

/// <summary>
/// Downloads a URL to a destination path on an injected <see cref="IFileSystem"/>,
/// resuming from any existing partial file by issuing a range request starting at the
/// current file length.
/// </summary>
public sealed class ResumableDownloader
{
    private readonly IHttpDownloader _http;
    private readonly IFileSystem _fileSystem;

    public ResumableDownloader(IHttpDownloader http, IFileSystem fileSystem)
    {
        _http = http;
        _fileSystem = fileSystem;
    }

    public async Task<long> DownloadAsync(string url, string destination, CancellationToken cancellationToken = default)
    {
        var start = _fileSystem.FileExists(destination) ? _fileSystem.GetFileSize(destination) : 0;
        var bytes = await _http.GetRangeAsync(url, start, cancellationToken);

        if (start == 0)
        {
            _fileSystem.WriteAllBytes(destination, bytes);
        }
        else
        {
            _fileSystem.AppendAllBytes(destination, bytes);
        }

        return start + bytes.Length;
    }
}
