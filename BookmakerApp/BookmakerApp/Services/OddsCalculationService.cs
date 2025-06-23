using BookmakerApp.Data;
using BookmakerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static System.Net.WebRequestMethods;
namespace BookmakerApp.Services;

public class OddsCalculationService
{
    private readonly ApplicationDbContext _db;
    private readonly ExternalFootballApiService _api;
    private readonly HttpClient _http;

    public OddsCalculationService(ApplicationDbContext db, ExternalFootballApiService api, HttpClient http)
    {
        _db = db;
        _api = api;
        _http = http;
    }

    public async Task CalculateOddsForTomorrowAsync()
    {
        var today = DateTime.UtcNow.Date;
        var matches = await _api.GetTodayMatchesGroupedByLeagueAsync(today.AddDays(1));

        foreach (var match in matches)
        {
            if (_db.MatchOdds.Any(m => m.MatchId == match.FixtureId))
                continue;

            var homeStats = await GetTeamStatsAsync(match.HomeTeamId, match.AwayTeamId, match.HomeTeam, isHome: true);
            var awayStats = await GetTeamStatsAsync(match.AwayTeamId, match.HomeTeamId, match.AwayTeam, isHome: false);

            var homeScore = homeStats.RecentFormPoints + homeStats.HeadToHeadPoints + homeStats.HomeAdvantage;
            var awayScore = awayStats.RecentFormPoints + awayStats.HeadToHeadPoints;

            var (oddsHome, oddsDraw, oddsAway) = CalculateOdds(homeScore, awayScore);

            _db.MatchOdds.Add(new MatchOdds
            {
                MatchId = match.FixtureId,
                MatchDate = match.Date,
                HomeTeam = match.HomeTeam,
                AwayTeam = match.AwayTeam,
                OddsHomeWin = Math.Round((decimal)oddsHome, 2),
                OddsDraw = Math.Round((decimal)oddsDraw, 2),
                OddsAwayWin = Math.Round((decimal)oddsAway, 2),
                CalculatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task<TeamStats> GetTeamStatsAsync(int teamId, int opponentId, string teamName, bool isHome)
    {
        var existingStats = await _db.TeamStats
            .FirstOrDefaultAsync(s => s.TeamId == teamId && s.LastUpdated.Date == DateTime.UtcNow.Date);

        if (existingStats != null)
            return existingStats;

        int recentFormPoints = 0;
        int homeAdvantage = isHome ? 2 : 0;
        int headToHeadPoints = 0;

        // ---- Pobieranie formy zespołu ----
        string[] seasonsToTry = { "2023", "2022" };
        List<JsonElement> fixtures = [];

        foreach (var season in seasonsToTry)
        {
            var formRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://v3.football.api-sports.io/fixtures?team={teamId}&season={season}&status=FT"),
                Headers = { { "x-apisports-key", "13c7527ad64a43ddb42da93ce94f7082" } }
            };
            var formResponse = await _http.SendAsync(formRequest);
            var formJson = await formResponse.Content.ReadFromJsonAsync<JsonElement>();

            fixtures = formJson.GetProperty("response").EnumerateArray()
                .OrderByDescending(f => f.GetProperty("fixture").GetProperty("date").GetDateTime())
                .Take(5)
                .ToList();

            if (fixtures.Count > 0)
                break;
        }

        foreach (var fixture in fixtures)
        {
            var goals = fixture.GetProperty("goals");
            var teams = fixture.GetProperty("teams");
            var isHomeTeam = teams.GetProperty("home").GetProperty("id").GetInt32() == teamId;

            var teamGoals = goals.GetProperty(isHomeTeam ? "home" : "away").GetInt32();
            var oppGoals = goals.GetProperty(isHomeTeam ? "away" : "home").GetInt32();

            if (teamGoals > oppGoals)
                recentFormPoints += 3;
            else if (teamGoals == oppGoals)
                recentFormPoints += 1;
        }

        // ---- Pobieranie ostatniego meczu H2H ----
        var h2hRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://v3.football.api-sports.io/fixtures/headtohead?h2h={teamId}-{opponentId}&status=FT"),
            Headers = { { "x-apisports-key", "13c7527ad64a43ddb42da93ce94f7082" } }
        };
        var h2hResponse = await _http.SendAsync(h2hRequest);
        var h2hJson = await h2hResponse.Content.ReadFromJsonAsync<JsonElement>();

        if (h2hJson.GetProperty("response").GetArrayLength() > 0)
        {
            var lastMatch = h2hJson.GetProperty("response").EnumerateArray()
                .OrderByDescending(m => m.GetProperty("fixture").GetProperty("date").GetDateTime())
                .First();

            var goals = lastMatch.GetProperty("goals");
            var teams = lastMatch.GetProperty("teams");
            var isHomeTeam = teams.GetProperty("home").GetProperty("id").GetInt32() == teamId;

            var teamGoals = goals.GetProperty(isHomeTeam ? "home" : "away").GetInt32();
            var oppGoals = goals.GetProperty(isHomeTeam ? "away" : "home").GetInt32();

            if (teamGoals > oppGoals)
                headToHeadPoints = 3;
            else if (teamGoals == oppGoals)
                headToHeadPoints = 1;
        }

        var stats = new TeamStats
        {
            TeamId = teamId,
            TeamName = teamName,
            RecentFormPoints = recentFormPoints,
            HomeAdvantage = homeAdvantage,
            HeadToHeadPoints = headToHeadPoints,
            LastUpdated = DateTime.UtcNow
        };

        _db.TeamStats.Add(stats);
        await _db.SaveChangesAsync();

        return stats;
    }




    private (double home, double draw, double away) CalculateOdds(int homeScore, int awayScore)
    {
        double diff = Math.Abs(homeScore - awayScore);
        double maxScore = Math.Max(homeScore, awayScore);
        double pDraw = 0.3 - 0.2 * (diff / (maxScore + 1));
        pDraw = Math.Clamp(pDraw * 0.95, 0.08, 0.25);

        double pHome = homeScore;
        double pAway = awayScore;

        // Zapewnienie minimalnej wartości
        double minP = 0.01;
        pHome = Math.Max(pHome, minP);
        pAway = Math.Max(pAway, minP);

        // Normalizacja
        double total = pHome + pAway + pDraw;
        pHome /= total;
        pAway /= total;
        pDraw /= total;

        // Kurs = 1 / prawdopodobieństwo * marża
        double margin = 1.2;

        return (
            Math.Round(margin / pHome, 2),
            Math.Round(margin / pDraw, 2),
            Math.Round(margin / pAway, 2)
        );
    }
}

