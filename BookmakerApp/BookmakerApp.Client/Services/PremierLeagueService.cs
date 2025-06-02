namespace BookmakerApp.Client.Services;

using BookmakerApp.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class PremierLeagueService
{
    private readonly HttpClient _http;

    public PremierLeagueService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MatchDto>> GetMatches()
    {
        return await _http.GetFromJsonAsync<List<MatchDto>>("api/matches");
    }
}