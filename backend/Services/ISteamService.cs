using System.Threading;
using System.Threading.Tasks;

namespace SteamService.Services;

public interface ISteamService
{
    Task FetchAndSaveGamesAsync(CancellationToken ct = default);
    Task FetchGameDetailsAsync(long steamAppId, CancellationToken ct = default);
}