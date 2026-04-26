using CondoNet.Financial.Infrastructure.CondoNet.ContractDefinition;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;

namespace CondoNet.Financial.Infrastructure.CondoNet
{
    public partial class CondoNetService(Nethereum.Web3.IWeb3 web3, string contractAddress) : CondoNetServiceBase(web3, contractAddress)
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.IWeb3 web3, CondoNetDeployment condoNetDeployment, CancellationTokenSource cancellationTokenSource = null!)
        {
            return web3.Eth.GetContractDeploymentHandler<CondoNetDeployment>().SendRequestAndWaitForReceiptAsync(condoNetDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.IWeb3 web3, CondoNetDeployment condoNetDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<CondoNetDeployment>().SendRequestAsync(condoNetDeployment);
        }

        public static async Task<CondoNetService> DeployContractAndGetServiceAsync(Nethereum.Web3.IWeb3 web3, CondoNetDeployment condoNetDeployment, CancellationTokenSource cancellationTokenSource = null!)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, condoNetDeployment, cancellationTokenSource);
            return new CondoNetService(web3, receipt.ContractAddress);
        }
    }


    public partial class CondoNetServiceBase(Nethereum.Web3.IWeb3 web3, string contractAddress) : ContractWeb3ServiceBase(web3, contractAddress)
    {
        public virtual Task<string> ActualizarConfiguracionRequestAsync(ActualizarConfiguracionFunction actualizarConfiguracionFunction)
        {
            return ContractHandler.SendRequestAsync(actualizarConfiguracionFunction);
        }

        public virtual Task<TransactionReceipt> ActualizarConfiguracionRequestAndWaitForReceiptAsync(ActualizarConfiguracionFunction actualizarConfiguracionFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(actualizarConfiguracionFunction, cancellationToken);
        }

        public virtual Task<string> ActualizarConfiguracionRequestAsync(string campo, string valor)
        {
            var actualizarConfiguracionFunction = new ActualizarConfiguracionFunction
            {
                Campo = campo,
                Valor = valor
            };

            return ContractHandler.SendRequestAsync(actualizarConfiguracionFunction);
        }

        public virtual Task<TransactionReceipt> ActualizarConfiguracionRequestAndWaitForReceiptAsync(string campo, string valor, CancellationTokenSource cancellationToken = null!)
        {
            var actualizarConfiguracionFunction = new ActualizarConfiguracionFunction
            {
                Campo = campo,
                Valor = valor
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(actualizarConfiguracionFunction, cancellationToken);
        }

        public Task<string> AdminQueryAsync(AdminFunction adminFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<AdminFunction, string>(adminFunction, blockParameter);
        }


        public virtual Task<string> AdminQueryAsync(BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<AdminFunction, string>(null!, blockParameter);
        }

        public virtual Task<string> AnchorMerkleRootRequestAsync(AnchorMerkleRootFunction anchorMerkleRootFunction)
        {
            return ContractHandler.SendRequestAsync(anchorMerkleRootFunction);
        }

        public virtual Task<TransactionReceipt> AnchorMerkleRootRequestAndWaitForReceiptAsync(AnchorMerkleRootFunction anchorMerkleRootFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(anchorMerkleRootFunction, cancellationToken);
        }

        public virtual Task<string> AnchorMerkleRootRequestAsync(string merkleRoot, BigInteger year, BigInteger month)
        {
            var anchorMerkleRootFunction = new AnchorMerkleRootFunction
            {
                MerkleRoot = merkleRoot,
                Year = year,
                Month = month
            };

            return ContractHandler.SendRequestAsync(anchorMerkleRootFunction);
        }

        public virtual Task<TransactionReceipt> AnchorMerkleRootRequestAndWaitForReceiptAsync(string merkleRoot, BigInteger year, BigInteger month, CancellationTokenSource cancellationToken = null!)
        {
            var anchorMerkleRootFunction = new AnchorMerkleRootFunction
            {
                MerkleRoot = merkleRoot,
                Year = year,
                Month = month
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(anchorMerkleRootFunction, cancellationToken);
        }

        public virtual Task<string> AnclarGastoRequestAsync(AnclarGastoFunction anclarGastoFunction)
        {
            return ContractHandler.SendRequestAsync(anclarGastoFunction);
        }

        public virtual Task<TransactionReceipt> AnclarGastoRequestAndWaitForReceiptAsync(AnclarGastoFunction anclarGastoFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(anclarGastoFunction, cancellationToken);
        }

        public virtual Task<string> AnclarGastoRequestAsync(string expenseId)
        {
            var anclarGastoFunction = new AnclarGastoFunction
            {
                ExpenseId = expenseId
            };

            return ContractHandler.SendRequestAsync(anclarGastoFunction);
        }

        public virtual Task<TransactionReceipt> AnclarGastoRequestAndWaitForReceiptAsync(string expenseId, CancellationTokenSource cancellationToken = null!)
        {
            var anclarGastoFunction = new AnclarGastoFunction
            {
                ExpenseId = expenseId
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(anclarGastoFunction, cancellationToken);
        }

        public virtual Task<string> CertificarPagoRequestAsync(CertificarPagoFunction certificarPagoFunction)
        {
            return ContractHandler.SendRequestAsync(certificarPagoFunction);
        }

        public virtual Task<TransactionReceipt> CertificarPagoRequestAndWaitForReceiptAsync(CertificarPagoFunction certificarPagoFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(certificarPagoFunction, cancellationToken);
        }

        public virtual Task<string> CertificarPagoRequestAsync(string referencia)
        {
            var certificarPagoFunction = new CertificarPagoFunction
            {
                Referencia = referencia
            };

            return ContractHandler.SendRequestAsync(certificarPagoFunction);
        }

        public virtual Task<TransactionReceipt> CertificarPagoRequestAndWaitForReceiptAsync(string referencia, CancellationTokenSource cancellationToken = null!)
        {
            var certificarPagoFunction = new CertificarPagoFunction
            {
                Referencia = referencia
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(certificarPagoFunction, cancellationToken);
        }

        public Task<string> ConfiguracionesQueryAsync(ConfiguracionesFunction configuracionesFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<ConfiguracionesFunction, string>(configuracionesFunction, blockParameter);
        }


        public virtual Task<string> ConfiguracionesQueryAsync(string returnValue1, BlockParameter blockParameter = null!)
        {
            var configuracionesFunction = new ConfiguracionesFunction
            {
                ReturnValue1 = returnValue1
            };

            return ContractHandler.QueryAsync<ConfiguracionesFunction, string>(configuracionesFunction, blockParameter);
        }

        public virtual Task<GastosOutputDTO> GastosQueryAsync(GastosFunction gastosFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GastosFunction, GastosOutputDTO>(gastosFunction, blockParameter);
        }

        public virtual Task<GastosOutputDTO> GastosQueryAsync(string returnValue1, BlockParameter blockParameter = null!)
        {
            var gastosFunction = new GastosFunction
            {
                ReturnValue1 = returnValue1
            };

            return ContractHandler.QueryDeserializingToObjectAsync<GastosFunction, GastosOutputDTO>(gastosFunction, blockParameter);
        }

        public Task<string> GetConfiguracionQueryAsync(GetConfiguracionFunction getConfiguracionFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<GetConfiguracionFunction, string>(getConfiguracionFunction, blockParameter);
        }


        public virtual Task<string> GetConfiguracionQueryAsync(string campo, BlockParameter blockParameter = null!)
        {
            var getConfiguracionFunction = new GetConfiguracionFunction
            {
                Campo = campo
            };

            return ContractHandler.QueryAsync<GetConfiguracionFunction, string>(getConfiguracionFunction, blockParameter);
        }

        public Task<string> GetMerkleRootQueryAsync(GetMerkleRootFunction getMerkleRootFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<GetMerkleRootFunction, string>(getMerkleRootFunction, blockParameter);
        }


        public virtual Task<string> GetMerkleRootQueryAsync(BigInteger year, BigInteger month, BlockParameter blockParameter = null!)
        {
            var getMerkleRootFunction = new GetMerkleRootFunction
            {
                Year = year,
                Month = month
            };

            return ContractHandler.QueryAsync<GetMerkleRootFunction, string>(getMerkleRootFunction, blockParameter);
        }

        public Task<string> MerkleRootsQueryAsync(MerkleRootsFunction merkleRootsFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<MerkleRootsFunction, string>(merkleRootsFunction, blockParameter);
        }


        public virtual Task<string> MerkleRootsQueryAsync(BigInteger returnValue1, BigInteger returnValue2, BlockParameter blockParameter = null!)
        {
            var merkleRootsFunction = new MerkleRootsFunction
            {
                ReturnValue1 = returnValue1,
                ReturnValue2 = returnValue2
            };

            return ContractHandler.QueryAsync<MerkleRootsFunction, string>(merkleRootsFunction, blockParameter);
        }

        public virtual Task<string> ModificarGastoRequestAsync(ModificarGastoFunction modificarGastoFunction)
        {
            return ContractHandler.SendRequestAsync(modificarGastoFunction);
        }

        public virtual Task<TransactionReceipt> ModificarGastoRequestAndWaitForReceiptAsync(ModificarGastoFunction modificarGastoFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(modificarGastoFunction, cancellationToken);
        }

        public virtual Task<string> ModificarGastoRequestAsync(string expenseId, string descripcion, BigInteger monto, BigInteger fecha)
        {
            var modificarGastoFunction = new ModificarGastoFunction
            {
                ExpenseId = expenseId,
                Descripcion = descripcion,
                Monto = monto,
                Fecha = fecha
            };

            return ContractHandler.SendRequestAsync(modificarGastoFunction);
        }

        public virtual Task<TransactionReceipt> ModificarGastoRequestAndWaitForReceiptAsync(string expenseId, string descripcion, BigInteger monto, BigInteger fecha, CancellationTokenSource cancellationToken = null!)
        {
            var modificarGastoFunction = new ModificarGastoFunction
            {
                ExpenseId = expenseId,
                Descripcion = descripcion,
                Monto = monto,
                Fecha = fecha
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(modificarGastoFunction, cancellationToken);
        }

        public virtual Task<PagosOutputDTO> PagosQueryAsync(PagosFunction pagosFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<PagosFunction, PagosOutputDTO>(pagosFunction, blockParameter);
        }

        public virtual Task<PagosOutputDTO> PagosQueryAsync(string returnValue1, BlockParameter blockParameter = null!)
        {
            var pagosFunction = new PagosFunction
            {
                ReturnValue1 = returnValue1
            };

            return ContractHandler.QueryDeserializingToObjectAsync<PagosFunction, PagosOutputDTO>(pagosFunction, blockParameter);
        }

        public virtual Task<string> RegistrarPagoRequestAsync(RegistrarPagoFunction registrarPagoFunction)
        {
            return ContractHandler.SendRequestAsync(registrarPagoFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarPagoRequestAndWaitForReceiptAsync(RegistrarPagoFunction registrarPagoFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarPagoFunction, cancellationToken);
        }

        public virtual Task<string> RegistrarPagoRequestAsync(string unitId, BigInteger monto, string referencia)
        {
            var registrarPagoFunction = new RegistrarPagoFunction
            {
                UnitId = unitId,
                Monto = monto,
                Referencia = referencia
            };

            return ContractHandler.SendRequestAsync(registrarPagoFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarPagoRequestAndWaitForReceiptAsync(string unitId, BigInteger monto, string referencia, CancellationTokenSource cancellationToken = null!)
        {
            var registrarPagoFunction = new RegistrarPagoFunction
            {
                UnitId = unitId,
                Monto = monto,
                Referencia = referencia
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarPagoFunction, cancellationToken);
        }

        public virtual Task<string> RegistrarSplitRequestAsync(RegistrarSplitFunction registrarSplitFunction)
        {
            return ContractHandler.SendRequestAsync(registrarSplitFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarSplitRequestAndWaitForReceiptAsync(RegistrarSplitFunction registrarSplitFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarSplitFunction, cancellationToken);
        }

        public virtual Task<string> RegistrarSplitRequestAsync(string unitId, BigInteger montoOperativo, BigInteger montoReserva)
        {
            var registrarSplitFunction = new RegistrarSplitFunction
            {
                UnitId = unitId,
                MontoOperativo = montoOperativo,
                MontoReserva = montoReserva
            };

            return ContractHandler.SendRequestAsync(registrarSplitFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarSplitRequestAndWaitForReceiptAsync(string unitId, BigInteger montoOperativo, BigInteger montoReserva, CancellationTokenSource cancellationToken = null!)
        {
            var registrarSplitFunction = new RegistrarSplitFunction
            {
                UnitId = unitId,
                MontoOperativo = montoOperativo,
                MontoReserva = montoReserva
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarSplitFunction, cancellationToken);
        }

        public virtual Task<string> RevertirPagoRequestAsync(RevertirPagoFunction revertirPagoFunction)
        {
            return ContractHandler.SendRequestAsync(revertirPagoFunction);
        }

        public virtual Task<TransactionReceipt> RevertirPagoRequestAndWaitForReceiptAsync(RevertirPagoFunction revertirPagoFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(revertirPagoFunction, cancellationToken);
        }

        public virtual Task<string> RevertirPagoRequestAsync(string unitId, BigInteger monto, string referencia)
        {
            var revertirPagoFunction = new RevertirPagoFunction
            {
                UnitId = unitId,
                Monto = monto,
                Referencia = referencia
            };

            return ContractHandler.SendRequestAsync(revertirPagoFunction);
        }

        public virtual Task<TransactionReceipt> RevertirPagoRequestAndWaitForReceiptAsync(string unitId, BigInteger monto, string referencia, CancellationTokenSource cancellationToken = null!)
        {
            var revertirPagoFunction = new RevertirPagoFunction
            {
                UnitId = unitId,
                Monto = monto,
                Referencia = referencia
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(revertirPagoFunction, cancellationToken);
        }

        public override List<Type> GetAllFunctionTypes()
        {
            return
            [
                typeof(ActualizarConfiguracionFunction),
                typeof(AdminFunction),
                typeof(AnchorMerkleRootFunction),
                typeof(AnclarGastoFunction),
                typeof(CertificarPagoFunction),
                typeof(ConfiguracionesFunction),
                typeof(GastosFunction),
                typeof(GetConfiguracionFunction),
                typeof(GetMerkleRootFunction),
                typeof(MerkleRootsFunction),
                typeof(ModificarGastoFunction),
                typeof(PagosFunction),
                typeof(RegistrarPagoFunction),
                typeof(RegistrarSplitFunction),
                typeof(RevertirPagoFunction)
            ];
        }

        public override List<Type> GetAllEventTypes()
        {
            return
            [
                typeof(ConfiguracionActualizadaEventDTO),
                typeof(GastoModificadoEventDTO),
                typeof(MerkleRootAncladoEventDTO),
                typeof(PagoRegistradoEventDTO),
                typeof(PagoRevertidoEventDTO),
                typeof(SplitRegistradoEventDTO)
            ];
        }

        public override List<Type> GetAllErrorTypes()
        {
            return [];
        }
    }
}
