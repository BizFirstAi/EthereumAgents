namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of GAS05 — gas/optimize. NOT a JSON-RPC primitive — a composite/custom feature built on top
/// of GAS02 (gas/price), GAS03 (gas/feeHistory), and GAS04 (gas/priorityFee) that computes a recommended
/// EIP-1559 fee pair for a given risk strategy. See EthereumGasService.OptimizeTransactionAsync for the
/// exact (clearly-labeled, non-empirically-validated) heuristic multipliers used per strategy.
/// </summary>
public sealed record EthereumGasOptimizeResult(
    bool Success,
    string Strategy,
    string BaseFeePerGasWei,
    string MaxPriorityFeePerGasWei,
    string MaxFeePerGasWei,
    string? EstimatedGasUnits,
    string? EstimatedTotalCostWei,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumGasOptimizeResult Ok(
        string strategy,
        string baseFeePerGasWei,
        string maxPriorityFeePerGasWei,
        string maxFeePerGasWei,
        string? estimatedGasUnits,
        string? estimatedTotalCostWei) =>
        new(true, strategy, baseFeePerGasWei, maxPriorityFeePerGasWei, maxFeePerGasWei, estimatedGasUnits, estimatedTotalCostWei, string.Empty, string.Empty);

    public static EthereumGasOptimizeResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, "0", "0", "0", null, null, errorCode, errorMessage);
}
