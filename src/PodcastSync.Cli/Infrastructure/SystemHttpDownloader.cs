using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PodcastSync.Downloads;

namespace PodcastSync.Cli.Infrastructure;

/// <summary>
/// Production <see cref="IHttpDownloader"/> backed by <see cref="HttpClient"/>
/// with <c>Range</c> header support for resumable downloads.
/// </summary>
public sealed class SystemHttpDownloader : IHttpDownloader
{
    private static readonly HttpClient Client = new()
    {
        DefaultRequestHeaders = { { "User-Agent", "PodcastSync/1.0" } },
        Timeout = TimeSpan.FromMinutes(15),
    };

    public async Task<byte[]> GetRangeAsync(string url, long start, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (start > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, null);
        }

        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
