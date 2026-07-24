namespace PodcastSync.DeviceSync;

/// <summary>
/// Outcome of a device transfer.
/// </summary>
public sealed class DeviceTransferResult
{
    public DeviceTransferResult(int copied, int skipped, long bytesCopied)
    {
        Copied = copied;
        Skipped = skipped;
        BytesCopied = bytesCopied;
    }

    public int Copied { get; }
    public int Skipped { get; }
    public long BytesCopied { get; }
}
