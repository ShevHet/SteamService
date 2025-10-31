using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SteamService.Data;

namespace SteamService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext db, ILogger<HealthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetHealth()
    {
        try
        {
            var dbConnected = await _db.Database.CanConnectAsync();
            var gamesCount = await _db.Games.CountAsync();

            return Ok(new
            {
                Status = "Healthy",
                Database = dbConnected ? "Connected" : "Disconnected",
                Timestamp = DateTime.UtcNow,
                Uptime = Environment.TickCount64,
                Metrics = new
                {
                    TotalGames = gamesCount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}