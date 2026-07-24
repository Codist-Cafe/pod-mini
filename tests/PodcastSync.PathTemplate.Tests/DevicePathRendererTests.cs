using System;
using PodcastSync.PathTemplate;
using Xunit;

namespace PodcastSync.PathTemplate.Tests;

public class DevicePathRendererTests
{
    [Fact]
    public void Render_SubstitutesAndSanitizesKnownTokens()
    {
        var renderer = new DevicePathRenderer();

        var result = renderer.Render(
            "{ShowTitle}/{PublishDate:yyyy-MM-dd}_{Title}.mp3",
            showTitle: "Soft? Daily",
            publishDate: new DateTime(2026, 7, 24),
            title: "Net 10");

        Assert.Equal("Soft Daily/2026-07-24_Net 10.mp3", result);
    }

    [Fact]
    public void Render_PreservesUnknownTokens()
    {
        var renderer = new DevicePathRenderer();

        var result = renderer.Render("{Foo} {ShowTitle}", showTitle: "Show", publishDate: new DateTime(2026, 1, 1), title: "T");

        Assert.Contains("{Foo}", result);
        Assert.Contains("Show", result);
    }

    [Fact]
    public void Render_AppliesCustomDateFormatSuffix()
    {
        var renderer = new DevicePathRenderer();

        var result = renderer.Render("{PublishDate:yyyy_MM_dd}", showTitle: "S", publishDate: new DateTime(2026, 7, 24), title: "T");

        Assert.Equal("2026_07_24", result);
    }

    [Fact]
    public void Render_DefaultsDateToIso_WhenNoFormatGiven()
    {
        var renderer = new DevicePathRenderer();

        var result = renderer.Render("{PublishDate}", showTitle: "S", publishDate: new DateTime(2026, 7, 24), title: "T");

        Assert.Equal("2026-07-24", result);
    }

    [Fact]
    public void Render_LeavesUnclosedTokenLiteral()
    {
        var renderer = new DevicePathRenderer();

        var result = renderer.Render("pre {ShowTitle", showTitle: "Show", publishDate: new DateTime(2026, 1, 1), title: "T");

        Assert.Equal("pre {ShowTitle", result);
    }
}
