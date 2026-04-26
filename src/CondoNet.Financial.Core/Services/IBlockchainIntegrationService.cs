using CondoNet.Shared.DTOs.Financial;
using System.Threading;
using System.Threading.Tasks;

namespace CondoNet.Financial.Core.Services;

public interface IBlockchainIntegrationService
{
    Task<BlockchainResponseDto> RegisterPaymentOnChainAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
    Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
    Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
}
