
using CondoNet.BlockChain.Core;
using Nethereum.Web3;
using CondoNet.BlockChain.Infrastructure.ContractDefinition;
using CondoNet.Shared.Financial.DTOs;
using CondoNet.Shared.Payment.DTOs;

namespace CondoNet.BlockChain.Infrastructure;

public class BlockchainIntegrationService : IBlockchainIntegrationService
{
    private readonly IBlockchainService _blockchainService;
    private readonly BlockchainSettings _settings;

    public BlockchainIntegrationService(IBlockchainService blockchainService, BlockchainSettings settings)
    {
        _blockchainService = blockchainService;
        _settings = settings;
    }

    public async Task<BlockchainResponseDto> RegisterPaymentOnChainAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new RegistrarPagoFunction
            {
                UnitId = payment.UnitId,
                Monto = Nethereum.Web3.Web3.Convert.ToWei(payment.Amount),
                Referencia = payment.TransactionReference
            };

            var txHash = await service.RegistrarPagoRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    public Task<BlockchainResponseDto> RegisterSplitOnChainAsync(string unitId, decimal montoOperativo, decimal montoReserva, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlockchainResponseDto { Success = false, ErrorMessage = "Not implemented" });

    public Task<BlockchainResponseDto> RevertPaymentOnChainAsync(string unitId, decimal monto, string referencia, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlockchainResponseDto { Success = false, ErrorMessage = "Not implemented" });

    public Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlockchainResponseDto { Success = false, ErrorMessage = "Not implemented" });

    public Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<BlockchainResponseDto> ModifyExpenseOnChainAsync(string expenseId, string description, decimal amount, DateTime date, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlockchainResponseDto { Success = false, ErrorMessage = "Not implemented" });

    public Task<BlockchainResponseDto> UpdateConfigOnChainAsync(string field, string value, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlockchainResponseDto { Success = false, ErrorMessage = "Not implemented" });
}
