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

        public virtual Task<string> GuardarMensajeRequestAsync(GuardarMensajeFunction guardarMensajeFunction)
        {
             return ContractHandler.SendRequestAsync(guardarMensajeFunction);
        }

        public virtual Task<TransactionReceipt> GuardarMensajeRequestAndWaitForReceiptAsync(GuardarMensajeFunction guardarMensajeFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(guardarMensajeFunction, cancellationToken);
        }

        public virtual Task<string> GuardarMensajeRequestAsync(string nuevoMensaje)
        {
            var guardarMensajeFunction = new GuardarMensajeFunction();
                guardarMensajeFunction.NuevoMensaje = nuevoMensaje;
            
             return ContractHandler.SendRequestAsync(guardarMensajeFunction);
        }

        public virtual Task<TransactionReceipt> GuardarMensajeRequestAndWaitForReceiptAsync(string nuevoMensaje, CancellationTokenSource cancellationToken = null)
        {
            var guardarMensajeFunction = new GuardarMensajeFunction();
                guardarMensajeFunction.NuevoMensaje = nuevoMensaje;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(guardarMensajeFunction, cancellationToken);
        }

        public Task<string> LeerMensajeQueryAsync(LeerMensajeFunction leerMensajeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<LeerMensajeFunction, string>(leerMensajeFunction, blockParameter);
        }

        
        public virtual Task<string> LeerMensajeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<LeerMensajeFunction, string>(null, blockParameter);
        }

        public Task<string> OwnerQueryAsync(OwnerFunction ownerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(ownerFunction, blockParameter);
        }

        
        public virtual Task<string> OwnerQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(null, blockParameter);
        }

        public override List<Type> GetAllFunctionTypes()
        {
            return new List<Type>
            {
                typeof(GuardarMensajeFunction),
                typeof(LeerMensajeFunction),
                typeof(OwnerFunction)
            };
        }

        public override List<Type> GetAllEventTypes()
        {
            return new List<Type>
            {

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
