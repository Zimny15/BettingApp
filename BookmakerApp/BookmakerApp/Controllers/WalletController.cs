using BookmakerApp.Data;
using BookmakerApp.Models;
using BookmakerApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public WalletController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<WalletDto>> Get()
    {
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id)
                     ?? new Wallet { UserId = user.Id, Balance = 0 };
        return Ok(new WalletDto { Balance = wallet.Balance });
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit()
    {
        var user = await _userManager.GetUserAsync(User);
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);

        if (wallet == null)
        {
            wallet = new Wallet { UserId = user.Id, Balance = 0 };
            _db.Wallets.Add(wallet);
        }

        wallet.Balance += 10;
        _db.WalletTransactions.Add(new WalletTransaction
        {
            Wallet = wallet,
            Amount = 10,
            Timestamp = DateTime.UtcNow,
            Type = "Deposit"
        });

        await _db.SaveChangesAsync();
        return Ok();
    }
}
