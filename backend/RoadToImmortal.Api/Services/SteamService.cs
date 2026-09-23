using System.Net.Http.Json;

namespace RoadToImmortal.Api.Services;

public class SteamService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SteamService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string?> GetPlayerNameAsync(long steamId)
    {
        var apiKey = _configuration["Steam:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Steam API key is not configured.");
        }

        var url =
            $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/" +
            $"?key={apiKey}&steamids={steamId}";

        var response = await _httpClient.GetFromJsonAsync<SteamPlayerResponse>(url);

        return response?.Response?.Players?.FirstOrDefault()?.PersonaName;
    }

    private class SteamPlayerResponse
    {
        public SteamResponse? Response { get; set; }
    }

    private class SteamResponse
    {
        public List<SteamPlayer>? Players { get; set; }
    }

    private class SteamPlayer
    {
        public string? PersonaName { get; set; }
    }
}