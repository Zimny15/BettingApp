namespace BookmakerApp.Models;

public class CombinedBetLeg
{
    public int Id { get; set; }
    public int CombinedBetId { get; set; }
    public CombinedBet CombinedBet { get; set; } = null!;

    public int MatchId { get; set; }
    public string SelectedOutcome { get; set; } = null!;
    public decimal Odds { get; set; }
}
