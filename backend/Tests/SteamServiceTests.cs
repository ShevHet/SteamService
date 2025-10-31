// Tests/SteamServiceTests.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class FakeHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses;
    public FakeHandler(IEnumerable<HttpResponseMessage> responses) => _responses = new Queue<HttpResponseMessage>(responses);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_responses.Count > 0)
            return Task.FromResult(_responses.Dequeue());
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

public class SteamServiceTests
{
    [Fact]
    public async Task FetchGameDetails_CreatesGameAndSnapshot()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("testdb1").Options;
        using var db = new ApplicationDbContext(options);

        // Sample Store API JSON (successful)
        var json = @"{ ""1675200"": { ""success"": true, ""data"": { ""name"": ""Test Game"", ""short_description"": ""desc"", ""header_image"": ""http://img"", ""release_date"": { ""date"": ""Jan 1, 2025"" } } } }";
        var responses = new[] { new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) } };
        var handler = new FakeHandler(responses);
        var client = new HttpClient(handler);
        var factory = Mock.Of<IHttpClientFactory>(f => f.CreateClient("steam") == client);

        var opts = Options.Create(new SteamOptions { BaseUrl = "https://store.steampowered.com", RequestDelayMs = 0 });
        var logger = NullLogger<SteamService>.Instance;
        var svc = new SteamService(factory, db, opts, logger);

        // Act
        await svc.FetchGameDetailsAsync(1675200);

        // Assert
        var g = await db.Games.FirstOrDefaultAsync();
        Assert.NotNull(g);
        Assert.Equal(1675200, g.SteamAppId);

        var snaps = await db.GameSnapshots.Where(s => s.GameId == g.Id).ToListAsync();
        Assert.Single(snaps);
    }

    [Fact]
    public async Task FetchGameDetails_RetryOnTransientError()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("testdb2").Options;
        using var db = new ApplicationDbContext(options);

        // first 500, then 200 with valid json
        var bad = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var okJson = @"{ ""1675201"": { ""success"": true, ""data"": { ""name"": ""Retry Game"", ""short_description"": ""desc"" } } }";
        var good = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(okJson) };

        var responses = new List<HttpResponseMessage> { bad, good };
        var handler = new FakeHandler(responses);
        var client = new HttpClient(handler);
        var factory = Mock.Of<IHttpClientFactory>(f => f.CreateClient("steam") == client);

        var opts = Options.Create(new SteamOptions { BaseUrl = "https://store.steampowered.com", RequestDelayMs = 0, Polly = new PollyOptions { RetryCount = 2 } });
        var svc = new SteamService(factory, db, opts, NullLogger<SteamService>.Instance);

        await svc.FetchGameDetailsAsync(1675201);

        var g = await db.Games.FirstOrDefaultAsync();
        Assert.NotNull(g);
        Assert.Equal(1675201, g.SteamAppId);
    }

    [Fact]
    public async Task Snapshot_Deduplication_SameDay()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("testdb3").Options;
        using var db = new ApplicationDbContext(options);

        var json = @"{ ""1675202"": { ""success"": true, ""data"": { ""name"": ""Dup Game"", ""short_description"": ""desc"" } } }";
        var responses = new[] { new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) } };
        var handler = new FakeHandler(responses);
        var client = new HttpClient(handler);
        var factory = Mock.Of<IHttpClientFactory>(f => f.CreateClient("steam") == client);

        var opts = Options.Create(new SteamOptions { BaseUrl = "https://store.steampowered.com", RequestDelayMs = 0 });
        var svc = new SteamService(factory, db, opts, NullLogger<SteamService>.Instance);

        // call twice
        await svc.FetchGameDetailsAsync(1675202);
        await svc.FetchGameDetailsAsync(1675202);

        var game = await db.Games.FirstOrDefaultAsync();
        Assert.NotNull(game);
        var snaps = await db.GameSnapshots.Where(s => s.GameId == game.Id).ToListAsync();
        Assert.Single(snaps); // deduplication: remains 1 snapshot for the same UTC date
    }
}
