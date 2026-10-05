using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoadToImmortal.Api.Data;
using RoadToImmortal.Api.Models;
using RoadToImmortal.Api.Services;
using Xunit;

namespace RoadToImmortal.Api.Tests;

public class DashboardServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetDashboardAsync_NoMatches_ReturnsNull()
    {
        using var db = CreateContext();

        var svc = new DashboardService(db);

        var result = await svc.GetDashboardAsync(12345);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDashboardAsync_ComputesOverallStats()
    {
        using var db = CreateContext();

        // seed hero
        db.Heroes.Add(new Hero { HeroId = 1, LocalizedName = "Example" });

        // seed matches: 2 wins, 1 loss
        db.Matches.Add(new Match { MatchId = 1, SteamId = 111, PlayerSlot = 0, RadiantWin = true, HeroId = 1, Kills = 5, Deaths = 2, Assists = 3, Duration = 1200, MatchDate = DateTime.UtcNow });
        db.Matches.Add(new Match { MatchId = 2, SteamId = 111, PlayerSlot = 0, RadiantWin = true, HeroId = 1, Kills = 2, Deaths = 4, Assists = 1, Duration = 900, MatchDate = DateTime.UtcNow.AddMinutes(-30) });
        db.Matches.Add(new Match { MatchId = 3, SteamId = 111, PlayerSlot = 128, RadiantWin = true, HeroId = 1, Kills = 1, Deaths = 5, Assists = 0, Duration = 800, MatchDate = DateTime.UtcNow.AddHours(-1) });

        // seed mmr
        db.MmrSnapshots.Add(new MmrSnapshot { SteamId = 111, Mmr = 3000, RecordedAt = DateTime.UtcNow.AddDays(-7) });
        db.MmrSnapshots.Add(new MmrSnapshot { SteamId = 111, Mmr = 3100, RecordedAt = DateTime.UtcNow });

        db.SaveChanges();

        var svc = new DashboardService(db);

        var dashboard = await svc.GetDashboardAsync(111);

        Assert.NotNull(dashboard);
        Assert.Equal(3, dashboard!.Overall.Games);
        Assert.Equal(2, dashboard.Overall.Wins);
        Assert.Equal(1, dashboard.Overall.Losses);
        Assert.Equal(2, dashboard.Mmr.History.Count);
        // Basic checks for MMR history and values
    }
}
