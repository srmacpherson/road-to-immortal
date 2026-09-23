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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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

app.MapGet("/players/{steamId}/matches", async (long steamId, DotaService dotaService) =>
{
    var matches = await dotaService.GetRecentMatchesAsync(steamId);

    return Results.Ok(matches);
});

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

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

app.Run();

record CreatePlayerRequest(
    long SteamId,
    string PersonaName,
    int? CurrentMmr
);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
