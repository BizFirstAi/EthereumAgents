namespace BizFirst.Integration.Ethereum.Services;

using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;

/// <summary>Transaction resource: read, send, receipt, pending, decode, and wait-for-confirmation.</summary>
public sealed class EthereumTransactionService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumTransactionService> _logger;

    public EthereumTransactionService(EthereumConnectionProvider connectionProvider, ILogger<EthereumTransactionService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>TX01 — transaction/get.</summary>
    public async Task<EthereumTransactionGetResult> GetTransactionAsync(string? network, string hash, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash).ConfigureAwait(false);
            if (tx is null)
                return EthereumTransactionGetResult.Fail("TRANSACTION_NOT_FOUND", $"Transaction '{hash}' was not found.");

            return EthereumTransactionGetResult.Ok(MapTransaction(tx));
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTransactionGetResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "transaction/get RPC error for {Hash} on {Network}", hash, network);
            return EthereumTransactionGetResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "transaction/get unexpected error for {Hash} on {Network}", hash, network);
            return EthereumTransactionGetResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>TX02 — transaction/send. Core operation. Requires a signing private key resolved by the caller.</summary>
    public async Task<EthereumTransactionSendResult> SendTransactionAsync(
        string? network, string privateKeyHex, string to, BigInteger valueWei, string? data,
        BigInteger? gasLimit, BigInteger? maxFeePerGasWei, BigInteger? maxPriorityFeePerGasWei, long? nonce,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var from = web3.TransactionManager.Account.Address;

            // Held across nonce-fetch (implicit, inside SendTransactionAsync when Nonce is null) and
            // send — prevents two concurrent signers for this address from racing the same "pending" nonce.
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(from, cancellationToken).ConfigureAwait(false);

            var input = new TransactionInput
            {
                From = from,
                To = to,
                Value = new HexBigInteger(valueWei),
                Data = string.IsNullOrWhiteSpace(data) ? null : data,
                Gas = gasLimit.HasValue ? new HexBigInteger(gasLimit.Value) : null,
                MaxFeePerGas = maxFeePerGasWei.HasValue ? new HexBigInteger(maxFeePerGasWei.Value) : null,
                MaxPriorityFeePerGas = maxPriorityFeePerGasWei.HasValue ? new HexBigInteger(maxPriorityFeePerGasWei.Value) : null,
                Nonce = nonce.HasValue ? new HexBigInteger(nonce.Value) : null,
            };

            var txHash = await web3.Eth.TransactionManager.SendTransactionAsync(input).ConfigureAwait(false);
            return EthereumTransactionSendResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTransactionSendResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "transaction/send RPC error to {To} on {Network}", to, network);
            return EthereumTransactionSendResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "transaction/send unexpected error to {To} on {Network}", to, network);
            return EthereumTransactionSendResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>TX03 — transaction/receipt (eth_getTransactionReceipt). Real success/revert status, logs, gas used.</summary>
    public async Task<EthereumTransactionReceiptResult> GetReceiptAsync(string? network, string hash, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash).ConfigureAwait(false);
            if (receipt is null)
                return EthereumTransactionReceiptResult.Fail("RECEIPT_NOT_FOUND", $"No receipt for '{hash}' yet — the transaction may still be pending.");

            var logs = (receipt.Logs is null)
                ? Array.Empty<EthereumEventLogEntry>()
                : MapLogs(receipt);

            return EthereumTransactionReceiptResult.Ok(
                receipt.TransactionHash,
                receipt.Status is null ? null : receipt.Status.Value == 1,
                receipt.BlockNumber is null ? null : (long)receipt.BlockNumber.Value,
                receipt.GasUsed?.Value.ToString() ?? "0",
                logs);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTransactionReceiptResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "transaction/receipt RPC error for {Hash} on {Network}", hash, network);
            return EthereumTransactionReceiptResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "transaction/receipt unexpected error for {Hash} on {Network}", hash, network);
            return EthereumTransactionReceiptResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// TX04 — transaction/pending. Mempool visibility is provider-dependent — most public RPC
    /// providers (Alchemy/Infura) do not expose txpool_content/eth_pendingTransactions; this fails
    /// clearly with NOT_SUPPORTED rather than silently returning an empty list on unsupported providers.
    /// </summary>
    public async Task<EthereumTransactionPendingResult> GetPendingTransactionsAsync(
        string? network, string? addressFilter, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var raw = await web3.Client.SendRequestAsync<Transaction[]>("eth_pendingTransactions").ConfigureAwait(false);
            var transactions = (raw ?? Array.Empty<Transaction>())
                .Where(t => string.IsNullOrWhiteSpace(addressFilter)
                    || string.Equals(t.From, addressFilter, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(t.To, addressFilter, StringComparison.OrdinalIgnoreCase))
                .Select(MapTransaction)
                .ToList();

            return EthereumTransactionPendingResult.Ok(transactions);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTransactionPendingResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogWarning(ex, "transaction/pending not supported by RPC provider on {Network}", network);
            return EthereumTransactionPendingResult.Fail("NOT_SUPPORTED", "This RPC provider does not expose pending-transaction visibility (eth_pendingTransactions).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "transaction/pending unexpected error on {Network}", network);
            return EthereumTransactionPendingResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>TX05 — transaction/decode. With an ABI, decodes into named function arguments; without one, reports only the function selector.</summary>
    public Task<EthereumTransactionDecodeResult> DecodeTransactionDataAsync(string data, string? abiJson, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data) || data.Length < 10)
                return Task.FromResult(EthereumTransactionDecodeResult.Fail("VAL_INVALID_DATA", "data must be at least a 4-byte function selector (0x + 8 hex chars)."));

            var selector = data.Substring(0, 10);

            if (string.IsNullOrWhiteSpace(abiJson))
            {
                var argsOnlySelector = new Dictionary<string, string> { ["selector"] = selector };
                return Task.FromResult(EthereumTransactionDecodeResult.Ok(string.Empty, selector, argsOnlySelector));
            }

            var contract = new Nethereum.Contracts.Contract(null, abiJson, string.Empty);
            var selectorHex = selector.Substring(2);
            var function = contract.ContractBuilder.ContractABI.Functions
                .FirstOrDefault(f => string.Equals(f.Sha3Signature, selectorHex, StringComparison.OrdinalIgnoreCase));

            if (function is null)
                return Task.FromResult(EthereumTransactionDecodeResult.Fail("FUNCTION_NOT_FOUND", $"No function in the supplied ABI matches selector {selector}."));

            var decoder = new FunctionCallDecoder();
            var parameters = decoder.DecodeInput(function, data);

            var decodedArgs = function.InputParameters
                .Select((p, i) => new { p.Name, Value = parameters.ElementAtOrDefault(i)?.Result?.ToString() ?? string.Empty })
                .ToDictionary(x => x.Name, x => x.Value);

            return Task.FromResult(EthereumTransactionDecodeResult.Ok(function.Name, selector, decodedArgs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "transaction/decode failed");
            return Task.FromResult(EthereumTransactionDecodeResult.Fail("DECODE_FAILED", ex.Message));
        }
    }

    /// <summary>TX06 — transaction/wait. Polls for a receipt until N confirmations or timeout.</summary>
    public async Task<EthereumTransactionWaitResult> WaitForTransactionAsync(
        string? network, string hash, int confirmations, int timeoutMs, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeoutMs);

            while (!timeoutCts.IsCancellationRequested)
            {
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash).ConfigureAwait(false);
                if (receipt is not null)
                {
                    var currentBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().ConfigureAwait(false);
                    var confirmed = receipt.BlockNumber is null ? 0 : (int)(currentBlock.Value - receipt.BlockNumber.Value) + 1;

                    if (confirmed >= confirmations)
                        return EthereumTransactionWaitResult.Ok(hash, receipt.Status is null ? null : receipt.Status.Value == 1, confirmed);
                }

                await Task.Delay(TimeSpan.FromSeconds(2), timeoutCts.Token).ConfigureAwait(false);
            }

            return EthereumTransactionWaitResult.Timeout(hash, 0);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (OperationCanceledException)
        {
            // Linked timeout CTS fired, not the caller's token — this is a timeout, not user cancellation.
            return EthereumTransactionWaitResult.Timeout(hash, 0);
        }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumTransactionWaitResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "transaction/wait RPC error for {Hash} on {Network}", hash, network);
            return EthereumTransactionWaitResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "transaction/wait unexpected error for {Hash} on {Network}", hash, network);
            return EthereumTransactionWaitResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    private static EthereumTransactionSummary MapTransaction(Transaction tx) => new(
        Hash: tx.TransactionHash,
        BlockNumber: tx.BlockNumber is null ? null : (long)tx.BlockNumber.Value,
        From: tx.From,
        To: tx.To,
        ValueWei: tx.Value?.Value.ToString() ?? "0",
        GasLimit: tx.Gas?.Value.ToString() ?? "0",
        GasPriceWei: tx.GasPrice?.Value.ToString(),
        Nonce: (long)(tx.Nonce?.Value ?? 0),
        Input: tx.Input ?? "0x",
        IsPending: tx.BlockNumber is null);

    private static IReadOnlyList<EthereumEventLogEntry> MapLogs(TransactionReceipt receipt) =>
        receipt.Logs!
            .Select(log => new EthereumEventLogEntry(
                log.Address ?? string.Empty,
                (log.Topics ?? Array.Empty<object>()).Select(t => t?.ToString() ?? string.Empty).ToList(),
                log.Data ?? "0x",
                log.BlockNumber is null ? null : (long)log.BlockNumber.Value,
                log.TransactionHash,
                log.LogIndex is null ? null : (int)log.LogIndex.Value))
            .ToList();
}
