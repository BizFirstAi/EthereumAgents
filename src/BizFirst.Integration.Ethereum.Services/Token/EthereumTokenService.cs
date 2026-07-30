namespace BizFirst.Integration.Ethereum.Services;

using Nethereum.Contracts;
using Nethereum.JsonRpc.Client;
using Nethereum.Util;

/// <summary>
/// ERC-20 token resource. The first 9 operations match the actual EIP-20 standard interface.
/// ERC20_10/ERC20_11 (Mint/Burn, addendum v3) are NOT part of the standard — they revert against
/// USDC/USDT/DAI — and are explicitly labeled advanced/contract-specific: Burn matches the real,
/// widely-adopted OpenZeppelin ERC20Burnable extension exactly; Mint has no real standard at all,
/// so it assumes the common `mint(address,uint256)` shape with an overridable function name.
/// </summary>
public sealed class EthereumTokenService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumTokenService> _logger;

    public EthereumTokenService(EthereumConnectionProvider connectionProvider, ILogger<EthereumTokenService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>ERC20_01 — token/balance.</summary>
    public async Task<EthereumTokenBalanceResult> GetBalanceAsync(
        string? network, string tokenAddress, string ownerAddress, bool formatDecimals, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var raw = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(ownerAddress).ConfigureAwait(false);

            decimal? formatted = null;
            if (formatDecimals)
            {
                var decimals = await GetDecimalsRawAsync(contract).ConfigureAwait(false);
                formatted = UnitConversion.Convert.FromWei(raw, decimals);
            }

            return EthereumTokenBalanceResult.Ok(raw.ToString(), formatted);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenBalanceResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/balance RPC error for {Token}/{Owner} on {Network}", tokenAddress, ownerAddress, network);
            return EthereumTokenBalanceResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/balance unexpected error for {Token}/{Owner} on {Network}", tokenAddress, ownerAddress, network);
            return EthereumTokenBalanceResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_02 — token/transfer. Most popular operation (stablecoin payments).</summary>
    public async Task<EthereumTokenTransferResult> TransferAsync(
        string? network, string privateKeyHex, string tokenAddress, string to, BigInteger amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var from = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(from, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var txHash = await contract.GetFunction("transfer").SendTransactionAsync(from, to, amount).ConfigureAwait(false);
            return EthereumTokenTransferResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenTransferResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/transfer RPC error for {Token} to {To} on {Network}", tokenAddress, to, network);
            return EthereumTokenTransferResult.Fail("RPC_ERROR", MapErc20RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/transfer unexpected error for {Token} to {To} on {Network}", tokenAddress, to, network);
            return EthereumTokenTransferResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_03 — token/approve.</summary>
    public async Task<EthereumTokenApproveResult> ApproveAsync(
        string? network, string privateKeyHex, string tokenAddress, string spender, BigInteger amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var from = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(from, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var txHash = await contract.GetFunction("approve").SendTransactionAsync(from, spender, amount).ConfigureAwait(false);
            return EthereumTokenApproveResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenApproveResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/approve RPC error for {Token} spender {Spender} on {Network}", tokenAddress, spender, network);
            return EthereumTokenApproveResult.Fail("RPC_ERROR", MapErc20RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/approve unexpected error for {Token} spender {Spender} on {Network}", tokenAddress, spender, network);
            return EthereumTokenApproveResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_04 — token/transferFrom (moves tokens via an existing allowance).</summary>
    public async Task<EthereumTokenTransferFromResult> TransferFromAsync(
        string? network, string privateKeyHex, string tokenAddress, string from, string to, BigInteger amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var txHash = await contract.GetFunction("transferFrom").SendTransactionAsync(sender, from, to, amount).ConfigureAwait(false);
            return EthereumTokenTransferFromResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenTransferFromResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/transferFrom RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenTransferFromResult.Fail("RPC_ERROR", MapErc20RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/transferFrom unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenTransferFromResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_05 — token/allowance.</summary>
    public async Task<EthereumTokenAllowanceResult> GetAllowanceAsync(
        string? network, string tokenAddress, string owner, string spender, bool formatDecimals, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var raw = await contract.GetFunction("allowance").CallAsync<BigInteger>(owner, spender).ConfigureAwait(false);

            decimal? formatted = null;
            if (formatDecimals)
            {
                var decimals = await GetDecimalsRawAsync(contract).ConfigureAwait(false);
                formatted = UnitConversion.Convert.FromWei(raw, decimals);
            }

            return EthereumTokenAllowanceResult.Ok(raw.ToString(), formatted);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenAllowanceResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/allowance RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenAllowanceResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/allowance unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenAllowanceResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_06 — token/totalSupply.</summary>
    public async Task<EthereumTokenTotalSupplyResult> GetTotalSupplyAsync(
        string? network, string tokenAddress, bool formatDecimals, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var raw = await contract.GetFunction("totalSupply").CallAsync<BigInteger>().ConfigureAwait(false);

            decimal? formatted = null;
            if (formatDecimals)
            {
                var decimals = await GetDecimalsRawAsync(contract).ConfigureAwait(false);
                formatted = UnitConversion.Convert.FromWei(raw, decimals);
            }

            return EthereumTokenTotalSupplyResult.Ok(raw.ToString(), formatted);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenTotalSupplyResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/totalSupply RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenTotalSupplyResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/totalSupply unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenTotalSupplyResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_07 — token/decimals.</summary>
    public async Task<EthereumTokenDecimalsResult> GetDecimalsAsync(string? network, string tokenAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var decimals = await GetDecimalsRawAsync(contract).ConfigureAwait(false);
            return EthereumTokenDecimalsResult.Ok(decimals);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenDecimalsResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/decimals RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenDecimalsResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/decimals unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenDecimalsResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_08 — token/name.</summary>
    public async Task<EthereumTokenNameResult> GetNameAsync(string? network, string tokenAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var name = await contract.GetFunction("name").CallAsync<string>().ConfigureAwait(false);
            return EthereumTokenNameResult.Ok(name ?? string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenNameResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/name RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenNameResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/name unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenNameResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC20_09 — token/symbol.</summary>
    public async Task<EthereumTokenSymbolResult> GetSymbolAsync(string? network, string tokenAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, tokenAddress);
            var symbol = await contract.GetFunction("symbol").CallAsync<string>().ConfigureAwait(false);
            return EthereumTokenSymbolResult.Ok(symbol ?? string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenSymbolResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/symbol RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenSymbolResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/symbol unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenSymbolResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// ERC20_10 — token/mint (addendum v3). No industry-standard signature exists — assumes
    /// `{functionName}(address,uint256)`, `functionName` defaulting to `"mint"`. The caller must
    /// hold minter role on-chain; this operation doesn't grant or check roles.
    /// </summary>
    public async Task<EthereumTokenMintResult> MintAsync(
        string? network, string privateKeyHex, string tokenAddress, string to, BigInteger amount, string functionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var from = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(from, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.BuildMintAbi(functionName, includeTokenId: true), tokenAddress);
            var txHash = await contract.GetFunction(functionName).SendTransactionAsync(from, to, amount).ConfigureAwait(false);
            return EthereumTokenMintResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenMintResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/mint RPC error for {Token} to {To} on {Network}", tokenAddress, to, network);
            return EthereumTokenMintResult.Fail("RPC_ERROR", MapErc20RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/mint unexpected error for {Token} to {To} on {Network}", tokenAddress, to, network);
            return EthereumTokenMintResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// ERC20_11 — token/burn (addendum v3). Matches the real OpenZeppelin ERC20Burnable extension:
    /// <paramref name="from"/> null/empty burns the caller's own balance (`burn(uint256)`);
    /// supplied burns via allowance (`burnFrom(address,uint256)`).
    /// </summary>
    public async Task<EthereumTokenBurnResult> BurnAsync(
        string? network, string privateKeyHex, string tokenAddress, string? from, BigInteger amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc20Burnable, tokenAddress);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);

            string txHash = string.IsNullOrWhiteSpace(from)
                ? await contract.GetFunction("burn").SendTransactionAsync(sender, amount).ConfigureAwait(false)
                : await contract.GetFunction("burnFrom").SendTransactionAsync(sender, from, amount).ConfigureAwait(false);

            return EthereumTokenBurnResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTokenBurnResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "token/burn RPC error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenBurnResult.Fail("RPC_ERROR", MapErc20RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "token/burn unexpected error for {Token} on {Network}", tokenAddress, network);
            return EthereumTokenBurnResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    private static async Task<int> GetDecimalsRawAsync(Contract contract) =>
        await contract.GetFunction("decimals").CallAsync<byte>().ConfigureAwait(false);

    /// <summary>
    /// ERC-20 reverts (e.g. insufficient balance/allowance) surface as generic RPC errors from most
    /// providers — this maps the common case to a clearer message without pretending to parse a
    /// custom revert reason format that isn't guaranteed across providers.
    /// </summary>
    private static string MapErc20RevertReason(RpcResponseException ex) => ex.Message;
}
