using Microsoft.EntityFrameworkCore;
using RoadToImmortal.Api.Data;
using RoadToImmortal.Api.Models;
using RoadToImmortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<SteamService>();
builder.Services.AddHttpClient<DotaService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

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

app.MapGet("/players/{steamId}/dashboard", async (long steamId, IDashboardService dashboardService) =>
{
    var dashboard = await dashboardService.GetDashboardAsync(steamId);

    if (dashboard is null)
    {
        return Results.NotFound(new { Message = "No matches found for this player." });
    }

    return Results.Ok(dashboard);
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
        .OrderByDescending(m => m.MatchDate)
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

app.MapGet("/players/{steamId}/heroes", async (long steamId, AppDbContext db) =>
{
    var heroStats = await db.Matches
        .Where(m => m.SteamId == steamId)
        .Join(
            db.Heroes,
            match => match.HeroId,
            hero => hero.HeroId,
            (match, hero) => new
            {
                match,
                hero
            })
        .GroupBy(x => new
        {
            x.hero.HeroId,
            x.hero.LocalizedName
        })
        .Select(group => new
        {
            HeroId = group.Key.HeroId,
            HeroName = group.Key.LocalizedName,

            Games = group.Count(),

            Wins = group.Count(x =>
                (x.match.PlayerSlot < 128 && x.match.RadiantWin) ||
                (x.match.PlayerSlot >= 128 && !x.match.RadiantWin)),

            AverageKills = Math.Round(
                group.Average(x => x.match.Kills), 2),

            AverageDeaths = Math.Round(
                group.Average(x => x.match.Deaths), 2),

            AverageAssists = Math.Round(
                group.Average(x => x.match.Assists), 2)
        })
        .OrderByDescending(h => h.Games)
        .ToListAsync();

    var result = heroStats.Select(h => new
    {
        h.HeroId,
        h.HeroName,
        h.Games,
        h.Wins,
        Losses = h.Games - h.Wins,
        WinRate = Math.Round(
            (double)h.Wins / h.Games * 100, 2),
        h.AverageKills,
        h.AverageDeaths,
        h.AverageAssists
    });

    return Results.Ok(result);
});

app.MapGet("/players/{steamId}/recent-form", async (long steamId, AppDbContext db) =>
{
    const int recentGames = 10;

    var matches = await db.Matches
        .Where(m => m.SteamId == steamId)
        .OrderByDescending(m => m.MatchDate)
        .Take(recentGames)
        .ToListAsync();

    if (matches.Count == 0)
    {
        return Results.NotFound(new
        {
            Message = "No matches found for this player."
        });
    }

    var results = matches.Select(m =>
    {
        var won =
            (m.PlayerSlot < 128 && m.RadiantWin) ||
            (m.PlayerSlot >= 128 && !m.RadiantWin);

        return new
        {
            m.MatchId,
            m.MatchDate,
            Result = won ? "W" : "L",
            m.HeroId,
            m.Kills,
            m.Deaths,
            m.Assists,
            m.Duration
        };
    }).ToList();

    var wins = results.Count(r => r.Result == "W");
    var losses = results.Count(r => r.Result == "L");

    return Results.Ok(new
    {
        Games = results.Count,
        Wins = wins,
        Losses = losses,
        WinRate = Math.Round(
            (double)wins / results.Count * 100, 2),
        Results = results
    });
});

app.MapGet("/players/{steamId}/mmr", async (long steamId, AppDbContext db) =>
{
    var snapshots = await db.MmrSnapshots
        .Where(m => m.SteamId == steamId)
        .OrderBy(m => m.RecordedAt)
        .Select(m => new
        {
            m.Id,
            m.Mmr,
            m.RecordedAt
        })
        .ToListAsync();

    if (snapshots.Count == 0)
    {
        return Results.NotFound(new
        {
            Message = "No MMR history found for this player."
        });
    }

    var startingMmr = snapshots.First().Mmr;
    var currentMmr = snapshots.Last().Mmr;
    var highestMmr = snapshots.Max(m => m.Mmr);

    var progression = new
    {
        SteamId = steamId,

        StartingMmr = startingMmr,
        CurrentMmr = currentMmr,
        HighestMmr = highestMmr,

        MmrGained = currentMmr - startingMmr,

        Snapshots = snapshots
    };

    return Results.Ok(progression);
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

app.MapPost("/players/{steamId}/matches/sync", async (long steamId, int mmr, 
    DotaService dotaService, AppDbContext db) =>
{
    await dotaService.SyncRecentMatchesAsync(steamId);

    var latestSnapshot = await db.MmrSnapshots
        .Where(m => m.SteamId == steamId)
        .OrderByDescending(m => m.RecordedAt)
        .FirstOrDefaultAsync();

    if (latestSnapshot == null || latestSnapshot.Mmr != mmr)
    {
        var snapshot = new MmrSnapshot
        {
            SteamId = steamId,
            Mmr = mmr,
            RecordedAt = DateTime.UtcNow
        };

        db.MmrSnapshots.Add(snapshot);
        await db.SaveChangesAsync();
    }

    return Results.Ok(new
    {
        SteamId = steamId,
        Mmr = mmr,
        Message = "Matches and MMR synced successfully."
    });
});

app.MapPost("/heroes/sync", async (DotaService dotaService) =>
{
    await dotaService.SyncHeroesAsync();

    return Results.Ok(new
    {
        Message = "Heroes synced successfully."
    });
});

app.MapPost("/players/{steamId}/mmr", async (long steamId, CreateMmrSnapshotRequest request,
    AppDbContext db) =>
{
    var snapshot = new MmrSnapshot
    {
        SteamId = steamId,
        Mmr = request.Mmr,
        RecordedAt = DateTime.UtcNow
    };

    db.MmrSnapshots.Add(snapshot);

    await db.SaveChangesAsync();

    return Results.Created(
        $"/players/{steamId}/mmr/{snapshot.Id}",
        snapshot);
});

#endregion

app.Run();

record CreatePlayerRequest(
    long SteamId,
    string PersonaName,
    int? CurrentMmr
);

record CreateMmrSnapshotRequest(int Mmr);