using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.CondoNet;
using CondoNet.Financial.Infrastructure.CondoNet.ContractDefinition;
using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Infrastructure.Blockchain;

public class BlockchainIntegrationService(IBlockchainService blockchainService, BlockchainSettings settings) : IBlockchainIntegrationService
{
    private readonly IBlockchainService _blockchainService = blockchainService;
    private readonly BlockchainSettings _settings = settings;


    // Registrar un pago en blockchain
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


    // Anclar Merkle Root en blockchain
    public async Task<BlockchainResponseDto> AnchorMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new AnchorMerkleRootFunction
            {
                MerkleRoot = merkleRoot,
                Year = period.Year,
                Month = period.Month
            };

            var txHash = await service.AnchorMerkleRootRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }


    // Validar Merkle Root en blockchain
    public async Task<bool> ValidateMerkleRootAsync(string merkleRoot, DateTime period, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var result = await service.GetMerkleRootQueryAsync(period.Year, period.Month);
            return string.Equals(result, merkleRoot, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    // Registrar split en blockchain
    public async Task<BlockchainResponseDto> RegisterSplitOnChainAsync(string unitId, decimal montoOperativo, decimal montoReserva, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new RegistrarSplitFunction
            {
                UnitId = unitId,
                MontoOperativo = Nethereum.Web3.Web3.Convert.ToWei(montoOperativo),
                MontoReserva = Nethereum.Web3.Web3.Convert.ToWei(montoReserva)
            };

            var txHash = await service.RegistrarSplitRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    // Modificar gasto en blockchain
    public async Task<BlockchainResponseDto> ModifyExpenseOnChainAsync(string expenseId, string descripcion, decimal monto, DateTime fecha, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new ModificarGastoFunction
            {
                ExpenseId = expenseId,
                Descripcion = descripcion,
                Monto = Nethereum.Web3.Web3.Convert.ToWei(monto),
                Fecha = new System.Numerics.BigInteger(((DateTimeOffset)fecha).ToUnixTimeSeconds())
            };

            var txHash = await service.ModificarGastoRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    // Revertir pago en blockchain
    public async Task<BlockchainResponseDto> RevertPaymentOnChainAsync(string unitId, decimal monto, string referencia, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new RevertirPagoFunction
            {
                UnitId = unitId,
                Monto = Nethereum.Web3.Web3.Convert.ToWei(monto),
                Referencia = referencia
            };

            var txHash = await service.RevertirPagoRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }

    // Actualizar configuración financiera en blockchain
    public async Task<BlockchainResponseDto> UpdateConfigOnChainAsync(string campo, string valor, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = _blockchainService.Web3Instance;
            var contractAddress = _settings.ContractAddress;
            var service = new CondoNetService(web3, contractAddress);

            var function = new ActualizarConfiguracionFunction
            {
                Campo = campo,
                Valor = valor
            };

            var txHash = await service.ActualizarConfiguracionRequestAsync(function);
            return new BlockchainResponseDto { Success = true, TransactionHash = txHash };
        }
        catch (Exception ex)
        {
            return new BlockchainResponseDto { Success = false, ErrorMessage = ex.Message };
        }
    }
}
