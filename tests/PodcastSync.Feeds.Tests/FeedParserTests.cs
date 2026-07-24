using System;
using PodcastSync.Feeds;
using Xunit;

namespace PodcastSync.Feeds.Tests;

public class FeedParserTests
{
    private const string ItunesNs = "http://www.itunes.com/dtds/podcast-1.0.dtd";

    private static string Rss(string body) =>
        $"""<?xml version="1.0" encoding="UTF-8"?><rss version="2.0" xmlns:itunes="{ItunesNs}"><channel><title>Software Engineering Daily</title><description>Eng talks</description>{body}</channel></rss>""";

    [Fact]
    public void ParsesRss20_IntoSubscriptionAndEpisodes()
    {
        var xml = Rss("""
            <item>
              <title>Architecture Patterns in .NET 10</title>
              <guid>guid-arch</guid>
              <pubDate>Fri, 24 Jul 2026 00:00:00 GMT</pubDate>
              <itunes:duration>2520</itunes:duration>
              <enclosure url="https://x/arch.mp3" length="100" type="audio/mpeg"/>
            </item>
            <item>
              <title>Async Channels</title>
              <guid>guid-async</guid>
              <pubDate>Mon, 20 Jul 2026 00:00:00 GMT</pubDate>
              <itunes:duration>2280</itunes:duration>
              <enclosure url="https://x/async.mp3" length="90" type="audio/mpeg"/>
            </item>
        """);

        var feed = new FeedParser().Parse(xml);

        Assert.Equal("Software Engineering Daily", feed.Title);
        Assert.Equal("Eng talks", feed.Description);
        Assert.Equal(2, feed.Episodes.Count);
        var first = feed.Episodes[0];
        Assert.Equal("guid-arch", first.Guid);
        Assert.Equal("Architecture Patterns in .NET 10", first.Title);
        Assert.Equal(new DateTime(2026, 7, 24), first.PublishDate.Date);
        Assert.Equal(2520, first.DurationSeconds);
        Assert.Equal("https://x/arch.mp3", first.AudioUrl);
    }

    [Fact]
    public void ParsesAtomFeed_Equivalently()
    {
        var xml = $"""<?xml version="1.0"?><feed xmlns="http://www.w3.org/2005/Atom"><title>Atom Cast</title><entry><title>Atom Ep</title><id>atom-guid</id><updated>2026-07-24T00:00:00Z</updated><link href="https://x/atom.mp3" rel="enclosure" type="audio/mpeg" length="100"/></entry></feed>""";

        var feed = new FeedParser().Parse(xml);

        Assert.Equal("Atom Cast", feed.Title);
        Assert.Single(feed.Episodes);
        Assert.Equal("atom-guid", feed.Episodes[0].Guid);
        Assert.Equal("https://x/atom.mp3", feed.Episodes[0].AudioUrl);
    }

    [Fact]
    public void DropsItems_WithoutAudioEnclosure()
    {
        var xml = Rss("""
            <item>
              <title>Has Audio</title>
              <guid>g1</guid>
              <enclosure url="https://x/a.mp3" length="1" type="audio/mpeg"/>
            </item>
            <item>
              <title>Text only, no enclosure</title>
              <guid>g2</guid>
            </item>
        """);

        var feed = new FeedParser().Parse(xml);

        Assert.Single(feed.Episodes);
        Assert.Equal("g1", feed.Episodes[0].Guid);
    }

    [Fact]
    public void MissingGuid_FallsBackToAudioUrl()
    {
        var xml = Rss("""
            <item>
              <title>No guid</title>
              <enclosure url="https://x/noguid.mp3" length="1" type="audio/mpeg"/>
            </item>
        """);

        var feed = new FeedParser().Parse(xml);

        Assert.Equal("https://x/noguid.mp3", feed.Episodes[0].Guid);
    }

    [Fact]
    public void MissingDuration_DefaultsToZero()
    {
        var xml = Rss("""
            <item>
              <title>No duration</title>
              <guid>g</guid>
              <enclosure url="https://x/a.mp3" length="1" type="audio/mpeg"/>
            </item>
        """);

        var feed = new FeedParser().Parse(xml);

        Assert.Equal(0, feed.Episodes[0].DurationSeconds);
    }

    [Fact]
    public void ParsesDuration_InColonForm()
    {
        var xml = Rss("""
            <item>
              <title>Colon dur</title>
              <guid>g</guid>
              <itunes:duration>1:02:03</itunes:duration>
              <enclosure url="https://x/a.mp3" length="1" type="audio/mpeg"/>
            </item>
        """);

        var feed = new FeedParser().Parse(xml);

        Assert.Equal(3723, feed.Episodes[0].DurationSeconds);
    }

    [Fact]
    public void ParsesFeed_WithDocTypeDeclaration()
    {
        var xml = """<?xml version="1.0"?><!DOCTYPE rss><rss version="2.0"><channel><title>DTD Feed</title>""" +
            """<item><title>Ep</title><guid>g</guid><enclosure url="https://x/a.mp3" length="1" type="audio/mpeg"/></item></channel></rss>""";

        var feed = new FeedParser().Parse(xml);

        Assert.Equal("DTD Feed", feed.Title);
        Assert.Single(feed.Episodes);
    }
}
