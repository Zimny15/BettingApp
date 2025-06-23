using BookmakerApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookmakerApp.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<BetHistory> Bets => Set<BetHistory>();
    public DbSet<MatchOdds> MatchOdds => Set<MatchOdds>();
    public DbSet<TeamStats> TeamStats => Set<TeamStats>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wallet>()
        .Property(w => w.Balance)
        .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<WalletTransaction>()
            .Property(wt => wt.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<BetHistory>(b =>
        {
            b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            b.Property(p => p.Odds).HasColumnType("decimal(5,2)");
            b.Property(p => p.Payout).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<MatchOdds>(m =>
        {
            m.HasIndex(e => e.MatchId).IsUnique();

            m.Property(p => p.OddsHomeWin).HasColumnType("decimal(5,2)");
            m.Property(p => p.OddsDraw).HasColumnType("decimal(5,2)");
            m.Property(p => p.OddsAwayWin).HasColumnType("decimal(5,2)");
        });
    }
}
