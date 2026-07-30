namespace BizFirst.Integration.Ethereum.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;

/// <summary>
/// Gas resource: fee estimation and pricing (GAS01-GAS04, direct JSON-RPC primitives) plus a composite
/// fee-recommendation feature (GAS05, built on top of the other four — see <see cref="OptimizeTransactionAsync"/>).
/// </summary>
public sealed class EthereumGasService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumGasService> _logger;

    public EthereumGasService(EthereumConnectionProvider connectionProvider, ILogger<EthereumGasService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>GAS01 — gas/estimate (eth_estimateGas).</summary>
    public async Task<EthereumGasEstimateResult> EstimateGasAsync(
        string? network, string to, string? value, string? data, string? from, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var callInput = new CallInput
            {
                To = to,
                From = string.IsNullOrWhiteSpace(from) ? null : from,
                Data = string.IsNullOrWhiteSpace(data) ? null : data,
                Value = string.IsNullOrWhiteSpace(value) ? null : new HexBigInteger(BigInteger.Parse(value)),
            };

            var gas = await web3.Eth.TransactionManager.EstimateGasAsync(callInput).ConfigureAwait(false);
            return EthereumGasEstimateResult.Ok(gas.Value.ToString());
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumGasEstimateResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (FormatException ex)
        {
            return EthereumGasEstimateResult.Fail("VAL_INVALID_VALUE", $"'value' must be a base-10 wei integer string: {ex.Message}");
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "gas/estimate RPC error for {To} on {Network}", to, network);
            return EthereumGasEstimateResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gas/estimate unexpected error for {To} on {Network}", to, network);
            return EthereumGasEstimateResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>GAS02 — gas/price (legacy eth_gasPrice).</summary>
    public async Task<EthereumGasPriceResult> GetGasPriceAsync(string? network, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var priceWei = await web3.Eth.GasPrice.SendRequestAsync().ConfigureAwait(false);
            var gwei = UnitConversion.Convert.FromWei(priceWei.Value, UnitConversion.EthUnit.Gwei);
            return EthereumGasPriceResult.Ok(priceWei.Value.ToString(), gwei);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumGasPriceResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "gas/price RPC error on {Network}", network);
            return EthereumGasPriceResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gas/price unexpected error on {Network}", network);
            return EthereumGasPriceResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// GAS03 — gas/feeHistory (eth_feeHistory). <paramref name="blockCount"/> trailing blocks ending at
    /// <paramref name="newestBlock"/> (default "latest"); <paramref name="rewardPercentiles"/> optionally
    /// requests per-block priority-fee percentiles (e.g. [10, 50, 90]).
    /// </summary>
    public async Task<EthereumGasFeeHistoryResult> GetFeeHistoryAsync(
        string? network, int blockCount, string? newestBlock, IReadOnlyList<double>? rewardPercentiles,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var block = EthereumAccountService.ParseBlockParameter(newestBlock ?? "latest");
            var percentiles = rewardPercentiles?.Select(p => (decimal)p).ToArray() ?? Array.Empty<decimal>();

            // VERIFY: confirmed by reflecting over the referenced Nethereum.RPC 6.0.0 assembly (not a guess) —
            // web3.Eth.FeeHistory is Nethereum.RPC.Eth.Transactions.IEthFeeHistory, whose sole member is
            //   Task<FeeHistoryResult> SendRequestAsync(HexBigInteger blockCount, BlockParameter newestBlock, decimal[] rewardPercentiles, object? id = null)
            // i.e. FeeHistory is the request object directly (no container/sub-property), and rewardPercentiles
            // is decimal[] (not double[] as the design table suggested), which is why we cast above.
            var history = await web3.Eth.FeeHistory.SendRequestAsync(new HexBigInteger(blockCount), block, percentiles).ConfigureAwait(false);

            var baseFees = history.BaseFeePerGas ?? Array.Empty<HexBigInteger>();
            var usedRatios = history.GasUsedRatio ?? Array.Empty<decimal>();
            var rewards = history.Reward;
            var oldest = (long)(history.OldestBlock?.Value ?? 0);

            // eth_feeHistory returns blockCount+1 base-fee entries (the extra trailing entry is the
            // estimated base fee for the next/pending block) but only blockCount reward entries, so the
            // last synthetic entry legitimately has no RewardsWei — guarded below, not a bug.
            var entries = new List<EthereumFeeHistoryEntry>(baseFees.Length);
            for (var i = 0; i < baseFees.Length; i++)
            {
                var rewardsForBlock = (rewards is not null && i < rewards.Length && rewards[i] is not null)
                    ? (IReadOnlyList<string>)rewards[i].Select(r => r.Value.ToString()).ToList()
                    : null;

                entries.Add(new EthereumFeeHistoryEntry(
                    BlockNumber: oldest + i,
                    BaseFeePerGasWei: baseFees[i].Value.ToString(),
                    GasUsedRatio: i < usedRatios.Length ? (double)usedRatios[i] : 0d,
                    RewardsWei: rewardsForBlock));
            }

            return EthereumGasFeeHistoryResult.Ok(oldest, entries);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumGasFeeHistoryResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "gas/feeHistory RPC error on {Network}", network);
            return EthereumGasFeeHistoryResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gas/feeHistory unexpected error on {Network}", network);
            return EthereumGasFeeHistoryResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>GAS04 — gas/priorityFee (eth_maxPriorityFeePerGas).</summary>
    public async Task<EthereumGasPriorityFeeResult> GetMaxPriorityFeeAsync(string? network, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);

            // VERIFY: confirmed by reflecting over Nethereum.RPC 6.0.0 — there is no PriorityFee/MaxPriorityFee
            // type anywhere in the assembly, so eth_maxPriorityFeePerGas has no strongly-typed wrapper in this
            // version. Falls back to the generic client call, mirroring EthereumTransactionService's
            // eth_pendingTransactions pattern (GetPendingTransactionsAsync).
            var hex = await web3.Client.SendRequestAsync<string>("eth_maxPriorityFeePerGas").ConfigureAwait(false);
            var wei = new HexBigInteger(hex).Value;
            var gwei = UnitConversion.Convert.FromWei(wei, UnitConversion.EthUnit.Gwei);

            return EthereumGasPriorityFeeResult.Ok(wei.ToString(), gwei);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumGasPriorityFeeResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogWarning(ex, "gas/priorityFee not supported by RPC provider on {Network}", network);
            return EthereumGasPriorityFeeResult.Fail("NOT_SUPPORTED", "This RPC provider does not expose eth_maxPriorityFeePerGas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gas/priorityFee unexpected error on {Network}", network);
            return EthereumGasPriorityFeeResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// GAS05 — gas/optimize. NOT a raw RPC primitive: a composite feature built on top of GAS02
    /// (<see cref="GetGasPriceAsync"/>), GAS03 (<see cref="GetFeeHistoryAsync"/>, last 10 blocks), and
    /// GAS04 (<see cref="GetMaxPriorityFeeAsync"/>). Computes a recommended EIP-1559
    /// maxFeePerGas/maxPriorityFeePerGas pair for the requested risk strategy.
    ///
    /// HEURISTIC — these multipliers are judgment calls, not empirically validated against real network
    /// data (unlike, e.g., the design docs' unsourced "20-40% cost reduction via batching" claim, which
    /// this deliberately does NOT repeat or imply):
    ///   - "safe":     baseFee x1.1 buffer, priority fee x0.8 (10th-percentile-biased) — cheaper, may confirm slowly.
    ///   - "standard": baseFee x1.2 buffer, priority fee x1.0 — the 1.2x figure mirrors the safety multiplier
    ///                 already used elsewhere in this design's docs; priority fee taken as-is from eth_maxPriorityFeePerGas.
    ///   - "fast":     baseFee x1.5 buffer, priority fee x1.5 (90th-percentile-biased) — pays a premium to confirm quickly.
    /// If eth_maxPriorityFeePerGas (GAS04) is unsupported by the RPC provider, falls back to the strategy's
    /// corresponding reward percentile (10/50/90) from the most recent block in the GAS03 fee history; if
    /// that is also unavailable, falls back to a fixed 1 gwei default (also a heuristic, not a network read).
    /// </summary>
    public async Task<EthereumGasOptimizeResult> OptimizeTransactionAsync(
        string? network, string? operationsJson, string? strategy, CancellationToken cancellationToken = default)
    {
        var normalizedStrategy = string.IsNullOrWhiteSpace(strategy) ? "standard" : strategy.Trim().ToLowerInvariant();
        if (normalizedStrategy is not ("safe" or "standard" or "fast"))
        {
            return EthereumGasOptimizeResult.Fail("VAL_INVALID_STRATEGY", $"strategy must be one of 'safe', 'standard', 'fast' — got '{strategy}'.");
        }

        try
        {
            var feeHistory = await GetFeeHistoryAsync(network, 10, "latest", new[] { 10d, 50d, 90d }, cancellationToken).ConfigureAwait(false);
            if (!feeHistory.Success)
                return EthereumGasOptimizeResult.Fail(feeHistory.ErrorCode, feeHistory.ErrorMessage);

            // The final entry is eth_feeHistory's synthetic next-block estimate — the most relevant base fee
            // for a transaction about to be submitted. Fall back to gas/price if feeHistory returned no blocks.
            BigInteger baseFeeWei;
            if (feeHistory.Blocks.Count > 0)
            {
                baseFeeWei = BigInteger.Parse(feeHistory.Blocks[^1].BaseFeePerGasWei);
            }
            else
            {
                var gasPrice = await GetGasPriceAsync(network, cancellationToken).ConfigureAwait(false);
                if (!gasPrice.Success)
                    return EthereumGasOptimizeResult.Fail(gasPrice.ErrorCode, gasPrice.ErrorMessage);
                baseFeeWei = BigInteger.Parse(gasPrice.GasPriceWei);
            }

            var percentileIndex = normalizedStrategy switch { "safe" => 0, "fast" => 2, _ => 1 }; // [10, 50, 90]
            var priorityFee = await GetMaxPriorityFeeAsync(network, cancellationToken).ConfigureAwait(false);

            BigInteger rawPriorityFeeWei;
            if (priorityFee.Success)
            {
                rawPriorityFeeWei = BigInteger.Parse(priorityFee.PriorityFeeWei);
            }
            else
            {
                var mostRecentActualBlock = feeHistory.Blocks.LastOrDefault(b => b.RewardsWei is { Count: > 0 });
                rawPriorityFeeWei = mostRecentActualBlock is not null && percentileIndex < mostRecentActualBlock.RewardsWei!.Count
                    ? BigInteger.Parse(mostRecentActualBlock.RewardsWei[percentileIndex])
                    : new BigInteger(1_000_000_000); // 1 gwei fallback — heuristic default, not a network read.
            }

            var (baseFeeMultiplier, priorityFeeMultiplier) = normalizedStrategy switch
            {
                "safe" => (1.1m, 0.8m),
                "fast" => (1.5m, 1.5m),
                _ => (1.2m, 1.0m), // "standard"
            };

            var maxPriorityFeeWei = ScaleBigInteger(rawPriorityFeeWei, priorityFeeMultiplier);
            var maxFeePerGasWei = ScaleBigInteger(baseFeeWei, baseFeeMultiplier) + maxPriorityFeeWei;

            var (estimatedGasUnits, estimatedTotalCostWei) = await TryEstimateOperationsCostAsync(
                network, operationsJson, maxFeePerGasWei, cancellationToken).ConfigureAwait(false);

            return EthereumGasOptimizeResult.Ok(
                normalizedStrategy,
                baseFeeWei.ToString(),
                maxPriorityFeeWei.ToString(),
                maxFeePerGasWei.ToString(),
                estimatedGasUnits,
                estimatedTotalCostWei);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumGasOptimizeResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "gas/optimize RPC error on {Network}", network);
            return EthereumGasOptimizeResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gas/optimize unexpected error on {Network}", network);
            return EthereumGasOptimizeResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// Best-effort: parses <paramref name="operationsJson"/> as a JSON array of pending call descriptions
    /// (<c>{"to","value","data","from"}</c>, all optional except "to"), sums their eth_estimateGas results
    /// via <see cref="EstimateGasAsync"/> (using <c>this</c> — no RPC calls are duplicated), and prices the
    /// total at <paramref name="maxFeePerGasWei"/>. Individual operations that fail to parse or estimate are
    /// skipped (logged, not fatal) since gas/optimize's primary job is the fee recommendation, not this
    /// secondary cost projection. Returns (null, null) when no operations were supplied.
    /// </summary>
    private async Task<(string? EstimatedGasUnits, string? EstimatedTotalCostWei)> TryEstimateOperationsCostAsync(
        string? network, string? operationsJson, BigInteger maxFeePerGasWei, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(operationsJson))
            return (null, null);

        List<GasOperationInput>? operations;
        try
        {
            operations = JsonSerializer.Deserialize<List<GasOperationInput>>(
                operationsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "gas/optimize: 'operations' was not valid JSON — skipping cost projection.");
            return (null, null);
        }

        if (operations is null or { Count: 0 })
            return (null, null);

        var totalGasUnits = BigInteger.Zero;
        foreach (var op in operations)
        {
            if (string.IsNullOrWhiteSpace(op.To))
                continue;

            var estimate = await EstimateGasAsync(network, op.To, op.Value, op.Data, op.From, cancellationToken).ConfigureAwait(false);
            if (estimate.Success)
                totalGasUnits += BigInteger.Parse(estimate.GasUnits);
            else
                _logger.LogWarning("gas/optimize: skipping operation to {To} — estimate failed: {Error}", op.To, estimate.ErrorMessage);
        }

        return (totalGasUnits.ToString(), (totalGasUnits * maxFeePerGasWei).ToString());
    }

    private static BigInteger ScaleBigInteger(BigInteger value, decimal multiplier)
    {
        // Scaled by 10,000 and back to keep the arithmetic in BigInteger (wei values can exceed decimal's
        // ~28-29 significant digits for extreme inputs) while still supporting 4-decimal-place multipliers.
        const int precision = 10_000;
        var scaledMultiplier = new BigInteger(multiplier * precision);
        return value * scaledMultiplier / precision;
    }

    private sealed record GasOperationInput(
        [property: JsonPropertyName("to")] string? To,
        [property: JsonPropertyName("value")] string? Value,
        [property: JsonPropertyName("data")] string? Data,
        [property: JsonPropertyName("from")] string? From);
}
