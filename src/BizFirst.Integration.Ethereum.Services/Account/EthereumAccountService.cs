namespace BizFirst.Integration.Ethereum.Services;

using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;

/// <summary>Account resource: balance, nonce, transaction history, and contract-vs-wallet detection.</summary>
public sealed class EthereumAccountService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumAccountService> _logger;

    public EthereumAccountService(EthereumConnectionProvider connectionProvider, ILogger<EthereumAccountService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>AC01 — account/balance.</summary>
    public async Task<EthereumAccountBalanceResult> GetBalanceAsync(
        string? network, string address, string unit, string? blockTag, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var block = ParseBlockParameter(blockTag);
            var balanceWei = await web3.Eth.GetBalance.SendRequestAsync(address, block).ConfigureAwait(false);

            var formatted = ConvertFromWei(balanceWei.Value, unit);
            return EthereumAccountBalanceResult.Ok(balanceWei.Value.ToString(), formatted, NormalizeUnit(unit));
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumAccountBalanceResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "account/balance RPC error for {Address} on {Network}", address, network);
            return EthereumAccountBalanceResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "account/balance unexpected error for {Address} on {Network}", address, network);
            return EthereumAccountBalanceResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>AC02 — account/nonce (eth_getTransactionCount).</summary>
    public async Task<EthereumAccountNonceResult> GetNonceAsync(
        string? network, string address, string? blockTag, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var block = ParseBlockParameter(blockTag);
            var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address, block).ConfigureAwait(false);
            return EthereumAccountNonceResult.Ok((long)nonce.Value);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumAccountNonceResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "account/nonce RPC error for {Address} on {Network}", address, network);
            return EthereumAccountNonceResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "account/nonce unexpected error for {Address} on {Network}", address, network);
            return EthereumAccountNonceResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// AC03 — account/history. NOT a JSON-RPC primitive — Ethereum nodes do not index transactions by
    /// address, so this requires a block-explorer indexer API (Etherscan-compatible) that isn't wired
    /// up in this pass. Fails clearly rather than silently returning an empty/wrong result.
    /// </summary>
    public Task<EthereumAccountHistoryResult> GetHistoryAsync(
        string? network, string address, long? fromBlock, long? toBlock, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("account/history requested for {Address} but no indexer API is configured.", address);
        return Task.FromResult(EthereumAccountHistoryResult.Fail(
            "INDEXER_NOT_CONFIGURED",
            "account/history requires an Etherscan-compatible block-explorer indexer API, which is not configured for this node. " +
            "Standard Ethereum JSON-RPC has no method to list an address's past transactions."));
    }

    /// <summary>AC04 — account/code (eth_getCode). Detects contract vs. wallet (EOA) addresses.</summary>
    public async Task<EthereumAccountCodeResult> GetCodeAsync(
        string? network, string address, string? blockTag, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var block = ParseBlockParameter(blockTag);
            var code = await web3.Eth.GetCode.SendRequestAsync(address, block).ConfigureAwait(false);
            return EthereumAccountCodeResult.Ok(code ?? "0x");
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumAccountCodeResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "account/code RPC error for {Address} on {Network}", address, network);
            return EthereumAccountCodeResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "account/code unexpected error for {Address} on {Network}", address, network);
            return EthereumAccountCodeResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    internal static BlockParameter ParseBlockParameter(string? blockTag) =>
        string.IsNullOrWhiteSpace(blockTag) || blockTag.Equals("latest", StringComparison.OrdinalIgnoreCase)
            ? BlockParameter.CreateLatest()
            : blockTag.Equals("pending", StringComparison.OrdinalIgnoreCase)
                ? BlockParameter.CreatePending()
                : blockTag.Equals("earliest", StringComparison.OrdinalIgnoreCase)
                    ? BlockParameter.CreateEarliest()
                    : new BlockParameter(new Nethereum.Hex.HexTypes.HexBigInteger(BigInteger.Parse(blockTag)));

    internal static decimal ConvertFromWei(BigInteger wei, string unit) => NormalizeUnit(unit) switch
    {
        "gwei" => UnitConversion.Convert.FromWei(wei, UnitConversion.EthUnit.Gwei),
        "ether" => UnitConversion.Convert.FromWei(wei, UnitConversion.EthUnit.Ether),
        _ => (decimal)wei,
    };

    internal static string NormalizeUnit(string? unit) =>
        string.IsNullOrWhiteSpace(unit) ? "wei" : unit.Trim().ToLowerInvariant();
}
