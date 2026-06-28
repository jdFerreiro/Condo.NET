using Nethereum.Web3;

namespace CondoNet.BlockChain.Core;

public interface IBlockchainService
{
    Web3 Web3Instance { get; }
    Task<string> GetBlockNumberAsync(CancellationToken cancellationToken = default);
}
