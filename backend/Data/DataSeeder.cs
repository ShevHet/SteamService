using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteamService.Models;

namespace SteamService.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        var sampleGames = CreateSampleGames();
        var gamesBySlug = new Dictionary<string, Game>();
        var newlyAddedGames = new List<Game>();

        foreach (var sample in sampleGames)
        {
            var existing = await dbContext.Games
                .FirstOrDefaultAsync(g => g.Slug == sample.Slug, cancellationToken);

            if (existing is null)
            {
                dbContext.Games.Add(sample);
                newlyAddedGames.Add(sample);
                gamesBySlug[sample.Slug] = sample;
                logger.LogInformation("Inserted sample game {Game}", sample.Name);
            }
            else
            {
                gamesBySlug[sample.Slug] = existing;
            }
        }

        if (newlyAddedGames.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var snapshotDates = GetSnapshotDates();
        var random = new Random();
        var snapshotsInserted = 0;

        foreach (var (_, game) in gamesBySlug)
        {
            foreach (var date in snapshotDates)
            {
                var exists = await dbContext.GameSnapshots
                    .AnyAsync(s => s.GameId == game.Id && s.SnapshotDateUtc == date, cancellationToken);

                if (exists)
                {
                    continue;
                }

                dbContext.GameSnapshots.Add(new GameSnapshot
                {
                    GameId = game.Id,
                    SnapshotDateUtc = date,
                    FollowersCount = random.Next(5_000, 120_000),
                    RawData = "{}",
                    CreatedAtUtc = DateTime.UtcNow
                });

                snapshotsInserted++;
            }
        }

        if (snapshotsInserted > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Sample seeding completed: {Games} games ensured, {Snapshots} snapshots ensured", gamesBySlug.Count, snapshotsInserted);
    }

    private static List<Game> CreateSampleGames()
    {
        var now = DateTime.UtcNow;

        return new List<Game>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987654,
                Name = "Star Siege: Echoes of War",
                Slug = "star-siege-echoes-of-war",
                ShortDescription = "Тактический шутер в космосе с кооперативом на 4 игрока.",
                FullDescription = "Погрузитесь в эпические космические сражения, отряды на 4 игрока, система прогрессии и глубокий сюжет.",
                ReleaseDate = new DateTime(2025, 11, 3, 12, 0, 0, DateTimeKind.Utc),
                Price = 2499,
                MetacriticScore = 82,
                Genres = new List<string> { "Shooter", "Co-op", "Sci-fi" },
                Platforms = new List<string> { "windows" },
                StoreUrl = "https://store.steampowered.com/app/987654",
                HeaderImageUrl = "https://picsum.photos/id/1011/640/360",
                IsComingSoon = false,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987655,
                Name = "Mystic Tales: Emerald Prophecy",
                Slug = "mystic-tales-emerald-prophecy",
                ShortDescription = "Ролевая игра с открытым миром и системой выбора.",
                FullDescription = "Исследуйте магический мир, принимайте решения, собирайте артефакты и развивайте своего героя.",
                ReleaseDate = new DateTime(2025, 11, 12, 15, 0, 0, DateTimeKind.Utc),
                Price = 1999,
                MetacriticScore = 88,
                Genres = new List<string> { "RPG", "Adventure", "Fantasy" },
                Platforms = new List<string> { "windows", "linux" },
                StoreUrl = "https://store.steampowered.com/app/987655",
                HeaderImageUrl = "https://picsum.photos/id/1025/640/360",
                IsComingSoon = false,
                CreatedAt = now.AddDays(-12),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987656,
                Name = "Circuit Drift: Neon Legends",
                Slug = "circuit-drift-neon-legends",
                ShortDescription = "Аркадные гонки на футуристических автомобилях с мультиплеером.",
                FullDescription = "Соревнуйтесь в динамичных гонках, прокачивайте свои машины и участвуйте в мировых турнирах.",
                ReleaseDate = new DateTime(2025, 11, 19, 18, 0, 0, DateTimeKind.Utc),
                Price = 1499,
                MetacriticScore = 76,
                Genres = new List<string> { "Racing", "Arcade", "Multiplayer" },
                Platforms = new List<string> { "windows", "mac" },
                StoreUrl = "https://store.steampowered.com/app/987656",
                HeaderImageUrl = "https://picsum.photos/id/1015/640/360",
                IsComingSoon = false,
                CreatedAt = now.AddDays(-8),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987657,
                Name = "Quantum Chef: Galactic Kitchen",
                Slug = "quantum-chef-galactic-kitchen",
                ShortDescription = "Кооперативный симулятор ресторана с физикой.",
                FullDescription = "Управляйте рестораном в космосе, готовьте блюда из экзотических ингредиентов и обслуживайте пришельцев.",
                ReleaseDate = new DateTime(2025, 11, 24, 10, 0, 0, DateTimeKind.Utc),
                Price = 999,
                MetacriticScore = 81,
                Genres = new List<string> { "Simulation", "Casual", "Co-op" },
                Platforms = new List<string> { "windows", "mac", "linux" },
                StoreUrl = "https://store.steampowered.com/app/987657",
                HeaderImageUrl = "https://picsum.photos/id/1035/640/360",
                IsComingSoon = false,
                CreatedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987658,
                Name = "Aurora Tactics: Last Stand",
                Slug = "aurora-tactics-last-stand",
                ShortDescription = "Пошаговая стратегия с элементами roguelike.",
                FullDescription = "Комбинируйте способности, собирайте отряды и откройте секрет падения цивилизации.",
                ReleaseDate = new DateTime(2025, 11, 29, 9, 0, 0, DateTimeKind.Utc),
                Price = 1799,
                MetacriticScore = 84,
                Genres = new List<string> { "Strategy", "Tactics", "Roguelike" },
                Platforms = new List<string> { "windows" },
                StoreUrl = "https://store.steampowered.com/app/987658",
                HeaderImageUrl = "https://picsum.photos/id/1043/640/360",
                IsComingSoon = false,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987659,
                Name = "RetroVerse: Pixel Heroes",
                Slug = "retroverse-pixel-heroes",
                ShortDescription = "Ретро-экшен с процедурной генерацией уровней.",
                FullDescription = "Боритесь с волнами противников, улучшайте персонажа и спасите мир в пиксельном стиле.",
                ReleaseDate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc),
                Price = 799,
                MetacriticScore = 73,
                Genres = new List<string> { "Action", "Rogue-lite", "Pixel" },
                Platforms = new List<string> { "windows", "linux" },
                StoreUrl = "https://store.steampowered.com/app/987659",
                HeaderImageUrl = "https://picsum.photos/id/1055/640/360",
                IsComingSoon = false,
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                SteamAppId = 987660,
                Name = "Frostfall Chronicles",
                Slug = "frostfall-chronicles",
                ShortDescription = "Приключенческая survival-игра в снежном королевстве.",
                FullDescription = "Выживайте в суровом холоде, стройте убежища, выполняйте сюжетные задания и спасайте жителей.",
                ReleaseDate = new DateTime(2025, 12, 5, 16, 0, 0, DateTimeKind.Utc),
                Price = 2199,
                MetacriticScore = 0,
                Genres = new List<string> { "Survival", "Adventure", "Open World" },
                Platforms = new List<string> { "windows", "mac" },
                StoreUrl = "https://store.steampowered.com/app/987660",
                HeaderImageUrl = "https://picsum.photos/id/1062/640/360",
                IsComingSoon = true,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now
            }
        };
    }

    private static IReadOnlyList<DateTime> GetSnapshotDates()
    {
        var dates = new List<DateTime>();

        for (var month = 5; month <= 11; month++)
        {
            dates.Add(new DateTime(2025, month, 25, 0, 0, 0, DateTimeKind.Utc));
        }

        return dates;
    }
}
