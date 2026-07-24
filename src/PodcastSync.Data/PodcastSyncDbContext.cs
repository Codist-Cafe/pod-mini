using Microsoft.EntityFrameworkCore;
using PodcastSync.Domain;

namespace PodcastSync.Data;

public sealed class PodcastSyncDbContext : DbContext
{
    public PodcastSyncDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Episode> Episodes => Set<Episode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var subscription = modelBuilder.Entity<Subscription>();
        subscription.HasKey(s => s.Id);
        subscription.Property(s => s.Title).IsRequired();
        subscription.Property(s => s.FeedUrl).IsRequired();
        subscription.Property(s => s.LocalFolderName).IsRequired();
        subscription.HasIndex(s => s.FeedUrl).IsUnique();

        var episode = modelBuilder.Entity<Episode>();
        episode.HasKey(e => e.Id);
        episode.Property(e => e.Guid).IsRequired();
        episode.Property(e => e.Title).IsRequired();
        episode.Property(e => e.AudioUrl).IsRequired();
        episode.HasIndex(e => new { e.SubscriptionId, e.Guid }).IsUnique();
        episode
            .HasOne<Subscription>()
            .WithMany()
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
