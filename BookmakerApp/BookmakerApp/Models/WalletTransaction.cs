namespace BookmakerApp.Models;

public class WalletTransaction
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
}
