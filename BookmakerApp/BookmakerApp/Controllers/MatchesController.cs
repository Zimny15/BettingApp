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
        var matches = await _api.GetTodayMatchesGroupedByLeagueAsync(DateTime.UtcNow);
        return Ok(matches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchDetailsDto>> GetMatch(int id)
    {
        var match = await _api.GetMatchDetailsAsync(id);
        if (match == null)
            return NotFound();

        return Ok(match);
    }
}
