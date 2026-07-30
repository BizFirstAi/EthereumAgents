namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of TX06 — transaction/wait (poll until N confirmations or timeout).</summary>
public sealed record EthereumTransactionWaitResult(
    bool Success,
    string TxHash,
    bool? Status,
    int Confirmations,
    bool TimedOut,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumTransactionWaitResult Ok(string txHash, bool? status, int confirmations) =>
        new(true, txHash, status, confirmations, false, string.Empty, string.Empty);

    public static EthereumTransactionWaitResult Timeout(string txHash, int confirmations) =>
        new(false, txHash, null, confirmations, true, "TRANSACTION_WAIT_TIMEOUT", $"Transaction {txHash} did not reach the required confirmations before the timeout elapsed.");

    public static EthereumTransactionWaitResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, null, 0, false, errorCode, errorMessage);
}
