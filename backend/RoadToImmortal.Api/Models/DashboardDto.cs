using System;
using System.Collections.Generic;

namespace RoadToImmortal.Api.Models;

public record DashboardDto(
    long SteamId,
    MmrDto Mmr,
    OverallDto Overall,
    List<HeroPerformanceDto> Heroes,
    RecentFormDto RecentForm
);

public record MmrDto(
    int? Starting,
    int? Current,
    int? Highest,
    int Gained,
    List<MmrSnapshotDto> History,
    int? ConfirmedMmr,
    int? PredictedMmr,
    int? PredictionDelta,
    bool PredictionNeedsConfirmation
);

public record MmrSnapshotDto(int Mmr, DateTime RecordedAt);

public record OverallDto(
    int Games,
    int Wins,
    int Losses,
    double WinRate,
    double AverageKills,
    double AverageDeaths,
    double AverageAssists
);

public record HeroPerformanceDto(
    int HeroId,
    string HeroName,
    int Games,
    int Wins,
    int Losses,
    double WinRate,
    double AverageKills,
    double AverageDeaths,
    double AverageAssists
);

public record RecentFormDto(
    int Games,
    int Wins,
    int Losses,
    double WinRate,
    List<RecentMatchDto> Matches
);

public record RecentMatchDto(
    long MatchId,
    string Result,
    int HeroId,
    string HeroName,
    int Kills,
    int Deaths,
    int Assists,
    int Duration,
    DateTime MatchDate
);
