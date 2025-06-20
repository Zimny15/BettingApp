using BookmakerApp.Shared.Models;
using System.Net.Http;
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

    public async Task<MatchDetailsDto?> GetMatchDetailsAsync(int matchId)
    {
        // Get statistics
        var statsRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://v3.football.api-sports.io/fixtures/statistics?fixture={matchId}"),
            Headers =
        {
            { "x-apisports-key", "13c7527ad64a43ddb42da93ce94f7082" }
        }
        };

        var statsResponse = await _http.SendAsync(statsRequest);
        if (!statsResponse.IsSuccessStatusCode)
            return null;

        var statsJson = await statsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var statResponse = statsJson.GetProperty("response");

        if (statResponse.GetArrayLength() < 2)
            return null;
        var homeStats = statResponse[0].GetProperty("statistics");
        var awayStats = statResponse[1].GetProperty("statistics");
        Console.WriteLine($"⚠️ Unexpected statistics count: {statResponse.GetArrayLength()} for match {matchId}");

        // Match basic info
        var matchInfo = await GetMatchBaseInfoAsync(matchId);

        // Combine statistics
        var statistics = new List<MatchStatDto>();
        for (int i = 0; i < homeStats.GetArrayLength(); i++)
        {
            var type = homeStats[i].GetProperty("type").GetString();
            var homeValue = homeStats[i].GetProperty("value").ToString();
            var awayValue = awayStats[i].GetProperty("value").ToString();

            statistics.Add(new MatchStatDto
            {
                Type = type,
                HomeValue = homeValue,
                AwayValue = awayValue
            });
        }

        return new MatchDetailsDto
        {
            HomeTeam = matchInfo.HomeTeam,
            AwayTeam = matchInfo.AwayTeam,
            HomeTeamLogo = matchInfo.HomeLogo,
            AwayTeamLogo = matchInfo.AwayLogo,
            HomeGoals = matchInfo.HomeGoals,
            AwayGoals = matchInfo.AwayGoals,
            Statistics = statistics
        };
    }

    private async Task<(string HomeTeam, string AwayTeam, string HomeLogo, string AwayLogo, int HomeGoals, int AwayGoals)> GetMatchBaseInfoAsync(int matchId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://v3.football.api-sports.io/fixtures?id={matchId}"),
            Headers =
        {
            { "x-apisports-key", "13c7527ad64a43ddb42da93ce94f7082" }
        }
        };

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var fixture = json.GetProperty("response")[0];

        var home = fixture.GetProperty("teams").GetProperty("home");
        var away = fixture.GetProperty("teams").GetProperty("away");
        var goals = fixture.GetProperty("goals");

        return (
            home.GetProperty("name").GetString(),
            away.GetProperty("name").GetString(),
            home.GetProperty("logo").GetString(),
            away.GetProperty("logo").GetString(),
            goals.GetProperty("home").GetInt32(),
            goals.GetProperty("away").GetInt32()
        );
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
            FixtureId = f.FixtureInfo?.Id ?? 0,
            Date = f.FixtureInfo?.Date ?? DateTime.MinValue,
            LeagueName = f.League?.Name ?? "Unknown League",
            LeagueLogo = f.League?.Logo,
            HomeTeam = f.Teams?.Home?.Name ?? "Team A",
            AwayTeam = f.Teams?.Away?.Name ?? "Team B",
            HomeTeamLogo = f.Teams?.Home?.Logo,
            AwayTeamLogo = f.Teams?.Away?.Logo,
            Status = f.Status?.Short ?? "NS",
            HomeGoals = f.Goals?.Home,
            AwayGoals = f.Goals?.Away
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
        public FixtureInfo? FixtureInfo { get; set; }

        [JsonPropertyName("league")]
        public LeagueInfo? League { get; set; }

        [JsonPropertyName("teams")]
        public Teams? Teams { get; set; }

        [JsonPropertyName("status")]
        public FixtureStatus? Status { get; set; }

        [JsonPropertyName("goals")]
        public Goals? Goals { get; set; }

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

        [JsonPropertyName("id")]
        public int Id { get; set; }
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
    private class FixtureStatus
    {
        [JsonPropertyName("short")]
        public string Short { get; set; }
    }

    private class Goals
    {
        [JsonPropertyName("home")]
        public int? Home { get; set; }

        [JsonPropertyName("away")]
        public int? Away { get; set; }
    }

}
