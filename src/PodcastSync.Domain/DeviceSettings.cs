namespace PodcastSync.Domain;

/// <summary>
/// Persisted configuration for an external hardware device (USB player, etc.).
/// Mirrors the PRD's DeviceSettings table.
/// </summary>
public sealed class DeviceSettings
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public string PathPattern { get; set; } = "{ShowTitle}/{PublishDate:yyyy-MM-dd}_{Title}.mp3";
    public int AutoCleanAfterDays { get; set; }
}
