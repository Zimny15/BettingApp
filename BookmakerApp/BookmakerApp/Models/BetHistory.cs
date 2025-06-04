namespace BookmakerApp.Models;

public class BetHistory
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int MatchId { get; set; }
    public decimal Amount { get; set; }
    public decimal Odds { get; set; }
    public DateTime BetPlacedAt { get; set; }
    public string Result { get; set; } = "Pending";
    public decimal Payout { get; set; }
}


