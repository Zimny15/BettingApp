namespace BookmakerApp.Models;

public class CombinedBet
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public decimal Stake { get; set; }
    public decimal TotalOdds { get; set; }
    public decimal PotentialPayout { get; set; }
    public string Result { get; set; } = "Pending";

    public List<CombinedBetLeg> Legs { get; set; } = new();
}
