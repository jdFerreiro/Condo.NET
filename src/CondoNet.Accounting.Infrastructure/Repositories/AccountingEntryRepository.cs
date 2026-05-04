using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Repositories;

public class AccountingEntryRepository : IAccountingEntryRepository
{
    private readonly Persistence.AccountingDbContext _context;
    public AccountingEntryRepository(Persistence.AccountingDbContext context)
    {
        _context = context;
    }
    public async Task<AccountingEntry?> GetByIdAsync(Guid id) => await _context.Entries.FindAsync(id);
    public async Task<IEnumerable<AccountingEntry>> GetAllByTransactionAsync(Guid transactionId) =>
        await _context.Entries.Where(e => e.TransactionId == transactionId).ToListAsync();
    public async Task AddAsync(AccountingEntry entry) => await _context.Entries.AddAsync(entry);
    public async Task UpdateAsync(AccountingEntry entry) => _context.Entries.Update(entry);
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Entries.FindAsync(id);
        if (entity != null) _context.Entries.Remove(entity);
    }
    public async Task<bool> ExistsAsync(Guid id) => await _context.Entries.AnyAsync(e => e.Id == id);
}
