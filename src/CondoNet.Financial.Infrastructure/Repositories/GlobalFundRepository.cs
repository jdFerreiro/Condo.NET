using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class GlobalFundRepository(Persistence.FinancialDbContext context) : IGlobalFundRepository
{
    private readonly Persistence.FinancialDbContext _context = context;

    public async Task<GlobalFund?> GetByTypeAsync(GlobalFundType fundType, CancellationToken cancellationToken = default)
    {
        return await _context.Set<GlobalFund>().FirstOrDefaultAsync(f => f.FundType == fundType, cancellationToken);
    }

    public async Task UpdateAsync(GlobalFund fund, CancellationToken cancellationToken = default)
    {
        _context.Set<GlobalFund>().Update(fund);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
