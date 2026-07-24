using System;
using PodcastSync.Domain;
using Xunit;

namespace PodcastSync.Domain.Tests;

public class SubscriptionEntityTests
{
    [Fact]
    public void Subscription_RoundTripsRequiredFieldsAndDefaults()
    {
        var sub = new Subscription
        {
            Title = "Software Engineering Daily",
            FeedUrl = "https://example.com/feed.xml",
            SiteUrl = "https://example.com",
            Description = "A podcast",
            ImageUrl = "https://example.com/art.png",
            LocalFolderName = "Software Engineering Daily"
        };

        Assert.Equal("Software Engineering Daily", sub.Title);
        Assert.Equal("https://example.com/feed.xml", sub.FeedUrl);
        Assert.Equal("https://example.com", sub.SiteUrl);
        Assert.Equal("A podcast", sub.Description);
        Assert.Equal("https://example.com/art.png", sub.ImageUrl);
        Assert.Equal("Software Engineering Daily", sub.LocalFolderName);
        Assert.Null(sub.LastFetchedAt);
        Assert.Equal(0, sub.Id);
    }

    [Fact]
    public void Episode_DefaultsMatchContract()
    {
        var ep = new Episode
        {
            SubscriptionId = 7,
            Guid = "guid-1",
            Title = "An Episode",
            PublishDate = new DateTime(2026, 7, 24),
            AudioUrl = "https://example.com/ep.mp3"
        };

        Assert.Equal(DownloadState.Pending, ep.DownloadState);
        Assert.Equal(0, ep.DurationSeconds);
        Assert.Equal(0, ep.FileSize);
        Assert.False(ep.IsPlayed);
        Assert.Null(ep.LocalFilePath);
        Assert.Equal("guid-1", ep.Guid);
    }
}
