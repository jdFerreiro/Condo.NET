using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using CondoNet.Financial.Infrastructure.CondoNet.ContractDefinition;

namespace CondoNet.Financial.Infrastructure.CondoNet
{
    public partial class CondoNetService: CondoNetServiceBase
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.IWeb3 web3, CondoNetDeployment condoNetDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<CondoNetDeployment>().SendRequestAndWaitForReceiptAsync(condoNetDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.IWeb3 web3, CondoNetDeployment condoNetDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<CondoNetDeployment>().SendRequestAsync(condoNetDeployment);
        }

        public static async Task<CondoNetService> DeployContractAndGetServiceAsync(Nethereum.Web3.IWeb3 web3, CondoNetDeployment condoNetDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, condoNetDeployment, cancellationTokenSource);
            return new CondoNetService(web3, receipt.ContractAddress);
        }

        public CondoNetService(Nethereum.Web3.IWeb3 web3, string contractAddress) : base(web3, contractAddress)
        {
        }

    }


    public partial class CondoNetServiceBase: ContractWeb3ServiceBase
    {

        public CondoNetServiceBase(Nethereum.Web3.IWeb3 web3, string contractAddress) : base(web3, contractAddress)
        {
        }

        public virtual Task<string> ActualizarConfiguracionRequestAsync(ActualizarConfiguracionFunction actualizarConfiguracionFunction)
        {
             return ContractHandler.SendRequestAsync(actualizarConfiguracionFunction);
        }

        public virtual Task<TransactionReceipt> ActualizarConfiguracionRequestAndWaitForReceiptAsync(ActualizarConfiguracionFunction actualizarConfiguracionFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(actualizarConfiguracionFunction, cancellationToken);
        }

        public virtual Task<string> ActualizarConfiguracionRequestAsync(string campo, string valor)
        {
            var actualizarConfiguracionFunction = new ActualizarConfiguracionFunction();
                actualizarConfiguracionFunction.Campo = campo;
                actualizarConfiguracionFunction.Valor = valor;
            
             return ContractHandler.SendRequestAsync(actualizarConfiguracionFunction);
        }

        public virtual Task<TransactionReceipt> ActualizarConfiguracionRequestAndWaitForReceiptAsync(string campo, string valor, CancellationTokenSource cancellationToken = null)
        {
            var actualizarConfiguracionFunction = new ActualizarConfiguracionFunction();
                actualizarConfiguracionFunction.Campo = campo;
                actualizarConfiguracionFunction.Valor = valor;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(actualizarConfiguracionFunction, cancellationToken);
        }

        public Task<string> AdminQueryAsync(AdminFunction adminFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AdminFunction, string>(adminFunction, blockParameter);
        }

        
        public virtual Task<string> AdminQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AdminFunction, string>(null, blockParameter);
        }

        public virtual Task<string> AnchorMerkleRootRequestAsync(AnchorMerkleRootFunction anchorMerkleRootFunction)
        {
             return ContractHandler.SendRequestAsync(anchorMerkleRootFunction);
        }

        public virtual Task<TransactionReceipt> AnchorMerkleRootRequestAndWaitForReceiptAsync(AnchorMerkleRootFunction anchorMerkleRootFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(anchorMerkleRootFunction, cancellationToken);
        }

        public virtual Task<string> AnchorMerkleRootRequestAsync(string merkleRoot, BigInteger year, BigInteger month)
        {
            var anchorMerkleRootFunction = new AnchorMerkleRootFunction();
                anchorMerkleRootFunction.MerkleRoot = merkleRoot;
                anchorMerkleRootFunction.Year = year;
                anchorMerkleRootFunction.Month = month;
            
             return ContractHandler.SendRequestAsync(anchorMerkleRootFunction);
        }

        public virtual Task<TransactionReceipt> AnchorMerkleRootRequestAndWaitForReceiptAsync(string merkleRoot, BigInteger year, BigInteger month, CancellationTokenSource cancellationToken = null)
        {
            var anchorMerkleRootFunction = new AnchorMerkleRootFunction();
                anchorMerkleRootFunction.MerkleRoot = merkleRoot;
                anchorMerkleRootFunction.Year = year;
                anchorMerkleRootFunction.Month = month;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(anchorMerkleRootFunction, cancellationToken);
        }

        public virtual Task<string> AnclarGastoRequestAsync(AnclarGastoFunction anclarGastoFunction)
        {
             return ContractHandler.SendRequestAsync(anclarGastoFunction);
        }

        public virtual Task<TransactionReceipt> AnclarGastoRequestAndWaitForReceiptAsync(AnclarGastoFunction anclarGastoFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(anclarGastoFunction, cancellationToken);
        }

        public virtual Task<string> AnclarGastoRequestAsync(string expenseId)
        {
            var anclarGastoFunction = new AnclarGastoFunction();
                anclarGastoFunction.ExpenseId = expenseId;
            
             return ContractHandler.SendRequestAsync(anclarGastoFunction);
        }

        public virtual Task<TransactionReceipt> AnclarGastoRequestAndWaitForReceiptAsync(string expenseId, CancellationTokenSource cancellationToken = null)
        {
            var anclarGastoFunction = new AnclarGastoFunction();
                anclarGastoFunction.ExpenseId = expenseId;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(anclarGastoFunction, cancellationToken);
        }

        public virtual Task<string> CertificarPagoRequestAsync(CertificarPagoFunction certificarPagoFunction)
        {
             return ContractHandler.SendRequestAsync(certificarPagoFunction);
        }

        public virtual Task<TransactionReceipt> CertificarPagoRequestAndWaitForReceiptAsync(CertificarPagoFunction certificarPagoFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(certificarPagoFunction, cancellationToken);
        }

        public virtual Task<string> CertificarPagoRequestAsync(string referencia)
        {
            var certificarPagoFunction = new CertificarPagoFunction();
                certificarPagoFunction.Referencia = referencia;
            
             return ContractHandler.SendRequestAsync(certificarPagoFunction);
        }

        public virtual Task<TransactionReceipt> CertificarPagoRequestAndWaitForReceiptAsync(string referencia, CancellationTokenSource cancellationToken = null)
        {
            var certificarPagoFunction = new CertificarPagoFunction();
                certificarPagoFunction.Referencia = referencia;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(certificarPagoFunction, cancellationToken);
        }

        public Task<string> ConfiguracionesQueryAsync(ConfiguracionesFunction configuracionesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ConfiguracionesFunction, string>(configuracionesFunction, blockParameter);
        }

        
        public virtual Task<string> ConfiguracionesQueryAsync(string returnValue1, BlockParameter blockParameter = null)
        {
            var configuracionesFunction = new ConfiguracionesFunction();
                configuracionesFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryAsync<ConfiguracionesFunction, string>(configuracionesFunction, blockParameter);
        }

        public virtual Task<GastosOutputDTO> GastosQueryAsync(GastosFunction gastosFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GastosFunction, GastosOutputDTO>(gastosFunction, blockParameter);
        }

        public virtual Task<GastosOutputDTO> GastosQueryAsync(string returnValue1, BlockParameter blockParameter = null)
        {
            var gastosFunction = new GastosFunction();
                gastosFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryDeserializingToObjectAsync<GastosFunction, GastosOutputDTO>(gastosFunction, blockParameter);
        }

        public Task<string> GetConfiguracionQueryAsync(GetConfiguracionFunction getConfiguracionFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetConfiguracionFunction, string>(getConfiguracionFunction, blockParameter);
        }

        
        public virtual Task<string> GetConfiguracionQueryAsync(string campo, BlockParameter blockParameter = null)
        {
            var getConfiguracionFunction = new GetConfiguracionFunction();
                getConfiguracionFunction.Campo = campo;
            
            return ContractHandler.QueryAsync<GetConfiguracionFunction, string>(getConfiguracionFunction, blockParameter);
        }

        public Task<string> GetMerkleRootQueryAsync(GetMerkleRootFunction getMerkleRootFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetMerkleRootFunction, string>(getMerkleRootFunction, blockParameter);
        }

        
        public virtual Task<string> GetMerkleRootQueryAsync(BigInteger year, BigInteger month, BlockParameter blockParameter = null)
        {
            var getMerkleRootFunction = new GetMerkleRootFunction();
                getMerkleRootFunction.Year = year;
                getMerkleRootFunction.Month = month;
            
            return ContractHandler.QueryAsync<GetMerkleRootFunction, string>(getMerkleRootFunction, blockParameter);
        }

        public Task<string> MerkleRootsQueryAsync(MerkleRootsFunction merkleRootsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MerkleRootsFunction, string>(merkleRootsFunction, blockParameter);
        }

        
        public virtual Task<string> MerkleRootsQueryAsync(BigInteger returnValue1, BigInteger returnValue2, BlockParameter blockParameter = null)
        {
            var merkleRootsFunction = new MerkleRootsFunction();
                merkleRootsFunction.ReturnValue1 = returnValue1;
                merkleRootsFunction.ReturnValue2 = returnValue2;
            
            return ContractHandler.QueryAsync<MerkleRootsFunction, string>(merkleRootsFunction, blockParameter);
        }

        public virtual Task<string> ModificarGastoRequestAsync(ModificarGastoFunction modificarGastoFunction)
        {
             return ContractHandler.SendRequestAsync(modificarGastoFunction);
        }

        public virtual Task<TransactionReceipt> ModificarGastoRequestAndWaitForReceiptAsync(ModificarGastoFunction modificarGastoFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(modificarGastoFunction, cancellationToken);
        }

        public virtual Task<string> ModificarGastoRequestAsync(string expenseId, string descripcion, BigInteger monto, BigInteger fecha)
        {
            var modificarGastoFunction = new ModificarGastoFunction();
                modificarGastoFunction.ExpenseId = expenseId;
                modificarGastoFunction.Descripcion = descripcion;
                modificarGastoFunction.Monto = monto;
                modificarGastoFunction.Fecha = fecha;
            
             return ContractHandler.SendRequestAsync(modificarGastoFunction);
        }

        public virtual Task<TransactionReceipt> ModificarGastoRequestAndWaitForReceiptAsync(string expenseId, string descripcion, BigInteger monto, BigInteger fecha, CancellationTokenSource cancellationToken = null)
        {
            var modificarGastoFunction = new ModificarGastoFunction();
                modificarGastoFunction.ExpenseId = expenseId;
                modificarGastoFunction.Descripcion = descripcion;
                modificarGastoFunction.Monto = monto;
                modificarGastoFunction.Fecha = fecha;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(modificarGastoFunction, cancellationToken);
        }

        public virtual Task<PagosOutputDTO> PagosQueryAsync(PagosFunction pagosFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<PagosFunction, PagosOutputDTO>(pagosFunction, blockParameter);
        }

        public virtual Task<PagosOutputDTO> PagosQueryAsync(string returnValue1, BlockParameter blockParameter = null)
        {
            var pagosFunction = new PagosFunction();
                pagosFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryDeserializingToObjectAsync<PagosFunction, PagosOutputDTO>(pagosFunction, blockParameter);
        }

        public virtual Task<string> RegistrarPagoRequestAsync(RegistrarPagoFunction registrarPagoFunction)
        {
             return ContractHandler.SendRequestAsync(registrarPagoFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarPagoRequestAndWaitForReceiptAsync(RegistrarPagoFunction registrarPagoFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarPagoFunction, cancellationToken);
        }

        public virtual Task<string> RegistrarPagoRequestAsync(string unitId, BigInteger monto, string referencia)
        {
            var registrarPagoFunction = new RegistrarPagoFunction();
                registrarPagoFunction.UnitId = unitId;
                registrarPagoFunction.Monto = monto;
                registrarPagoFunction.Referencia = referencia;
            
             return ContractHandler.SendRequestAsync(registrarPagoFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarPagoRequestAndWaitForReceiptAsync(string unitId, BigInteger monto, string referencia, CancellationTokenSource cancellationToken = null)
        {
            var registrarPagoFunction = new RegistrarPagoFunction();
                registrarPagoFunction.UnitId = unitId;
                registrarPagoFunction.Monto = monto;
                registrarPagoFunction.Referencia = referencia;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarPagoFunction, cancellationToken);
        }

        public virtual Task<string> RegistrarSplitRequestAsync(RegistrarSplitFunction registrarSplitFunction)
        {
             return ContractHandler.SendRequestAsync(registrarSplitFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarSplitRequestAndWaitForReceiptAsync(RegistrarSplitFunction registrarSplitFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarSplitFunction, cancellationToken);
        }

        public virtual Task<string> RegistrarSplitRequestAsync(string unitId, BigInteger montoOperativo, BigInteger montoReserva)
        {
            var registrarSplitFunction = new RegistrarSplitFunction();
                registrarSplitFunction.UnitId = unitId;
                registrarSplitFunction.MontoOperativo = montoOperativo;
                registrarSplitFunction.MontoReserva = montoReserva;
            
             return ContractHandler.SendRequestAsync(registrarSplitFunction);
        }

        public virtual Task<TransactionReceipt> RegistrarSplitRequestAndWaitForReceiptAsync(string unitId, BigInteger montoOperativo, BigInteger montoReserva, CancellationTokenSource cancellationToken = null)
        {
            var registrarSplitFunction = new RegistrarSplitFunction();
                registrarSplitFunction.UnitId = unitId;
                registrarSplitFunction.MontoOperativo = montoOperativo;
                registrarSplitFunction.MontoReserva = montoReserva;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(registrarSplitFunction, cancellationToken);
        }

        public virtual Task<string> RevertirPagoRequestAsync(RevertirPagoFunction revertirPagoFunction)
        {
             return ContractHandler.SendRequestAsync(revertirPagoFunction);
        }

        public virtual Task<TransactionReceipt> RevertirPagoRequestAndWaitForReceiptAsync(RevertirPagoFunction revertirPagoFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(revertirPagoFunction, cancellationToken);
        }

        public virtual Task<string> RevertirPagoRequestAsync(string unitId, BigInteger monto, string referencia)
        {
            var revertirPagoFunction = new RevertirPagoFunction();
                revertirPagoFunction.UnitId = unitId;
                revertirPagoFunction.Monto = monto;
                revertirPagoFunction.Referencia = referencia;
            
             return ContractHandler.SendRequestAsync(revertirPagoFunction);
        }

        public virtual Task<TransactionReceipt> RevertirPagoRequestAndWaitForReceiptAsync(string unitId, BigInteger monto, string referencia, CancellationTokenSource cancellationToken = null)
        {
            var revertirPagoFunction = new RevertirPagoFunction();
                revertirPagoFunction.UnitId = unitId;
                revertirPagoFunction.Monto = monto;
                revertirPagoFunction.Referencia = referencia;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(revertirPagoFunction, cancellationToken);
        }

        public override List<Type> GetAllFunctionTypes()
        {
            return new List<Type>
            {
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
            };
        }

        public override List<Type> GetAllEventTypes()
        {
            return new List<Type>
            {
                typeof(ConfiguracionActualizadaEventDTO),
                typeof(GastoModificadoEventDTO),
                typeof(MerkleRootAncladoEventDTO),
                typeof(PagoRegistradoEventDTO),
                typeof(PagoRevertidoEventDTO),
                typeof(SplitRegistradoEventDTO)
            };
        }

        public override List<Type> GetAllErrorTypes()
        {
            return new List<Type>
            {

            };
        }
    }
}
