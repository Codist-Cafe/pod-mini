namespace PodcastSync.DeviceSync;

/// <summary>
/// Thrown when the planned transfer exceeds the destination volume's free space.
/// </summary>
public sealed class InsufficientDeviceSpaceException : InvalidOperationException
{
    public InsufficientDeviceSpaceException(long bytesNeeded, long bytesAvailable)
        : base($"Transfer needs {bytesNeeded} bytes but only {bytesAvailable} are available on the device.")
    {
        BytesNeeded = bytesNeeded;
        BytesAvailable = bytesAvailable;
    }

    public long BytesNeeded { get; }

    public long BytesAvailable { get; }
}
