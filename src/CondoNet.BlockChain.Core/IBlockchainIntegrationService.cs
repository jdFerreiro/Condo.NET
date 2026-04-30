using System.Threading;
using System.Threading.Tasks;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.BlockChain.Core;

public interface IBlockchainIntegrationService
{
    Task<BlockchainResponseDto> RegisterPaymentOnChainAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
    Task<BlockchainResponseDto> RegisterSplitOnChainAsync(string unitId, decimal montoOperativo, decimal montoReserva, CancellationToken cancellationToken = default);
    Task<BlockchainResponseDto> RevertPaymentOnChainAsync(string unitId, decimal monto, string referencia, CancellationToken cancellationToken = default);
    Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
    Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
    Task<BlockchainResponseDto> ModifyExpenseOnChainAsync(string expenseId, string description, decimal amount, DateTime date, CancellationToken cancellationToken = default);
    Task<BlockchainResponseDto> UpdateConfigOnChainAsync(string field, string value, CancellationToken cancellationToken = default);
}
