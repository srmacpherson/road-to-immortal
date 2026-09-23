using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RoadToImmortal.Api.Data;
using RoadToImmortal.Api.Models;

namespace RoadToImmortal.Api.Services;

public class DotaService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _db;

    public DotaService(HttpClient httpClient, AppDbContext db)
    {
        _httpClient = httpClient;
        _db = db;
    }

    public async Task<List<DotaMatch>> GetRecentMatchesAsync(long steamId)
    {
        var accountId = steamId - 76561197960265728;

        var url =
            $"https://api.opendota.com/api/players/{accountId}/matches?limit=20";

        return await _httpClient.GetFromJsonAsync<List<DotaMatch>>(url)
               ?? new List<DotaMatch>();
    }

    public async Task SyncRecentMatchesAsync(long steamId)
    {
        var accountId = steamId - 76561197960265728;

        var url =
            $"https://api.opendota.com/api/players/{accountId}/matches?limit=20";

        var matches = await _httpClient.GetFromJsonAsync<List<DotaMatch>>(url)
                      ?? new List<DotaMatch>();

        foreach (var match in matches)
        {
            var existingMatch = await _db.Matches.FindAsync(match.MatchId);

            // Idempotency guard
            if (existingMatch != null)
            {
                continue;
            }

            var databaseMatch = new Match
            {
                MatchId = match.MatchId,
                SteamId = steamId,
                PlayerSlot = match.PlayerSlot,
                RadiantWin = match.RadiantWin,
                Duration = match.Duration,
                GameMode = match.GameMode,
                HeroId = match.HeroId,
                Kills = match.Kills,
                Deaths = match.Deaths,
                Assists = match.Assists
            };

            _db.Matches.Add(databaseMatch);
        }

        await _db.SaveChangesAsync();
    }
}

public class DotaMatch
{
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }

    [JsonPropertyName("player_slot")]
    public int PlayerSlot { get; set; }

    [JsonPropertyName("radiant_win")]
    public bool RadiantWin { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("game_mode")]
    public int GameMode { get; set; }

    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }

    [JsonPropertyName("kills")]
    public int Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }

    [JsonPropertyName("assists")]
    public int Assists { get; set; }
}