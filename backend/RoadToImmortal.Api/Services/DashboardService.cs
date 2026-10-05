using Microsoft.EntityFrameworkCore;
using RoadToImmortal.Api.Data;
using RoadToImmortal.Api.Models;
using Microsoft.Extensions.Configuration;

namespace RoadToImmortal.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public DashboardService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<DashboardDto?> GetDashboardAsync(long steamId)
    {
        var mmrSnapshots = await _db.MmrSnapshots
            .Where(m => m.SteamId == steamId)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();

        // apply ranked-match filtering if configured
        var rankedModes = _config.GetSection("RankedGameModes").Get<int[]>() ?? Array.Empty<int>();

        IQueryable<Match> matchesQuery = _db.Matches.Where(m => m.SteamId == steamId);

        if (rankedModes.Length > 0)
        {
            matchesQuery = matchesQuery.Where(m => rankedModes.Contains(m.GameMode));
        }

        var matches = await matchesQuery
            .OrderByDescending(m => m.MatchDate)
            .ToListAsync();

        if (matches.Count == 0)
        {
            return null;
        }

        var heroes = await _db.Heroes.ToListAsync();

        var wins = matches.Count(m =>
            (m.PlayerSlot < 128 && m.RadiantWin) ||
            (m.PlayerSlot >= 128 && !m.RadiantWin));

        var losses = matches.Count - wins;

        var overall = new OverallDto(
            Games: matches.Count,
            Wins: wins,
            Losses: losses,
            WinRate: Math.Round((double)wins / matches.Count * 100, 2),
            AverageKills: Math.Round(matches.Average(m => m.Kills), 2),
            AverageDeaths: Math.Round(matches.Average(m => m.Deaths), 2),
            AverageAssists: Math.Round(matches.Average(m => m.Assists), 2)
        );

        var matchesForHeroQuery = _db.Matches.Where(m => m.SteamId == steamId);
        if (rankedModes.Length > 0)
        {
            matchesForHeroQuery = matchesForHeroQuery.Where(m => rankedModes.Contains(m.GameMode));
        }

        var heroStats = await matchesForHeroQuery
            .Join(
                _db.Heroes,
                match => match.HeroId,
                hero => hero.HeroId,
                (match, hero) => new { match, hero })
            .GroupBy(x => new { x.hero.HeroId, x.hero.LocalizedName })
            .Select(group => new
            {
                HeroId = group.Key.HeroId,
                HeroName = group.Key.LocalizedName,
                Games = group.Count(),
                Wins = group.Count(x =>
                    (x.match.PlayerSlot < 128 && x.match.RadiantWin) ||
                    (x.match.PlayerSlot >= 128 && !x.match.RadiantWin)),
                AverageKills = Math.Round(group.Average(x => x.match.Kills), 2),
                AverageDeaths = Math.Round(group.Average(x => x.match.Deaths), 2),
                AverageAssists = Math.Round(group.Average(x => x.match.Assists), 2)
            })
            .OrderByDescending(h => h.Games)
            .ToListAsync();

        var heroPerformance = heroStats.Select(h => new HeroPerformanceDto(
            HeroId: h.HeroId,
            HeroName: h.HeroName,
            Games: h.Games,
            Wins: h.Wins,
            Losses: h.Games - h.Wins,
            WinRate: Math.Round((double)h.Wins / h.Games * 100, 2),
            AverageKills: h.AverageKills,
            AverageDeaths: h.AverageDeaths,
            AverageAssists: h.AverageAssists
        )).ToList();

        const int recentGameCount = 10;

        var recentMatches = matches
            .Take(recentGameCount)
            .Select(m =>
            {
                var won = (m.PlayerSlot < 128 && m.RadiantWin) || (m.PlayerSlot >= 128 && !m.RadiantWin);
                return new RecentMatchDto(
                    MatchId: m.MatchId,
                    Result: won ? "W" : "L",
                    HeroId: m.HeroId,
                    HeroName: heroes.FirstOrDefault(h => h.HeroId == m.HeroId)?.LocalizedName ?? "Unknown Hero",
                    Kills: m.Kills,
                    Deaths: m.Deaths,
                    Assists: m.Assists,
                    Duration: m.Duration,
                    MatchDate: m.MatchDate
                );
            })
            .ToList();

        var recentWins = recentMatches.Count(m => m.Result == "W");

        // -------------------------
        // Predicted MMR
        // -------------------------

        // configuration
        var mmrPerGame = _config.GetValue<int?>("MmrPerGame") ?? 25;
        var confirmationThreshold = _config.GetValue<int?>("ConfirmationThreshold") ?? 50;
        // no-op patch: insertion to ensure proper apply ordering

        // find last confirmed snapshot
        var lastConfirmed = await _db.MmrSnapshots
            .Where(m => m.SteamId == steamId && m.IsConfirmed)
            .OrderByDescending(m => m.RecordedAt)
            .FirstOrDefaultAsync();

        int? confirmedMmr = lastConfirmed?.Mmr;
        DateTime anchorTime = DateTime.MinValue;

        if (lastConfirmed != null)
        {
            anchorTime = lastConfirmed.RecordedAt;
        }
        else
        {
            // fall back to player current mmr
            var player = await _db.Players.FindAsync(steamId);
            if (player != null && player.CurrentMmr.HasValue)
            {
                confirmedMmr = player.CurrentMmr;
                anchorTime = player.LastUpdated;
            }
            else if (mmrSnapshots.Count > 0)
            {
                // fall back to latest snapshot
                var last = mmrSnapshots.Last();
                confirmedMmr = last.Mmr;
                anchorTime = last.RecordedAt;
            }
        }

        int? predictedMmr = null;
        int? predictionDelta = null;
        bool needsConfirmation = false;

        if (confirmedMmr.HasValue)
        {
            // determine matches since anchor
            var matchesSinceAnchor = matches
                .Where(m => m.MatchDate > anchorTime)
                .ToList();

            var winsSince = matchesSinceAnchor.Count(m =>
                (m.PlayerSlot < 128 && m.RadiantWin) ||
                (m.PlayerSlot >= 128 && !m.RadiantWin));

            var lossesSince = matchesSinceAnchor.Count - winsSince;

            predictedMmr = confirmedMmr + (winsSince - lossesSince) * mmrPerGame;
            predictionDelta = predictedMmr - confirmedMmr;
            needsConfirmation = Math.Abs(predictionDelta.Value) >= confirmationThreshold;
        }

        var dashboard = new DashboardDto(
            SteamId: steamId,
            Mmr: new MmrDto(
                Starting: mmrSnapshots.FirstOrDefault()?.Mmr,
                Current: mmrSnapshots.LastOrDefault()?.Mmr,
                Highest: mmrSnapshots.Count > 0 ? mmrSnapshots.Max(m => m.Mmr) : (int?)null,
                Gained: mmrSnapshots.Count > 0 ? mmrSnapshots.Last().Mmr - mmrSnapshots.First().Mmr : 0,
                History: mmrSnapshots.Select(m => new MmrSnapshotDto(m.Mmr, m.RecordedAt)).ToList(),
                ConfirmedMmr: confirmedMmr,
                PredictedMmr: predictedMmr,
                PredictionDelta: predictionDelta,
                PredictionNeedsConfirmation: needsConfirmation
            ),
            Overall: overall,
            Heroes: heroPerformance,
            RecentForm: new RecentFormDto(
                Games: recentMatches.Count,
                Wins: recentWins,
                Losses: recentMatches.Count - recentWins,
                WinRate: recentMatches.Count > 0 ? Math.Round((double)recentWins / recentMatches.Count * 100, 2) : 0,
                Matches: recentMatches
            )
        );

        return dashboard;
    }
}
