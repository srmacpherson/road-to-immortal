using Microsoft.EntityFrameworkCore;
using RoadToImmortal.Api.Data;
using RoadToImmortal.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
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
