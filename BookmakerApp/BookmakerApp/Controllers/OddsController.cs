using BookmakerApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookmakerApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OddsController : ControllerBase
{
    private readonly OddsCalculationService _service;

    public OddsController(OddsCalculationService service)
    {
        _service = service;
    }

    [HttpGet("calculate")]
    public async Task<IActionResult> CalculateTomorrowOdds()
    {
        await _service.CalculateOddsForTomorrowAsync();
        return Ok("Kursy zostały obliczone i zapisane.");
    }
}
