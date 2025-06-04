using BookmakerApp.Data;

namespace BookmakerApp.Models;

public class Wallet
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public decimal Balance { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
