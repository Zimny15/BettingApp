using BookmakerApp.Data;
using BookmakerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmakerApp.Services;

public class MatchOddsService
{
    private readonly ApplicationDbContext _db;

    public MatchOddsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MatchOdds?> GetOddsForMatchAsync(int matchId)
    {
        return await _db.MatchOdds.FirstOrDefaultAsync(o => o.MatchId == matchId);
    }
}

