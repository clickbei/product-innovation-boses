using BosesApp.Core.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BosesApp.Core.Data;

/// <summary>
/// SQLite database context for Boses application
/// Provides primary persistence layer with JSON fallback capability
/// </summary>
public class BosesDbContext : DbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public BosesDbContext(DbContextOptions<BosesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => e.Email);

            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.LastLoginAt).HasDefaultValueSql("datetime('now')");
        });
    }
}
