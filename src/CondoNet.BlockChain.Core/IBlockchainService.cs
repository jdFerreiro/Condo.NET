using Nethereum.Web3;
using System.Threading;
using System.Threading.Tasks;

namespace CondoNet.BlockChain.Core;

public interface IBlockchainService
{
    Web3 Web3Instance { get; }
    Task<string> GetBlockNumberAsync(CancellationToken cancellationToken = default);
}
