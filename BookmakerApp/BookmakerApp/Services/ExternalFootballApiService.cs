using BookmakerApp.Shared.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookmakerApp.Services;

public class ExternalFootballApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<ExternalFootballApiService> _logger;

    public ExternalFootballApiService(HttpClient http, ILogger<ExternalFootballApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<MatchDto>> GetTodayMatchesGroupedByLeagueAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var url = $"https://v3.football.api-sports.io/fixtures?date={date}";

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
            Headers =
                {
                    { "x-apisports-key", "13c7527ad64a43ddb42da93ce94f7082" }
        },
        };

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResponse>(json);
        _logger.LogInformation("➡️ Odpowiedź z RapidAPI: {0}", json);

        if (parsed?.Response == null)
        {
            _logger.LogWarning("Brak danych w odpowiedzi RapidAPI. Odpowiedź: {json}", json);
            return new List<MatchDto>();
        }

        return parsed.Response.Select(f => new MatchDto
        {
            Date = f.FixtureInfo.Date,
            LeagueName = f.League.Name,
            LeagueLogo = f.League.Logo,
            HomeTeam = f.Teams.Home.Name,
            AwayTeam = f.Teams.Away.Name,
            HomeTeamLogo = f.Teams.Home.Logo,
            AwayTeamLogo = f.Teams.Away.Logo
        }).ToList();
    }

    private class ApiResponse
    {
        [JsonPropertyName("response")]
        public List<Fixture> Response { get; set; }
    }

    private class Fixture
    {
        [JsonPropertyName("fixture")]
        public FixtureInfo FixtureInfo { get; set; }

        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; }


        [JsonPropertyName("teams")]
        public Teams Teams { get; set; }
    }

    private class LeagueInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }
    }

    private class FixtureInfo
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }

    private class Teams
    {
        [JsonPropertyName("home")]
        public Team Home { get; set; }

        [JsonPropertyName("away")]
        public Team Away { get; set; }
    }

    private class Team
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
    }
}
