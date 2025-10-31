// Tests/GamesControllerTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SteamService.Controllers.v1;
using SteamService.Data;
using SteamService.DTOs;
using SteamService.Models;
using Xunit;

public class GamesControllerTests
{
    private ApplicationDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(opts);
    }

    [Fact]
    public async Task GetGamesByMonth_FiltersByMonthGenrePlatform()
    {
        using var db = CreateDb();
        db.Games.AddRange(
            new Game
            {
                SteamAppId = 1,
                Name = "Action Game",
                ReleaseDate = new DateOnly(2025, 11, 10),
                Genres = new List<string> { "Action" },
                Platforms = new List<string> { "windows" }
            },
            new Game
            {
                SteamAppId = 2,
                Name = "RPG Game",
                ReleaseDate = new DateOnly(2025, 12, 1),
                Genres = new List<string> { "RPG" },
                Platforms = new List<string> { "windows" }
            });
        db.SaveChanges();

        var ctrl = new GamesController(db, NullLogger<GamesController>.Instance);

        var result = await ctrl.GetGamesByMonth("2025-11", "windows", "Action", default);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var games = Assert.IsAssignableFrom<IEnumerable<GameDto>>(ok.Value);

        Assert.Single(games);
        Assert.Equal("Action Game", games.First().Name);
    }

    [Fact]
    public async Task GetCalendarByMonth_GroupsByDay()
    {
        using var db = CreateDb();
        db.Games.AddRange(
            new Game { SteamAppId = 1, ReleaseDate = new DateOnly(2025, 11, 10) },
            new Game { SteamAppId = 2, ReleaseDate = new DateOnly(2025, 11, 10) },
            new Game { SteamAppId = 3, ReleaseDate = new DateOnly(2025, 11, 12) }
        );
        db.SaveChanges();

        var ctrl = new GamesController(db, NullLogger<GamesController>.Instance);

        var result = await ctrl.GetCalendarByMonth("2025-11", null, null, default);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var calendar = Assert.IsType<CalendarMonthDto>(ok.Value);

        Assert.Equal("2025-11", calendar.Month);
        Assert.Equal(2, calendar.Days.Count);

        var day10 = calendar.Days.First(d => d.Date.Day == 10);
        Assert.Equal(2, day10.Count);
    }
}
