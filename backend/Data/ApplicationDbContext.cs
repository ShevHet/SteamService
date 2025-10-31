using Microsoft.EntityFrameworkCore;
using SteamService.Models;

namespace SteamService.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Models.Game> Games { get; set; }
    public DbSet<Models.Genre> Genres { get; set; }
    public DbSet<Models.Platform> Platforms { get; set; }
    public DbSet<Models.GameGenre> GameGenres { get; set; }
    public DbSet<Models.GamePlatform> GamePlatforms { get; set; }
    public DbSet<Models.GameSnapshot> GameSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Game>()
    .Property(g => g.Genres)
    .HasColumnType("jsonb");

        modelBuilder.Entity<Models.Game>()
            .Property(g => g.Platforms)
            .HasColumnType("jsonb");


        // Game configuration
        modelBuilder.Entity<Models.Game>(entity =>
        {
            entity.HasIndex(g => g.SteamAppId).IsUnique();
            entity.HasIndex(g => g.ReleaseDate);

            entity.Property(g => g.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(g => g.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // Genre configuration
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasIndex(g => g.Name).IsUnique();
            entity.Property(g => g.CreatedAtUtc).HasDefaultValueSql("NOW()");
            entity.Property(g => g.UpdatedAtUtc).HasDefaultValueSql("NOW()");
        });

        // Platform configuration
        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Property(p => p.CreatedAtUtc).HasDefaultValueSql("NOW()");
            entity.Property(p => p.UpdatedAtUtc).HasDefaultValueSql("NOW()");
        });

        // GameGenre (many-to-many) configuration
        modelBuilder.Entity<GameGenre>(entity =>
        {
            entity.HasKey(gg => new { gg.GameId, gg.GenreId });

            entity.HasIndex(gg => gg.GameId);
            entity.HasIndex(gg => gg.GenreId);

            entity.HasOne(gg => gg.Game)
                .WithMany()
                .HasForeignKey(gg => gg.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gg => gg.Genre)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(gg => gg.CreatedAtUtc).HasDefaultValueSql("NOW()");
        });

        // GamePlatform (many-to-many) configuration
        modelBuilder.Entity<GamePlatform>(entity =>
        {
            entity.HasKey(gp => new { gp.GameId, gp.PlatformId });

            entity.HasIndex(gp => gp.GameId);
            entity.HasIndex(gp => gp.PlatformId);

            entity.HasOne(gp => gp.Game)
                .WithMany()
                .HasForeignKey(gp => gp.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.Platform)
                .WithMany(p => p.GamePlatforms)
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(gp => gp.CreatedAtUtc).HasDefaultValueSql("NOW()");
        });

        // GameSnapshot configuration
        modelBuilder.Entity<GameSnapshot>(entity =>
        {
            entity.HasIndex(gs => new { gs.GameId, gs.SnapshotDateUtc });
            entity.HasIndex(gs => gs.SnapshotDateUtc);

            // Настройка типа колонки RawData как jsonb
            entity.Property(gs => gs.RawData)
                .HasColumnType("jsonb");

            // GIN индекс для JSONB поля RawData
            entity.HasIndex(gs => gs.RawData)
                .HasMethod("GIN")
                .HasOperators("jsonb_ops");

            entity.HasOne(gs => gs.Game)
                .WithMany()
                .HasForeignKey(gs => gs.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(gs => gs.CreatedAtUtc).HasDefaultValueSql("NOW()");
        });
    }
}