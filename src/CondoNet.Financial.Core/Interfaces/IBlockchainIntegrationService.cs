using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IBlockchainIntegrationService
    {
        Task<BlockchainResponseDto> RegisterPaymentOnChainAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
        Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
        Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default);
        Task<BlockchainResponseDto> RegisterSplitOnChainAsync(string unitId, decimal montoOperativo, decimal montoReserva, CancellationToken cancellationToken = default);
        Task<BlockchainResponseDto> ModifyExpenseOnChainAsync(string expenseId, string descripcion, decimal monto, DateTime fecha, CancellationToken cancellationToken = default);
        Task<BlockchainResponseDto> RevertPaymentOnChainAsync(string unitId, decimal monto, string referencia, CancellationToken cancellationToken = default);
        Task<BlockchainResponseDto> UpdateConfigOnChainAsync(string campo, string valor, CancellationToken cancellationToken = default);
    }

}
