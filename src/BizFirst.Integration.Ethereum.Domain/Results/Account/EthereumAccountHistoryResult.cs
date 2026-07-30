namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of AC03 — account/history. NOT a JSON-RPC primitive: Ethereum nodes do not index
/// transactions by address, so this requires a configured block-explorer indexer API
/// (e.g. Etherscan-compatible) rather than a Nethereum RPC call. See
/// <see cref="EthereumIndexerNotConfiguredException"/> handling in EthereumAccountService.
/// </summary>
public sealed record EthereumAccountHistoryResult(
    bool Success,
    IReadOnlyList<EthereumTransactionHistoryRecord> Transactions,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumAccountHistoryResult Ok(IReadOnlyList<EthereumTransactionHistoryRecord> transactions) =>
        new(true, transactions, string.Empty, string.Empty);

    public static EthereumAccountHistoryResult Fail(string errorCode, string errorMessage) =>
        new(false, Array.Empty<EthereumTransactionHistoryRecord>(), errorCode, errorMessage);
}
