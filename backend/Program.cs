using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SteamService.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly;
using SteamService.Services;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Steam Releases Aggregator API",
        Version = "v1",
        Description = "Backend service for aggregating and analyzing Steam game releases",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@example.com"
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dataSource, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:4200",
                "https://yourfrontend.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Length", "Content-Range");
    });
});

builder.Services.Configure<SteamOptions>(builder.Configuration.GetSection("Steam"));

IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount) =>
    Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .OrResult(msg => (int)msg.StatusCode == 429)
        .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int failures, int durationSec) =>
    Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(failures, TimeSpan.FromSeconds(durationSec));

var pollyCfg = builder.Configuration.GetSection("Steam:Polly").Get<PollyOptions>() ?? new PollyOptions();

builder.Services.AddHttpClient("steam", client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("Steam:BaseUrl") ?? "https://store.steampowered.com";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SteamService/1.0 (+https://example.com)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy(pollyCfg.RetryCount))
.AddPolicyHandler(GetCircuitBreakerPolicy(pollyCfg.CircuitBreakerFailures, pollyCfg.CircuitBreakerDurationSec));

builder.Services.AddScoped<ISteamService, SteamService.Services.SteamDataService>();
builder.Services.AddHostedService<SteamSyncBackgroundService>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

if (args.Contains("--seed"))
{
    var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DataSeederRunner");

    await DataSeederRunner.RunAsync(scopeFactory, logger);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Steam Aggregator API v1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRouting();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying pending migrations...");
            context.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var seederLogger = loggerFactory.CreateLogger("DataSeeder");
        await DataSeeder.SeedAsync(context, seederLogger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");

        if (app.Environment.IsProduction())
            throw;
    }
}

app.MapControllers();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An internal server error occurred",
            requestId = context.TraceIdentifier
        });
    }
});

Console.WriteLine($"Application starting in {app.Environment.EnvironmentName} environment");
Console.WriteLine($"Database: {builder.Configuration.GetConnectionString("DefaultConnection")}");

app.Run();

public partial class Program { }
