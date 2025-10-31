// Services/SteamSyncBackgroundService.cs
using Microsoft.Extensions.Hosting;

namespace SteamService.Services;

public class SteamSyncBackgroundService : BackgroundService
{
    private readonly ILogger<SteamSyncBackgroundService> _logger;
    private readonly IServiceProvider _sp;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6); // ������: ������ 6 �����

    public SteamSyncBackgroundService(IServiceProvider sp, ILogger<SteamSyncBackgroundService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SteamSyncBackgroundService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var steam = scope.ServiceProvider.GetRequiredService<ISteamService>();
                await steam.FetchAndSaveGamesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background sync failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
