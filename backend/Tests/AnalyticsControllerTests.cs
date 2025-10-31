// Tests/AnalyticsControllerTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SteamService.Controllers.v1;
using SteamService.Data;
using SteamService.DTOs;
using SteamService.Models;
using Xunit;

public class AnalyticsControllerTests
{
    private ApplicationDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(opts);
    }

    [Fact]
    public async Task GetGenreStats_Returns_Top5Genres()
    {
        using var db = CreateDb();

        var g1 = new Game { Id = 1, Genres = new List<string> { "Action", "Adventure" } };
        var g2 = new Game { Id = 2, Genres = new List<string> { "Action", "RPG" } };
        var g3 = new Game { Id = 3, Genres = new List<string> { "RPG" } };
        var g4 = new Game { Id = 4, Genres = new List<string> { "Strategy" } };
        var g5 = new Game { Id = 5, Genres = new List<string> { "Action" } };
        var g6 = new Game { Id = 6, Genres = new List<string> { "Simulation" } };

        db.Games.AddRange(g1, g2, g3, g4, g5, g6);
        db.GameSnapshots.AddRange(
            new GameSnapshot { GameId = 1, SnapshotDateUtc = new DateTime(2025, 10, 20), FollowersCount = 1000 },
            new GameSnapshot { GameId = 2, SnapshotDateUtc = new DateTime(2025, 10, 20), FollowersCount = 2000 },
            new GameSnapshot { GameId = 3, SnapshotDateUtc = new DateTime(2025, 10, 20), FollowersCount = 3000 },
            new GameSnapshot { GameId = 4, SnapshotDateUtc = new DateTime(2025, 10, 20), FollowersCount = 1500 },
            new GameSnapshot { GameId = 5, SnapshotDateUtc = new DateTime(2025, 10, 20), FollowersCount = 500 },
            new GameSnapshot { GameId = 6, SnapshotDateUtc = new DateTime(2025, 10, 20), FollowersCount = 1200 }
        );
        db.SaveChanges();

        var ctrl = new AnalyticsController(db, NullLogger<AnalyticsController>.Instance);

        var result = await ctrl.GetGenreStats(default);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var data = Assert.IsAssignableFrom<IEnumerable<GenreStatDto>>(ok.Value);

        Assert.True(data.Count() <= 5);
        var top = data.First();
        Assert.Equal("Action", top.Genre);
        Assert.True(top.Games >= 1);
        Assert.True(top.AvgFollowers > 0);
    }
}
