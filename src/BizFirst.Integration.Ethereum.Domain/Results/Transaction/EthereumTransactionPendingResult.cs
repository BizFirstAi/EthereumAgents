namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of TX04 — transaction/pending. Mempool visibility is RPC-provider-dependent
/// (not every provider exposes txpool_content / eth_pendingTransactions equivalents).
/// </summary>
public sealed record EthereumTransactionPendingResult(bool Success, IReadOnlyList<EthereumTransactionSummary> Transactions, string ErrorCode, string ErrorMessage)
{
    public static EthereumTransactionPendingResult Ok(IReadOnlyList<EthereumTransactionSummary> transactions) =>
        new(true, transactions, string.Empty, string.Empty);

    public static EthereumTransactionPendingResult Fail(string errorCode, string errorMessage) =>
        new(false, Array.Empty<EthereumTransactionSummary>(), errorCode, errorMessage);
}
