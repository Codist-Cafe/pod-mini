using System.IO;

namespace PodcastSync.DeviceSync;

/// <summary>
/// Production <see cref="IVolumeInfo"/> backed by <see cref="DriveInfo"/>.
/// </summary>
public sealed class SystemVolumeInfo : IVolumeInfo
{
    public long GetAvailableFreeSpace(string path)
    {
        var root = Path.GetPathRoot(path);
        if (string.IsNullOrEmpty(root))
        {
            return 0;
        }

        return new DriveInfo(root).AvailableFreeSpace;
    }
}
