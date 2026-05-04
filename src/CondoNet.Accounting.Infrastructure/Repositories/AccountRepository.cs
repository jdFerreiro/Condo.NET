using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly Persistence.AccountingDbContext _context;
    public AccountRepository(Persistence.AccountingDbContext context)
    {
        _context = context;
    }
    public async Task<Account?> GetByIdAsync(Guid id) => await _context.Accounts.FindAsync(id);
    public async Task<IEnumerable<Account>> GetAllAsync(Guid organizationId, Guid condominiumId) =>
        await _context.Accounts.Where(a => a.OrganizationId == organizationId && a.CondominiumId == condominiumId).ToListAsync();
    public async Task AddAsync(Account account) => await _context.Accounts.AddAsync(account);
    public async Task UpdateAsync(Account account) => _context.Accounts.Update(account);
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Accounts.FindAsync(id);
        if (entity != null) _context.Accounts.Remove(entity);
    }
    public async Task<bool> ExistsAsync(Guid id) => await _context.Accounts.AnyAsync(a => a.Id == id);
}
