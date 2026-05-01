using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class CurrencyRateRepository(FinancialDbContext context) : ICurrencyRateRepository
{
    private readonly FinancialDbContext _context = context;

    public async Task<CurrencyRate> GetByCurrencyAndDateAsync(string currencyIsoCode, DateTime date, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default)
        => await _context.Set<CurrencyRate>()
            .Where(r => r.CurrencyIsoCode == currencyIsoCode && r.Date.Date == date.Date)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CurrencyRate> GetActiveRateAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default)
        => await _context.Set<CurrencyRate>()
            .Where(r => r.CurrencyIsoCode == currencyIsoCode && r.IsActive)
            .OrderByDescending(r => r.Date)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<CurrencyRate>> GetHistoryAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<CurrencyRate>().AsQueryable();
        query = query.Where(r => r.CurrencyIsoCode == currencyIsoCode);
        if (from.HasValue)
            query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.Date <= to.Value);
        return await query.OrderByDescending(r => r.Date).ToListAsync(cancellationToken);
    }

    public async Task AddOrUpdateAsync(CurrencyRate rate, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<CurrencyRate>()
            .FirstOrDefaultAsync(r => r.CurrencyIsoCode == rate.CurrencyIsoCode && r.Date.Date == rate.Date.Date && r.OrganizationId == rate.OrganizationId && r.CondominiumId == rate.CondominiumId, cancellationToken);
        if (existing != null)
        {
            existing.Rate = rate.Rate;
            existing.Source = rate.Source;
            existing.IsActive = rate.IsActive;
            _context.Update(existing);
        }
        else
        {
            await _context.AddAsync(rate, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
