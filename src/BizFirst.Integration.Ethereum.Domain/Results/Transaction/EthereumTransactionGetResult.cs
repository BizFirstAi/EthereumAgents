namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of TX01 — transaction/get.</summary>
public sealed record EthereumTransactionGetResult(bool Success, EthereumTransactionSummary? Transaction, string ErrorCode, string ErrorMessage)
{
    public static EthereumTransactionGetResult Ok(EthereumTransactionSummary transaction) => new(true, transaction, string.Empty, string.Empty);
    public static EthereumTransactionGetResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}
