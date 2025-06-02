using BookmakerApp.Services;
using BookmakerApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookmakerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly ExternalFootballApiService _api;

    public MatchesController(ExternalFootballApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchDto>>> Get()
    {
        //var matches = await _api.GetPremierLeagueMatchesAsync();
        //return Ok(matches);

        //    var test = new List<MatchDto>
        //{
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.AddHours(3),
        //        HomeTeam = "Liverpool",
        //        AwayTeam = "Chelsea"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.Date.AddHours(14), // 14:00 UTC
        //        HomeTeam = "Arsenal",
        //        AwayTeam = "Aston Villa",
        //        HomeTeamLogo = "https://media.api-sports.io/football/teams/42.png",
        //        AwayTeamLogo = "https://media.api-sports.io/football/teams/66.png"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.Date.AddHours(16), // 16:00 UTC
        //        HomeTeam = "Chelsea",
        //        AwayTeam = "Everton",
        //        HomeTeamLogo = "https://media.api-sports.io/football/teams/49.png",
        //        AwayTeamLogo = "https://media.api-sports.io/football/teams/45.png"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.Date.AddHours(19), // 19:00 UTC
        //        HomeTeam = "Liverpool",
        //        AwayTeam = "Brentford"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.AddHours(24),
        //        HomeTeam = "Arsenal",
        //        AwayTeam = "Manchester City"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.Date.AddDays(1).AddHours(13), // 13:00 UTC
        //        HomeTeam = "Manchester United",
        //        AwayTeam = "Newcastle"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.Date.AddDays(1).AddHours(15), // 15:00 UTC
        //        HomeTeam = "Tottenham",
        //        AwayTeam = "Wolverhampton"
        //    },
        //    new MatchDto
        //    {
        //        Date = DateTime.UtcNow.Date.AddDays(1).AddHours(18), // 18:00 UTC
        //        HomeTeam = "West Ham",
        //        AwayTeam = "Brighton"
        //    }
        //};
        //    return Ok(test);

        var matches = await _api.GetTodayMatchesGroupedByLeagueAsync();
        return Ok(matches);
    }
}
