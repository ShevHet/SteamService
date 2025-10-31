using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using SteamService.Data;
using SteamService.Models;

namespace SteamService.Services;

public class SteamOptions
{
	public string BaseUrl { get; set; } = "https://store.steampowered.com";
	public int RequestDelayMs { get; set; } = 1000;
	public int MaxRequestsPerSecond { get; set; } = 1;
	public PollyOptions Polly { get; set; } = new PollyOptions();
}
public class PollyOptions
{
	public int RetryCount { get; set; } = 3;
	public int CircuitBreakerFailures { get; set; } = 5;
	public int CircuitBreakerDurationSec { get; set; } = 30;
}

public class SteamDataService : ISteamService
{
	private readonly IHttpClientFactory _httpFactory;
	private readonly ApplicationDbContext _db;
	private readonly ILogger<SteamDataService> _logger;
	private readonly SteamOptions _opts;
	private readonly SemaphoreSlim _rateLock = new(1, 1);
	private DateTime _lastRequest = DateTime.MinValue;

	public SteamDataService(IHttpClientFactory httpFactory,
						ApplicationDbContext db,
						IOptions<SteamOptions> opts,
						ILogger<SteamDataService> logger)
	{
		_httpFactory = httpFactory;
		_db = db;
		_logger = logger;
		_opts = opts.Value;
	}

	public async Task FetchAndSaveGamesAsync(CancellationToken ct = default)
	{
		// ������: �������� ������ appids � � production ����� ����-�� ����� ������ (coming soon / API)
		// ��� ������� ������ static ������ ��� ����� � "https://store.steampowered.com/search/" ...
		// ����� � ����������� ������: ���� id

		var appIds = new long[] { 1675200 }; // ������ (������ �� �������� ������ ������)
		int processed = 0, success = 0, failed = 0;

		foreach (var id in appIds)
		{
			if (ct.IsCancellationRequested) break;
			processed++;
			try
			{
				await FetchGameDetailsAsync(id, ct);
				success++;
			}
			catch (Exception ex)
			{
				failed++;
				_logger.LogError(ex, "Error processing {AppId}", id);
			}
		}

		_logger.LogInformation("FetchAndSaveGames finished. Processed: {0}, Success: {1}, Failed: {2}",
			processed, success, failed);
	}

