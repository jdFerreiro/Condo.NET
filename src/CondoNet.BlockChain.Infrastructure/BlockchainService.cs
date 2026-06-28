using CondoNet.BlockChain.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

namespace CondoNet.BlockChain.Infrastructure;

public class BlockchainService : IBlockchainService
{
    private readonly ILogger<BlockchainService> _logger;
    private readonly BlockchainSettings _settings;
    private readonly Web3 _web3;

    public BlockchainService(ILogger<BlockchainService> logger, IOptions<BlockchainSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
        var account = new Nethereum.Web3.Accounts.Account(_settings.PrivateKey);
        _web3 = new Web3(account, _settings.RpcUrl);
    }

    public Web3 Web3Instance => _web3;

    public async Task<string> GetBlockNumberAsync(CancellationToken cancellationToken = default)
    {
        var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync(cancellationToken);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Current block number: {blockNumber}", blockNumber.Value);
        }
        return blockNumber.Value.ToString();
    }
}
