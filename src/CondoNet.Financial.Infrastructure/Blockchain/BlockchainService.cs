using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace CondoNet.Financial.Infrastructure.Blockchain
{
    public class BlockchainSettings
    {
        public string RpcUrl { get; set; } = string.Empty;
        public string ContractAddress { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
    }


    public interface IBlockchainService
    {
        Web3 Web3Instance { get; }
        Task<string> GetBlockNumberAsync(CancellationToken cancellationToken = default);
    }

    public class BlockchainService : IBlockchainService
    {
        private readonly ILogger<BlockchainService> _logger;
        private readonly BlockchainSettings _settings;
        private readonly Web3 _web3;

        public BlockchainService(ILogger<BlockchainService> logger, IOptions<BlockchainSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
            var account = new Account(_settings.PrivateKey);
            _web3 = new Web3(account, _settings.RpcUrl);
        }

        public Web3 Web3Instance => _web3;

        public async Task<string> GetBlockNumberAsync(CancellationToken cancellationToken = default)
        {
            var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync(cancellationToken);
            _logger.LogInformation($"Block number: {blockNumber.Value}");
            return blockNumber.Value.ToString();
        }
    }
}
