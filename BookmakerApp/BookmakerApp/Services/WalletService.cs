﻿using BookmakerApp.Data;
using BookmakerApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookmakerApp.Services
{
    public class WalletService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WalletService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Wallet> CreateWalletAsync(ApplicationUser user)
        {
            var wallet = new Wallet
            {
                UserId = user.Id,
                Balance = 0
            };
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task CreateTransactionAsync(int walletId, decimal amount, string type)
        {
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                Type = type,
                Timestamp = DateTime.UtcNow
            };
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateWalletBalanceAsync(int walletId, decimal amount, bool isDeposit)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null)
                return false;

            wallet.Balance += isDeposit ? amount : -amount;

            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Wallet?> GetCurrentUserWalletAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return null;

            var wallet = await _context.Wallets.Include(w => w.User).FirstOrDefaultAsync(w => w.UserId == user.Id);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0
                };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }
            return wallet;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<WalletTransaction>> GetWalletTransactionsAsync(int walletId)
        {
            return await _context.WalletTransactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }
        public async Task<bool> AddFundsAsync(decimal amount)
        {
            var wallet = await GetCurrentUserWalletAsync();
            if (wallet == null)
                return false;

            wallet.Balance += amount;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                Type = "Top-up",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WithdrawFundsAsync(decimal amount)
        {
            var wallet = await GetCurrentUserWalletAsync();
            if (wallet == null || amount <= 0 || wallet.Balance < amount)
                return false;

            wallet.Balance -= amount;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = -amount,
                Type = "Withdrawal",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveBetAsync(BetHistory bet, Wallet wallet)
        {
            _context.Bets.Add(bet);
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _context.WalletTransactions.FindAsync(transactionId);
            if (transaction == null)
                return false;

            _context.WalletTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
