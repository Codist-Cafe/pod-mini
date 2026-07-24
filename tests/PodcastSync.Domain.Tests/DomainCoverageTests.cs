using System;
using PodcastSync.Domain;
using Xunit;

namespace PodcastSync.Domain.Tests;

public class DomainCoverageTests
{
    [Fact]
    public void DeviceSettings_RoundTripsAllFieldsWithDefaults()
    {
        var settings = new DeviceSettings
        {
            Id = 3,
            DeviceName = "Sansa Clip",
            TargetPath = "/media/sansa",
            AutoCleanAfterDays = 14
        };

        Assert.Equal(3, settings.Id);
        Assert.Equal("Sansa Clip", settings.DeviceName);
        Assert.Equal("/media/sansa", settings.TargetPath);
        Assert.Equal("{ShowTitle}/{PublishDate:yyyy-MM-dd}_{Title}.mp3", settings.PathPattern);
        Assert.Equal(14, settings.AutoCleanAfterDays);
    }

    [Fact]
    public void IllegalDownloadTransitionException_CarriesStateAndMessage()
    {
        var ex = Assert.Throws<IllegalDownloadTransitionException>(() =>
            DownloadStateTransitions.Transition(DownloadState.Downloaded, DownloadState.Pending));

        Assert.Equal(DownloadState.Downloaded, ex.From);
        Assert.Equal(DownloadState.Pending, ex.To);
        Assert.Contains("Downloaded", ex.Message);
        Assert.Contains("Pending", ex.Message);
    }

    [Fact]
    public void Subscription_CreatedAtIsRoundTrippable()
    {
        var now = new DateTime(2026, 7, 24, 9, 0, 0, DateTimeKind.Utc);
        var sub = new Subscription { Title = "T", FeedUrl = "u", CreatedAt = now };

        Assert.Equal(now, sub.CreatedAt);
    }

    [Fact]
    public void Episode_IdIsRoundTrippable()
    {
        var ep = new Episode { Id = 42, Guid = "g", Title = "t", AudioUrl = "a" };

        Assert.Equal(42, ep.Id);
    }
}
