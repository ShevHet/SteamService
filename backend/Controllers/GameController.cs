using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamService.Data;
using SteamService.DTOs;
using SteamService.Models;

namespace SteamService.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<GamesController> _logger;

        public GamesController(ApplicationDbContext db, ILogger<GamesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGamesByMonth(
            [FromQuery] string month,
            [FromQuery] string? platform,
            [FromQuery] string? genre,
            CancellationToken ct)
        {
            if (!DateTime.TryParse($"{month}-01", out var monthDate))
                return BadRequest("Invalid month format. Expected yyyy-MM");

            var start = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            IQueryable<Game> query = _db.Games.AsNoTracking()
                .Where(g => g.ReleaseDate != null &&
                            g.ReleaseDate.Value >= start &&
                            g.ReleaseDate.Value < end);

            var gameEntities = await query
                .OrderBy(g => g.ReleaseDate)
                .ToListAsync(ct);

            if (!string.IsNullOrWhiteSpace(platform))
            {
                var lower = platform.ToLower();
                gameEntities = gameEntities
                    .Where(g => g.Platforms != null && 
                               g.Platforms.Any(p => p != null && p.ToLower() == lower))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                var lower = genre.ToLower();
                gameEntities = gameEntities
                    .Where(g => g.Genres != null && 
                               g.Genres.Any(p => p != null && p.ToLower() == lower))
                    .ToList();
            }

            var games = gameEntities.Select(g => new GameDto
            {
                Id = g.Id,
                SteamAppId = g.SteamAppId,
                Name = g.Name,
                Slug = g.Slug,
                ReleaseDate = g.ReleaseDate,
                StoreUrl = g.StoreUrl,
                HeaderImageUrl = g.HeaderImageUrl,
                Genres = g.Genres ?? new List<string>(),
                Platforms = g.Platforms ?? new List<string>(),
                ShortDescription = g.ShortDescription,
                Price = g.Price,
                MetacriticScore = g.MetacriticScore,
                IsComingSoon = g.IsComingSoon,
                CreatedAt = g.CreatedAt
            }).ToList();

            return Ok(games);
        }

        [HttpGet("calendar")]
        public async Task<ActionResult<CalendarMonthDto>> GetCalendarByMonth(
            [FromQuery] string month,
            [FromQuery] string? platform,
            [FromQuery] string? genre,
            CancellationToken ct)
        {
            if (!DateTime.TryParse($"{month}-01", out var monthDate))
                return BadRequest("Invalid month format. Expected yyyy-MM");

            var start = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            IQueryable<Game> query = _db.Games.AsNoTracking()
                .Where(g => g.ReleaseDate != null &&
                            g.ReleaseDate.Value >= start &&
                            g.ReleaseDate.Value < end);

            var games = await query.ToListAsync(ct);

            if (!string.IsNullOrWhiteSpace(platform))
            {
                var lower = platform.ToLower();
                games = games
                    .Where(g => g.Platforms != null && 
                               g.Platforms.Any(p => p != null && p.ToLower() == lower))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                var lower = genre.ToLower();
                games = games
                    .Where(g => g.Genres != null && 
                               g.Genres.Any(p => p != null && p.ToLower() == lower))
                    .ToList();
            }

            var grouped = games
                .Where(g => g.ReleaseDate.HasValue)
                .GroupBy(g => g.ReleaseDate!.Value.Date)
                .Select(g => new CalendarDayDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Date)
                .ToList();

            var result = new CalendarMonthDto
            {
                Month = month,
                Days = grouped
            };

            return Ok(result);
        }
    }
}
