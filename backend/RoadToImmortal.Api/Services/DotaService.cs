using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RoadToImmortal.Api.Services;

public class DotaService
{
    private readonly HttpClient _httpClient;

    public DotaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DotaMatch>> GetRecentMatchesAsync(long steamId)
    {
        var accountId = steamId - 76561197960265728;

        var url =
            $"https://api.opendota.com/api/players/{accountId}/matches?limit=20";

        return await _httpClient.GetFromJsonAsync<List<DotaMatch>>(url)
               ?? new List<DotaMatch>();
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