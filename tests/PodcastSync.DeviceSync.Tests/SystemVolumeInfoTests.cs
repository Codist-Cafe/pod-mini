using System.IO;
using PodcastSync.DeviceSync;
using Xunit;

namespace PodcastSync.DeviceSync.Tests;

public class SystemVolumeInfoTests
{
    [Fact]
    public void GetAvailableFreeSpace_ForRootedPath_ReturnsNonNegative()
    {
        var volume = new SystemVolumeInfo();

        var free = volume.GetAvailableFreeSpace(Path.GetTempPath());

        Assert.True(free >= 0);
    }

    [Fact]
    public void GetAvailableFreeSpace_ForRelativePath_ReturnsZero()
    {
        var volume = new SystemVolumeInfo();

        var free = volume.GetAvailableFreeSpace("no-root-relative-path");

        Assert.Equal(0, free);
    }
}
