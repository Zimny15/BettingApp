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
        var matches = await _api.GetTodayMatchesGroupedByLeagueAsync(today.AddDays(0));

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
        string[] seasonsToTry = { "2023", "2022" };
        List<JsonElement> fixtures = [];


        foreach (var season in seasonsToTry)
        {
            var formRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://v3.football.api-sports.io/fixtures?team={teamId}&season={season}&status=FT"),
                Headers = { { "x-apisports-key", "b833b2c7a72d286a0d4b77054b31d6de" } }
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

        var h2hRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://v3.football.api-sports.io/fixtures/headtohead?h2h={teamId}-{opponentId}&status=FT"),
            Headers = { { "x-apisports-key", "b833b2c7a72d286a0d4b77054b31d6de" } }
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

        if (recentFormPoints == 0 && headToHeadPoints == 0)
        {
            var rand = new Random();
            recentFormPoints = rand.Next(1, 11);
            headToHeadPoints = rand.Next(0, 4);
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
        double minProb = 0.1;
        double maxProb = 0.8;

        double homeRaw = Math.Max(1.0, homeScore);
        double awayRaw = Math.Max(1.0, awayScore);
        double total = homeRaw + awayRaw;

        double pHome = homeRaw / total;
        double pAway = awayRaw / total;

        double diff = Math.Abs(homeRaw - awayRaw);
        double pDraw = diff switch
        {
            < 1.0 => 0.30,
            < 2.0 => 0.25,
            < 3.0 => 0.18,
            _ => 0.12
        };

        double norm = pHome + pAway + pDraw;
        pHome = Math.Clamp(pHome / norm, minProb, maxProb);
        pAway = Math.Clamp(pAway / norm, minProb, maxProb);
        pDraw = Math.Clamp(pDraw / norm, 0.10, 0.30);

        double margin = 1.12 + (1.0 - diff / 10.0);

        double oHome = Math.Min(Math.Round(margin / pHome, 2), 6.5);
        double oAway = Math.Min(Math.Round(margin / pAway, 2), 6.5);
        double oDraw = Math.Min(Math.Round(margin / pDraw, 2), 6.0);

        return (oHome, oDraw, oAway);
    }
}

