using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Services
{
    public interface ICurrencyService
    {
        Task<CurrencyRate?> GetRateAsync(string currencyIsoCode, DateTime date, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default);
        Task<CurrencyRate?> GetActiveRateAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CurrencyRate>> GetRatesHistoryAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
        Task AddOrUpdateRateAsync(CurrencyRate rate, CancellationToken cancellationToken = default);
    }

}
