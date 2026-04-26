using CondoNet.Financial.Core.Services;
using CondoNet.Financial.Infrastructure.CondoNet;
using CondoNet.Financial.Infrastructure.CondoNet.ContractDefinition;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Infrastructure.Blockchain;

public class BlockchainIntegrationService : IBlockchainIntegrationService
{
    private readonly IBlockchainService _blockchainService;
    private readonly BlockchainSettings _settings;

    public BlockchainIntegrationService(IBlockchainService blockchainService, BlockchainSettings settings)
    {
        _blockchainService = blockchainService;
        _settings = settings;
    }

    // Guarda un mensaje en la blockchain (simula registrar un pago)
    public async Task<BlockchainResponseDto> RegisterPaymentOnChainAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new GuardarMensajeFunction
            {
                NuevoMensaje = $"Pago registrado: Unidad={payment.UnitId}, Monto={payment.Amount}, Ref={payment.TransactionReference}"
            };

            var txHash = await service.GuardarMensajeRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    // Guarda el Merkle Root como mensaje (simula anclaje de integridad)
    public async Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new GuardarMensajeFunction
            {
                NuevoMensaje = $"MerkleRoot:{merkleRoot}|Periodo:{period:yyyy-MM}"
            };

            var txHash = await service.GuardarMensajeRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    // Valida el Merkle Root leyendo el mensaje más reciente
    public async Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var mensaje = await service.LeerMensajeQueryAsync();
            // Espera que el mensaje tenga el formato "MerkleRoot:{merkleRoot}|Periodo:{period:yyyy-MM}"
            var expected = $"MerkleRoot:{merkleRoot}|Periodo:{period:yyyy-MM}";
            return mensaje == expected;
        }
        catch
        {
            return false;
        }
    }
}
