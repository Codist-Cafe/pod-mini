namespace PodcastSync.DeviceSync;

/// <summary>
/// Reports available free space on a target volume so transfers can be guarded.
/// </summary>
public interface IVolumeInfo
{
    long GetAvailableFreeSpace(string path);
}
