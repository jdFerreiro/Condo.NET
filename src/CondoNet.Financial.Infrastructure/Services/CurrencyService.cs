using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Core.Services;

namespace CondoNet.Financial.Infrastructure.Services;

public class CurrencyService(ICurrencyRateRepository currencyRateRepository,
    ICurrencyAdjustmentLogRepository adjustmentLogRepository) : ICurrencyService
{
    private readonly ICurrencyRateRepository _currencyRateRepository = currencyRateRepository;
    private readonly ICurrencyAdjustmentLogRepository _adjustmentLogRepository = adjustmentLogRepository;


    public async Task<CurrencyRate?> GetRateAsync(string currencyIsoCode, DateTime date, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default)
        => await _currencyRateRepository.GetByCurrencyAndDateAsync(currencyIsoCode, date, organizationId, condominiumId, cancellationToken);

    public async Task<CurrencyRate?> GetActiveRateAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, CancellationToken cancellationToken = default)
        => await _currencyRateRepository.GetActiveRateAsync(currencyIsoCode, organizationId, condominiumId, cancellationToken);

    public async Task<IEnumerable<CurrencyRate>> GetRatesHistoryAsync(string currencyIsoCode, Guid organizationId, Guid condominiumId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        => await _currencyRateRepository.GetHistoryAsync(currencyIsoCode, organizationId, condominiumId, from, to, cancellationToken);

    public async Task AddOrUpdateRateAsync(CurrencyRate rate, CancellationToken cancellationToken = default)
    {
        // Obtener la tasa anterior si existe
        var previous = await _currencyRateRepository.GetActiveRateAsync(rate.CurrencyIsoCode, rate.OrganizationId, rate.CondominiumId, cancellationToken);
        await _currencyRateRepository.AddOrUpdateAsync(rate, cancellationToken);

        // Registrar ajuste solo si hay cambio
        if (previous != null && previous.Rate != rate.Rate)
        {
            var log = new CurrencyAdjustmentLog
            {
                OrganizationId = rate.OrganizationId,
                EntityType = EntityType.Rate,
                EntityId = rate.Id,
                PreviousRate = previous.Rate,
                NewRate = rate.Rate,
                AmountVesDiff = rate.Rate - previous.Rate,
                Reason = CurrencyAdjustmentReason.RateUpdate,
                CreatedAt = DateTime.UtcNow
            };
            await _adjustmentLogRepository.AddAsync(log, cancellationToken);
        }
    }
}
