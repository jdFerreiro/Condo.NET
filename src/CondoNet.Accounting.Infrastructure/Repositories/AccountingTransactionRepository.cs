using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Repositories;

public class AccountingTransactionRepository : IAccountingTransactionRepository
{
    private readonly Persistence.AccountingDbContext _context;
    public AccountingTransactionRepository(Persistence.AccountingDbContext context)
    {
        _context = context;
    }
    public async Task<AccountingTransaction?> GetByIdAsync(Guid id) => await _context.Transactions.Include(t => t.Entries).FirstOrDefaultAsync(t => t.Id == id);
    public async Task<IEnumerable<AccountingTransaction>> GetAllAsync(Guid organizationId, Guid condominiumId) =>
        await _context.Transactions.Where(t => t.OrganizationId == organizationId && t.CondominiumId == condominiumId).Include(t => t.Entries).ToListAsync();
    public async Task AddAsync(AccountingTransaction transaction) => await _context.Transactions.AddAsync(transaction);
    public async Task UpdateAsync(AccountingTransaction transaction) => _context.Transactions.Update(transaction);
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Transactions.FindAsync(id);
        if (entity != null) _context.Transactions.Remove(entity);
    }
    public async Task<bool> ExistsAsync(Guid id) => await _context.Transactions.AnyAsync(t => t.Id == id);
}
