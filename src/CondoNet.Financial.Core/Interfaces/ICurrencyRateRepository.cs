using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface ICurrencyRateRepository
{
    Task<CurrencyRate?> GetByCurrencyAndDateAsync(string currencyIsoCode, DateTime date, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default);
    Task<CurrencyRate?> GetActiveRateAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyRate>> GetHistoryAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(CurrencyRate rate, CancellationToken cancellationToken = default);
}
