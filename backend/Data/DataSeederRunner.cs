using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SteamService.Data;

public static class DataSeederRunner
{
    public static async Task RunAsync(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var factory = services.GetRequiredService<ILoggerFactory>();
        var seederLogger = factory.CreateLogger("DataSeeder");

        await DataSeeder.SeedAsync(dbContext, seederLogger);
        logger.LogInformation("Data seeding completed successfully.");
    }
}
