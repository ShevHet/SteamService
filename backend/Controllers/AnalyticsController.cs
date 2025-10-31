using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamService.Data;
using SteamService.DTOs;
using SteamService.Models;

namespace SteamService.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(ApplicationDbContext db, ILogger<AnalyticsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("trends")]
        public async Task<ActionResult<IEnumerable<GenreTrendDto>>> GetGenreTrends(
            [FromQuery] int months = 3,
            [FromQuery] bool includeCurrentMonth = true,
            CancellationToken ct = default)
        {
            if (months <= 0)
                return BadRequest("months must be > 0");

            var monthsList = BuildMonthList(months, includeCurrentMonth);
            var results = new List<(string Month, DateTime SnapshotDateUtc)>();
            foreach (var month in monthsList)
            {
                var start = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);

                var snapshot = await _db.GameSnapshots
                    .Where(s => s.SnapshotDateUtc >= start && s.SnapshotDateUtc < end)
                    .OrderByDescending(s => s.SnapshotDateUtc)
                    .Select(s => s.SnapshotDateUtc)
                    .FirstOrDefaultAsync(ct);

                if (snapshot == default)
                {
                    snapshot = await _db.GameSnapshots
                        .Where(s => s.SnapshotDateUtc < end)
                        .OrderByDescending(s => s.SnapshotDateUtc)
                        .Select(s => s.SnapshotDateUtc)
                        .FirstOrDefaultAsync(ct);
                }

                if (snapshot != default)
                    results.Add(($"{month:yyyy-MM}", snapshot));
            }

            if (results.Count == 0)
                return NotFound("No snapshot data available for the requested period");

            var monthGenreStats = new Dictionary<string, List<(string Genre, long AvgFollowers, int Games)>>();

            foreach (var (monthStr, snapDate) in results)
            {
                var dayStart = DateTime.SpecifyKind(snapDate.Date, DateTimeKind.Utc);
                var dayEnd = dayStart.AddDays(1);

                var monthSnapshotsList = await _db.GameSnapshots.AsNoTracking()
                    .Where(s => s.SnapshotDateUtc >= dayStart && s.SnapshotDateUtc < dayEnd)
                    .ToListAsync(ct);

                if (monthSnapshotsList.Count == 0)
                {
                    monthGenreStats[monthStr] = new List<(string Genre, long AvgFollowers, int Games)>();
                    continue;
                }

                var gameIds = monthSnapshotsList.Select(s => s.GameId).Distinct().ToList();
                var games = await _db.Games.AsNoTracking()
                    .Where(g => gameIds.Contains(g.Id))
                    .ToDictionaryAsync(g => g.Id, ct);

                var joined = monthSnapshotsList
                    .Where(s => games.ContainsKey(s.GameId))
                    .SelectMany(s =>
                    {
                        var game = games[s.GameId];
                        var genres = game.Genres?.AsEnumerable() ?? Enumerable.Empty<string>();
                        return genres.Select(genre => new
                        {
                            Genre = genre,
                            Followers = s.FollowersCount,
                            GameId = game.Id
                        });
                    })
                    .ToList();

                var stats = joined
                    .GroupBy(x => x.Genre)
                    .Select(grp => new
                    {
                        Genre = grp.Key,
                        Games = grp.Select(x => x.GameId).Distinct().Count(),
                        AvgFollowers = (long)Math.Round(grp.Average(x => (double)x.Followers))
                    })
                    .OrderByDescending(x => x.Games)
                    .ThenBy(x => x.Genre)
                    .Take(5)
                    .ToList();

                monthGenreStats[monthStr] = stats
                    .Select(x => (x.Genre, x.AvgFollowers, x.Games))
                    .ToList();
            }

            var allGenres = monthGenreStats.Values
                .SelectMany(v => v.Select(x => x.Genre))
                .Distinct()
                .ToList();

            var response = new List<GenreTrendDto>();
            foreach (var genre in allGenres)
            {
                var trend = new GenreTrendDto { Genre = genre };

                foreach (var (monthStr, stats) in monthGenreStats)
                {
                    var orderedStats = stats
                        .OrderByDescending(x => x.Games)
                        .ThenBy(x => x.Genre)
                        .ToList();

                    var match = orderedStats
                        .Select((x, index) => new { x.Genre, Rank = index + 1, x.AvgFollowers })
                        .FirstOrDefault(x => x.Genre == genre);

                    trend.Data.Add(new GenreTrendPointDto
                    {
                        Month = monthStr,
                        AvgFollowers = match?.AvgFollowers,
                        Rank = match?.Rank
                    });
                }

                trend.Data = trend.Data.OrderBy(x => x.Month).ToList();

                response.Add(trend);
            }

            return Ok(response);
        }

        [HttpGet("genres-pie")]
        public async Task<ActionResult<IEnumerable<GenrePieChartDto>>> GetGenresPieChart(
            [FromQuery] string month,
            [FromQuery] string? platform,
            CancellationToken ct = default)
        {
            if (!DateTime.TryParse($"{month}-01", out var monthDate))
                return BadRequest("Invalid month format. Expected yyyy-MM");

            var start = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var gamesForMonth = await _db.Games.AsNoTracking()
                .Where(g => g.ReleaseDate != null && g.ReleaseDate.Value >= start && g.ReleaseDate.Value < end)
                .ToListAsync(ct);

            if (!string.IsNullOrWhiteSpace(platform))
            {
                var lower = platform.Trim();
                gamesForMonth = gamesForMonth
                    .Where(g => (g.Platforms?.AsEnumerable() ?? Enumerable.Empty<string>())
                        .Any(p => string.Equals(p, lower, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            var genreStats = gamesForMonth
                .SelectMany(g => (g.Genres?.AsEnumerable() ?? Enumerable.Empty<string>()))
                .GroupBy(genre => genre)
                .Select(grp => new GenrePieChartDto
                {
                    Genre = grp.Key,
                    Count = grp.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var total = genreStats.Sum(x => x.Count);
            if (total > 0)
            {
                foreach (var stat in genreStats)
                {
                    stat.Percentage = Math.Round((double)stat.Count / total * 100, 2);
                }
            }

            return Ok(genreStats);
        }

        private static List<DateTime> BuildMonthList(int months, bool includeCurrentMonth)
        {
            var now = DateTime.UtcNow;
            var baseMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var list = new List<DateTime>();

            if (includeCurrentMonth)
            {
                for (int i = 0; i < months; i++)
                    list.Add(baseMonth.AddMonths(-i));
            }
            else
            {
                for (int i = 1; i <= months; i++)
                    list.Add(baseMonth.AddMonths(-i));
            }

            list.Reverse();
            return list;
        }
    }
}

