using BookmakerApp.Services;
using BookmakerApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookmakerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StandingsController : ControllerBase
{
    private readonly StandingsService _service;

    public StandingsController(StandingsService service)
    {
        _service = service;
    }

    [HttpGet("premierleague")]
    public async Task<ActionResult<List<StandingDto>>> Get()
    {
        var standings = await _service.GetStandingsAsync(39, 2023);
        return Ok(standings);
    }

    [HttpGet("la-liga")]
    public async Task<ActionResult<List<StandingDto>>> GetLaLigaStandings()
    {
        var standings = await _service.GetStandingsAsync(140, 2023);
        return Ok(standings);
    }

    [HttpGet("ekstraklasa")]
    public async Task<ActionResult<List<StandingDto>>> GetEkstraklasaStandings()
    {
        var standings = await _service.GetStandingsAsync(106, 2023);
        return Ok(standings);
    }
}
