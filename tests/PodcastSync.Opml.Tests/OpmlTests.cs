using System.Linq;
using PodcastSync.Domain;
using Xunit;

namespace PodcastSync.Opml.Tests;

public class OpmlExportTests
{
    [Fact]
    public void Export_ProducesValidOpmlWithOutlines()
    {
        var subs = new[]
        {
            new Subscription { Title = "Podcast A", FeedUrl = "https://x/a.xml", SiteUrl = "https://a.com" },
            new Subscription { Title = "Podcast B", FeedUrl = "https://x/b.xml" },
        };

        var xml = OpmlExporter.Export(subs);

        Assert.Contains("version=\"2.0\"", xml);
        Assert.Contains("text=\"Podcast A\"", xml);
        Assert.Contains("xmlUrl=\"https://x/a.xml\"", xml);
        Assert.Contains("text=\"Podcast B\"", xml);
        Assert.Contains("xmlUrl=\"https://x/b.xml\"", xml);
        Assert.StartsWith("<?xml", xml);
    }

    [Fact]
    public void Export_EmptyList_ProducesValidEmptyOutline()
    {
        var xml = OpmlExporter.Export(System.Array.Empty<Subscription>());

        Assert.Contains("version=\"2.0\"", xml);
    }
}

public class OpmlImportTests
{
    [Fact]
    public void Import_ExtractsOnlyFeedUrlsWithXmlUrl()
    {
        var opml = """<?xml version="1.0"?><opml version="2.0"><body><outline text="Show A" type="rss" xmlUrl="https://x/a.xml"/><outline text="Folder"><outline text="Show B" xmlUrl="https://x/b.xml"/></outline><outline text="No feed"/></body></opml>""";

        var urls = OpmlImporter.ImportFeedUrls(opml);

        Assert.Equal(2, urls.Count);
        Assert.Contains("https://x/a.xml", urls);
        Assert.Contains("https://x/b.xml", urls);
        Assert.DoesNotContain(null, urls);
    }

    [Fact]
    public void Import_EmptyBody_ReturnsEmptyList()
    {
        var opml = """<?xml version="1.0"?><opml version="2.0"><body/></opml>""";

        var urls = OpmlImporter.ImportFeedUrls(opml);

        Assert.Empty(urls);
    }
}
