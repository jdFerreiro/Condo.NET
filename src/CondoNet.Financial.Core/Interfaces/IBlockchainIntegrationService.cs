using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IBlockchainIntegrationService
    {
        Task<BlockchainResponseDto> RegisterPaymentOnChainAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
        Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
        Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
    }

}
