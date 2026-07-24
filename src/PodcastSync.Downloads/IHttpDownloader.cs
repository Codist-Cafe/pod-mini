using System.Threading;
using System.Threading.Tasks;

namespace PodcastSync.Downloads;

/// <summary>
/// Resumable HTTP downloader abstraction. <see cref="GetRangeAsync"/> returns the
/// bytes of a resource starting at <paramref name="start"/> (0 fetches the full body),
/// enabling resumable transfers via HTTP <c>Range</c> requests.
/// </summary>
public interface IHttpDownloader
{
    Task<byte[]> GetRangeAsync(string url, long start, CancellationToken cancellationToken = default);
}
