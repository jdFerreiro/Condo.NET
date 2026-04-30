using CondoNet.BlockChain.Core;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Payment.Api.Services;

public interface IPaymentBlockchainService
{
    Task<BlockchainResponseDto> RegisterPaymentAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
}

public class PaymentBlockchainService : IPaymentBlockchainService
{
    private readonly IBlockchainIntegrationService _blockchainIntegrationService;

    public PaymentBlockchainService(IBlockchainIntegrationService blockchainIntegrationService)
    {
        _blockchainIntegrationService = blockchainIntegrationService;
    }

    public async Task<BlockchainResponseDto> RegisterPaymentAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
        => await _blockchainIntegrationService.RegisterPaymentOnChainAsync(payment, cancellationToken);
}
