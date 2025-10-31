// Controllers/SteamController.cs
using Microsoft.AspNetCore.Mvc;
using SteamService.Services;

namespace SteamService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SteamController : ControllerBase
{
    private readonly ISteamService _steam;
    public SteamController(ISteamService steam) => _steam = steam;

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(CancellationToken ct)
    {
        await _steam.FetchAndSaveGamesAsync(ct);
        return Ok(new { status = "started", timestamp = DateTime.UtcNow });
    }
}
