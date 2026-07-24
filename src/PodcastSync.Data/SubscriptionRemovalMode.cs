namespace PodcastSync.Data;

/// <summary>
/// How <see cref="SubscriptionRepository.RemoveAsync"/> treats on-disk files.
/// </summary>
public enum SubscriptionRemovalMode
{
    RecordsOnly,
    RecordsAndFiles,
}
