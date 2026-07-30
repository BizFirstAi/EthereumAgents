namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of GAS03 — gas/feeHistory (eth_feeHistory).</summary>
public sealed record EthereumGasFeeHistoryResult(
    bool Success,
    long OldestBlock,
    IReadOnlyList<EthereumFeeHistoryEntry> Blocks,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumGasFeeHistoryResult Ok(long oldestBlock, IReadOnlyList<EthereumFeeHistoryEntry> blocks) =>
        new(true, oldestBlock, blocks, string.Empty, string.Empty);

    public static EthereumGasFeeHistoryResult Fail(string errorCode, string errorMessage) =>
        new(false, 0, Array.Empty<EthereumFeeHistoryEntry>(), errorCode, errorMessage);
}