	public async Task FetchGameDetailsAsync(long steamAppId, CancellationToken ct = default)
	{
		// 1) robots.txt polite check
		if (!await IsAllowedByRobotsAsync(ct))
		{
			_logger.LogWarning("Robots.txt disallows scraping; aborting fetch for {AppId}", steamAppId);
			return;
		}

		// 2) call store api first (public store API)
		var client = _httpFactory.CreateClient("steam");
		string storeApiUrl = $"{_opts.BaseUrl}/api/appdetails?appids={steamAppId}&cc=us&l=en";

		HttpResponseMessage resp = await SendWithRateLimitAndPoliciesAsync(() => client.GetAsync(storeApiUrl, ct), ct);

		string raw = string.Empty;
		if (resp.IsSuccessStatusCode)
		{
			raw = await resp.Content.ReadAsStringAsync(ct);
			// parse JSON
			try
			{
				// Store API returns { "APPID": { success: true, data: { ... } } }
				using var doc = JsonDocument.Parse(raw);
				var root = doc.RootElement;
				if (root.TryGetProperty(steamAppId.ToString(), out var appEl) &&
					appEl.TryGetProperty("success", out var successEl) &&
					successEl.GetBoolean())
				{
					var data = appEl.GetProperty("data");

					// �������� ������������ ���� (��������, ��������, header image, release date)
					var name = data.GetProperty("name").GetString() ?? "";
					var desc = data.TryGetProperty("short_description", out var sd) ? sd.GetString() ?? "" : "";
					var header = data.TryGetProperty("header_image", out var hi) ? hi.GetString() ?? "" : "";
					DateOnly? releaseDate = null;
					if (data.TryGetProperty("release_date", out var rd) && rd.TryGetProperty("date", out var date))
					{
						// Parse simple formats - production: use robust parser
						var dateStr = date.GetString();
						if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var dt))
							releaseDate = DateOnly.FromDateTime(dt);
					}

					// upsert game
					var game = await _db.Games.FirstOrDefaultAsync(g => g.SteamAppId == steamAppId, ct);
					if (game == null)
					{
						game = new Game
						{
							SteamAppId = steamAppId,
							Name = name,
							ShortDescription = desc,
							HeaderImageUrl = header,
							StoreUrl = $"{_opts.BaseUrl}/app/{steamAppId}"
						};
						_db.Games.Add(game);
						await _db.SaveChangesAsync(ct); // so we have Game.Id
					}
					else
					{
						game.Name = name;
						game.ShortDescription = desc;
						game.HeaderImageUrl = header;
						game.StoreUrl = $"{_opts.BaseUrl}/app/{steamAppId}";
						_db.Games.Update(game);
						await _db.SaveChangesAsync(ct);
					}

					// Create/update snapshot for today (UTC date)
					await UpsertSnapshotAsync(game.Id, raw, 0 /*followers unknown*/, ct);
					return;
				}
			}
			catch (JsonException je)
			{
				_logger.LogWarning(je, "JSON parse failed for app {AppId}, falling back to HTML", steamAppId);
			}
		}

		// 3) Fallback: parse store page HTML for some fields (e.g., followers)
		// URL страницы магазина: https://store.steampowered.com/app/{appId}/
		var pageUrl = $"{_opts.BaseUrl}/app/{steamAppId}/";
		resp = await SendWithRateLimitAndPoliciesAsync(() => client.GetAsync(pageUrl, ct), ct);
		if (!resp.IsSuccessStatusCode)
		{
			_logger.LogWarning("Failed to fetch HTML page for {AppId} with status {Status}", steamAppId, resp.StatusCode);
			throw new HttpRequestException($"Failed to fetch store page for {steamAppId}: {resp.StatusCode}");
		}

		raw = await resp.Content.ReadAsStringAsync(ct);

		// HtmlAgilityPack parse example � ���� followers count (������, �������� �������� ���� ���������)
		var docHtml = new HtmlDocument();
		docHtml.LoadHtml(raw);
		long followers = 0;
		try
		{
			// ������ �������� ������: ���� ���������� 'Followers' � production: ������ xpath
			var node = docHtml.DocumentNode.SelectSingleNode("//div[contains(@class,'user_reviews')]");
			// ... � ����������� �� �������� �������� �������
			// ��� ������� ���������� ������ ������� � ��������� followers=0
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "HTML parse error for {AppId}", steamAppId);
		}

		// In HTML fallback also try to extract name/header/description similar to JSON approach
		// Minimal upsert using just raw HTML
		var existing = await _db.Games.FirstOrDefaultAsync(g => g.SteamAppId == steamAppId, ct);
		if (existing == null)
		{
			var g = new Game
			{
				SteamAppId = steamAppId,
				Name = $"App {steamAppId}",
				ShortDescription = "",
				HeaderImageUrl = "",
				StoreUrl = pageUrl
			};
			_db.Games.Add(g);
			await _db.SaveChangesAsync(ct);
			await UpsertSnapshotAsync(g.Id, raw, followers, ct);
		}
		else
		{
			existing.StoreUrl = pageUrl;
			_db.Games.Update(existing);
			await _db.SaveChangesAsync(ct);
			await UpsertSnapshotAsync(existing.Id, raw, followers, ct);
		}
	}

	private async Task UpsertSnapshotAsync(Guid gameId, string rawData, long followers, CancellationToken ct)
	{
		var today = DateTime.UtcNow.Date;
		var existingSnapshot = await _db.GameSnapshots
			.Where(s => s.GameId == gameId && s.SnapshotDateUtc.Date == today)
			.FirstOrDefaultAsync(ct);

		if (existingSnapshot != null)
		{
			existingSnapshot.FollowersCount = followers;
			existingSnapshot.RawData = rawData;
			_db.GameSnapshots.Update(existingSnapshot);
		}
		else
		{
			var snapshot = new GameSnapshot
			{
				GameId = gameId,
				FollowersCount = followers,
				SnapshotDateUtc = DateTime.UtcNow,
				RawData = rawData
			};
			_db.GameSnapshots.Add(snapshot);
		}

		await _db.SaveChangesAsync(ct);
	}

	private async Task<bool> IsAllowedByRobotsAsync(CancellationToken ct)
	{
		try
		{
			var client = _httpFactory.CreateClient("steam");
			var url = $"{_opts.BaseUrl}/robots.txt";
			var resp = await SendWithRateLimitAndPoliciesAsync(() => client.GetAsync(url, ct), ct);
			if (!resp.IsSuccessStatusCode) return true; // can't fetch robots -> optimistic allow

			var txt = await resp.Content.ReadAsStringAsync(ct);
			// ����� ������� ��������: ���� ���� ������ "Disallow: /" � ���������
			if (txt.Contains("Disallow: /"))
				return false;
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "robots.txt check failed - allowing by default");
			return true;
		}
	}

	// Rate-limit + send via policies: ensures request spacing and relies on HttpClient configured with Polly
	private async Task<HttpResponseMessage> SendWithRateLimitAndPoliciesAsync(Func<Task<HttpResponseMessage>> send, CancellationToken ct)
	{
		// Simple spacing rate-limit (RequestDelayMs)
		await _rateLock.WaitAsync(ct);
		try
		{
			var delta = DateTime.UtcNow - _lastRequest;
			var desired = TimeSpan.FromMilliseconds(_opts.RequestDelayMs);
			if (delta < desired)
			{
				var wait = desired - delta;
				await Task.Delay(wait, ct);
			}

			_lastRequest = DateTime.UtcNow;
		}
		finally
		{
			_rateLock.Release();
		}

		// Execute the request (HttpClient has Polly policies via DI registration)
		var response = await send();
		return response;
	}
}
