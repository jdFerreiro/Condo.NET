using CondoNet.Financial.Infrastructure.Mensaje.ContractDefinition;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace CondoNet.Financial.Infrastructure.Mensaje
{
    public partial class MensajeService(Nethereum.Web3.IWeb3 web3, string contractAddress) : MensajeServiceBase(web3, contractAddress)
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.IWeb3 web3, MensajeDeployment mensajeDeployment, CancellationTokenSource cancellationTokenSource = null!)
        {
            return web3.Eth.GetContractDeploymentHandler<MensajeDeployment>().SendRequestAndWaitForReceiptAsync(mensajeDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.IWeb3 web3, MensajeDeployment mensajeDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<MensajeDeployment>().SendRequestAsync(mensajeDeployment);
        }

        public static async Task<MensajeService> DeployContractAndGetServiceAsync(Nethereum.Web3.IWeb3 web3, MensajeDeployment mensajeDeployment, CancellationTokenSource cancellationTokenSource = null!)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, mensajeDeployment, cancellationTokenSource);
            return new MensajeService(web3, receipt.ContractAddress);
        }
    }


    public partial class MensajeServiceBase(Nethereum.Web3.IWeb3 web3, string contractAddress) : ContractWeb3ServiceBase(web3, contractAddress)
    {
        public virtual Task<string> GuardarMensajeRequestAsync(GuardarMensajeFunction guardarMensajeFunction)
        {
            return ContractHandler.SendRequestAsync(guardarMensajeFunction);
        }

        public virtual Task<TransactionReceipt> GuardarMensajeRequestAndWaitForReceiptAsync(GuardarMensajeFunction guardarMensajeFunction, CancellationTokenSource cancellationToken = null!)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(guardarMensajeFunction, cancellationToken);
        }

        public virtual Task<string> GuardarMensajeRequestAsync(string nuevoMensaje)
        {
            var guardarMensajeFunction = new GuardarMensajeFunction
            {
                NuevoMensaje = nuevoMensaje
            };

            return ContractHandler.SendRequestAsync(guardarMensajeFunction);
        }

        public virtual Task<TransactionReceipt> GuardarMensajeRequestAndWaitForReceiptAsync(string nuevoMensaje, CancellationTokenSource cancellationToken = null!)
        {
            var guardarMensajeFunction = new GuardarMensajeFunction
            {
                NuevoMensaje = nuevoMensaje
            };

            return ContractHandler.SendRequestAndWaitForReceiptAsync(guardarMensajeFunction, cancellationToken);
        }

        public Task<string> LeerMensajeQueryAsync(LeerMensajeFunction leerMensajeFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<LeerMensajeFunction, string>(leerMensajeFunction, blockParameter);
        }


        public virtual Task<string> LeerMensajeQueryAsync(BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<LeerMensajeFunction, string>(null!, blockParameter);
        }

        public Task<string> OwnerQueryAsync(OwnerFunction ownerFunction, BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(ownerFunction, blockParameter);
        }


        public virtual Task<string> OwnerQueryAsync(BlockParameter blockParameter = null!)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(null!, blockParameter);
        }

        public override List<Type> GetAllFunctionTypes()
        {
            return
            [
                typeof(GuardarMensajeFunction),
                typeof(LeerMensajeFunction),
                typeof(OwnerFunction)
            ];
        }

        public override List<Type> GetAllEventTypes()
        {
            return [];
        }

        public override List<Type> GetAllErrorTypes()
        {
            return [];
        }
    }
}
