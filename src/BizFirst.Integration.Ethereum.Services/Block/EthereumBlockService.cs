namespace BizFirst.Integration.Ethereum.Services;

using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;

/// <summary>Block resource: single-block lookup, latest height, and a composite recent-blocks listing.</summary>
public sealed class EthereumBlockService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumBlockService> _logger;

    public EthereumBlockService(EthereumConnectionProvider connectionProvider, ILogger<EthereumBlockService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>BL01 — block/get.</summary>
    public async Task<EthereumBlockGetResult> GetBlockAsync(
        string? network, string? blockNumberOrHash, bool includeTransactions, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var block = await FetchBlockAsync(web3, blockNumberOrHash, includeTransactions).ConfigureAwait(false);
            if (block is null)
                return EthereumBlockGetResult.Fail("BLOCK_NOT_FOUND", $"Block '{blockNumberOrHash ?? "latest"}' was not found.");

            return EthereumBlockGetResult.Ok(block);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumBlockGetResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "block/get RPC error for {Block} on {Network}", blockNumberOrHash, network);
            return EthereumBlockGetResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "block/get unexpected error for {Block} on {Network}", blockNumberOrHash, network);
            return EthereumBlockGetResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>BL02 — block/number (eth_blockNumber).</summary>
    public async Task<EthereumBlockNumberResult> GetBlockNumberAsync(string? network, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var number = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().ConfigureAwait(false);
            return EthereumBlockNumberResult.Ok((long)number.Value);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumBlockNumberResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "block/number RPC error on {Network}", network);
            return EthereumBlockNumberResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "block/number unexpected error on {Network}", network);
            return EthereumBlockNumberResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// BL03 — block/list. Composite operation: not a single RPC primitive, fans out to N sequential
    /// GetBlockAsync-equivalent calls starting at <paramref name="fromBlock"/> (or the current head).
    /// </summary>
    public async Task<EthereumBlockListResult> ListRecentBlocksAsync(
        string? network, int count, long? fromBlock, CancellationToken cancellationToken = default)
    {
        if (count <= 0 || count > 100)
            return EthereumBlockListResult.Fail("VAL_INVALID_COUNT", "count must be between 1 and 100.");

        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var head = fromBlock ?? (long)(await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().ConfigureAwait(false)).Value;

            var blocks = new List<EthereumBlockSummary>(count);
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var blockNumber = head - i;
                if (blockNumber < 0) break;

                var block = await FetchBlockAsync(web3, blockNumber.ToString(), includeTransactions: false).ConfigureAwait(false);
                if (block is not null) blocks.Add(block);
            }

            return EthereumBlockListResult.Ok(blocks);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumBlockListResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "block/list RPC error on {Network}", network);
            return EthereumBlockListResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "block/list unexpected error on {Network}", network);
            return EthereumBlockListResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    private static async Task<EthereumBlockSummary?> FetchBlockAsync(IWeb3 web3, string? blockNumberOrHash, bool includeTransactions)
    {
        BlockWithTransactionHashes? block;
        if (string.IsNullOrWhiteSpace(blockNumberOrHash) || blockNumberOrHash.Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            block = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                .SendRequestAsync(EthereumAccountService.ParseBlockParameter("latest")).ConfigureAwait(false);
        }
        else if (blockNumberOrHash.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && blockNumberOrHash.Length == 66)
        {
            block = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByHash.SendRequestAsync(blockNumberOrHash).ConfigureAwait(false);
        }
        else
        {
            block = await web3.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                .SendRequestAsync(EthereumAccountService.ParseBlockParameter(blockNumberOrHash)).ConfigureAwait(false);
        }

        if (block is null) return null;

        return new EthereumBlockSummary(
            Number: (long)block.Number.Value,
            Hash: block.BlockHash,
            ParentHash: block.ParentHash,
            TimestampUnix: (long)block.Timestamp.Value,
            Miner: block.Miner,
            GasLimit: block.GasLimit.Value.ToString(),
            GasUsed: block.GasUsed.Value.ToString(),
            TransactionCount: block.TransactionHashes?.Length ?? 0,
            TransactionHashes: includeTransactions ? block.TransactionHashes : null);
    }
}
