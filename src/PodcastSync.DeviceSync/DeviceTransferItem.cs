using System;

namespace PodcastSync.DeviceSync;

/// <summary>
/// A downloaded episode targeted for transfer to a device.
/// </summary>
public sealed class DeviceTransferItem
{
    public string ShowTitle { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceFilePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
