using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories
{
    public class FinancialRepository(FinancialDbContext context) : IFinancialRepository
    {
        private readonly FinancialDbContext _context = context;

        public async Task<decimal> GetBalanceAsync(int accountId)
        {
            // Ejemplo: Suma de ingresos menos egresos
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .SumAsync(t => t.Type == TransactionType.Credit ? t.Amount : -t.Amount);
        }

        public async Task AddTransactionAsync(Transaction entity)
        {
            await _context.Transactions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
    }
}