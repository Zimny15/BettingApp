using BookmakerApp.Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookmakerApp.Services;

public class StandingsService
{
    private readonly HttpClient _http;
    private readonly ILogger<StandingsService> _logger;

    public StandingsService(HttpClient http, ILogger<StandingsService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<StandingDto>> GetStandingsAsync(int leagueId, int season)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://v3.football.api-sports.io/standings?league={leagueId}&season={season}"),
            Headers = { { "x-apisports-key", "13c7527ad64a43ddb42da93ce94f7082" } }
        };

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var table = doc.RootElement
            .GetProperty("response")[0]
            .GetProperty("league")
            .GetProperty("standings")[0]
            .EnumerateArray()
            .Select(t => new StandingDto
            {
                Rank = t.GetProperty("rank").GetInt32(),
                TeamName = t.GetProperty("team").GetProperty("name").GetString()!,
                Logo = t.GetProperty("team").GetProperty("logo").GetString()!,
                Played = t.GetProperty("all").GetProperty("played").GetInt32(),
                Win = t.GetProperty("all").GetProperty("win").GetInt32(),
                Draw = t.GetProperty("all").GetProperty("draw").GetInt32(),
                Lose = t.GetProperty("all").GetProperty("lose").GetInt32(),
                Goals = $"{t.GetProperty("all").GetProperty("goals").GetProperty("for").GetInt32()}:{t.GetProperty("all").GetProperty("goals").GetProperty("against").GetInt32()}",
                Points = t.GetProperty("points").GetInt32()
            }).ToList();

        return table;
    }
}
