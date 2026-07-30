namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>One block's fee data as returned by GAS03 — gas/feeHistory (eth_feeHistory).</summary>
public sealed record EthereumFeeHistoryEntry(
    long BlockNumber,
    string BaseFeePerGasWei,
    double GasUsedRatio,
    IReadOnlyList<string>? RewardsWei);
