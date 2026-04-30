using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;

namespace CondoNet.BlockChain.Infrastructure;

public class CondoNetService
{
    private readonly Web3 _web3;
    private readonly string _contractAddress;
    private readonly ContractHandler _contractHandler;

    public CondoNetService(Web3 web3, string contractAddress)
    {
        _web3 = web3;
        _contractAddress = contractAddress;
        _contractHandler = _web3.Eth.GetContractHandler(_contractAddress);
    }

    public async Task<string> RegistrarPagoRequestAsync(ContractDefinition.RegistrarPagoFunction function)
        => await _contractHandler.SendRequestAsync(function);
}
