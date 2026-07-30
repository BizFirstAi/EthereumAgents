namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of TX03 — transaction/receipt (renamed from v1's "transaction/status" — this is the real eth_getTransactionReceipt primitive).</summary>
public sealed record EthereumTransactionReceiptResult(
    bool Success,
    string TxHash,
    bool? Status,
    long? BlockNumber,
    string GasUsed,
    IReadOnlyList<EthereumEventLogEntry> Logs,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumTransactionReceiptResult Ok(
        string txHash, bool? status, long? blockNumber, string gasUsed, IReadOnlyList<EthereumEventLogEntry> logs) =>
        new(true, txHash, status, blockNumber, gasUsed, logs, string.Empty, string.Empty);

    public static EthereumTransactionReceiptResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, null, null, "0", Array.Empty<EthereumEventLogEntry>(), errorCode, errorMessage);
}
