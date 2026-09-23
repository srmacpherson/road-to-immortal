using Microsoft.EntityFrameworkCore;
using RoadToImmortal.Api.Data;
using RoadToImmortal.Api.Models;
using RoadToImmortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<SteamService>();
builder.Services.AddHttpClient<DotaService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

# region GET

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Application = "Road to Immortal",
        Status = "Running",
        Version = "0.1"
    });
});

app.MapGet("/players/{steamId}/steam", async (long steamId, SteamService steamService) =>
{
    var playerName = await steamService.GetPlayerNameAsync(steamId);

    if (playerName is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new
    {
        SteamId = steamId,
        PersonaName = playerName
    });
});

// get from OpenDota via DotaService
//app.MapGet("/players/{steamId}/matches", async (long steamId, DotaService dotaService) =>
//{
//    var matches = await dotaService.GetRecentMatchesAsync(steamId);

//    return Results.Ok(matches);
//});

// get from database via AppDbContext
app.MapGet("/players/{steamId}/matches", async (long steamId, AppDbContext db) =>
{
    var matches = await db.Matches
        .Where(m => m.SteamId == steamId)
        .OrderByDescending(m => m.FetchedAt)
        .ToListAsync();

    return Results.Ok(matches);
});

app.MapGet("/players/{steamId}/summary", async (long steamId, AppDbContext db) =>
{
    var matches = await db.Matches
        .Where(m => m.SteamId == steamId)
        .ToListAsync();

    if (matches.Count == 0)
    {
        return Results.NotFound(new
        {
            Message = "No matches found for this player."
        });
    }

    var wins = matches.Count(m =>
        (m.PlayerSlot < 128 && m.RadiantWin) ||
        (m.PlayerSlot >= 128 && !m.RadiantWin));

    var losses = matches.Count - wins;

    var summary = new
    {
        SteamId = steamId,
        MatchesPlayed = matches.Count,
        Wins = wins,
        Losses = losses,
        WinRate = Math.Round((double)wins / matches.Count * 100, 2),
        AverageKills = Math.Round(matches.Average(m => m.Kills), 2),
        AverageDeaths = Math.Round(matches.Average(m => m.Deaths), 2),
        AverageAssists = Math.Round(matches.Average(m => m.Assists), 2),
        AverageDurationSeconds = Math.Round(matches.Average(m => m.Duration), 2)
    };

    return Results.Ok(summary);
});

#endregion

#region POST

app.MapPost("/players", async (CreatePlayerRequest request, AppDbContext db) =>
{
    var player = new Player
    {
        SteamId = request.SteamId,
        PersonaName = request.PersonaName,
        CurrentMmr = request.CurrentMmr,
        LastUpdated = DateTime.UtcNow
    };

    db.Players.Add(player);
    await db.SaveChangesAsync();

    return Results.Created($"/players/{player.SteamId}", player);
});

app.MapPost("/players/{steamId}/matches/sync", async (long steamId, DotaService dotaService) =>
{
    await dotaService.SyncRecentMatchesAsync(steamId);

    return Results.Ok(new
    {
        SteamId = steamId,
        Message = "Recent matches synced successfully."
    });
});

#endregion

app.Run();

record CreatePlayerRequest(
    long SteamId,
    string PersonaName,
    int? CurrentMmr
);
