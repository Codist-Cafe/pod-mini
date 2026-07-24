using System.Collections.Generic;
using System.IO;
using PodcastSync.PathTemplate;
using PodcastSync.Storage;

namespace PodcastSync.DeviceSync;

/// <summary>
/// Calibre-style device sync engine. Renders destinations from a path pattern,
/// skips duplicates (same name + size), guards against insufficient free space,
/// and copies the planned delta to the target device path.
/// </summary>
public sealed class DeviceSyncService
{
    private readonly IVolumeInfo _volumeInfo;
    private readonly IFileSystem _fileSystem;
    private readonly DevicePathRenderer _renderer;

    public DeviceSyncService(IVolumeInfo volumeInfo, IFileSystem fileSystem, DevicePathRenderer renderer)
    {
        _volumeInfo = volumeInfo;
        _fileSystem = fileSystem;
        _renderer = renderer;
    }

    public DeviceTransferResult TransferAsync(
        IReadOnlyList<DeviceTransferItem> items,
        string deviceRoot,
        string pathPattern)
    {
        var plan = new List<(DeviceTransferItem Item, string Destination)>(items.Count);
        var skipped = 0;
        long bytesNeeded = 0;

        foreach (var item in items)
        {
            var rendered = _renderer.Render(pathPattern, item.ShowTitle, item.PublishDate, item.Title);
            var destination = Path.Join(deviceRoot, rendered);

            if (_fileSystem.FileExists(destination) && _fileSystem.GetFileSize(destination) == item.SizeBytes)
            {
                skipped++;
                continue;
            }

            plan.Add((item, destination));
            bytesNeeded += item.SizeBytes;
        }

        var available = _volumeInfo.GetAvailableFreeSpace(deviceRoot);
        if (bytesNeeded > available)
        {
            throw new InsufficientDeviceSpaceException(bytesNeeded, available);
        }

        var copied = 0;
        long bytesCopied = 0;
        foreach (var (item, destination) in plan)
        {
            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            _fileSystem.CopyFile(item.SourceFilePath, destination, overwrite: true);
            copied++;
            bytesCopied += item.SizeBytes;
        }

        return new DeviceTransferResult(copied, skipped, bytesCopied);
    }
}
