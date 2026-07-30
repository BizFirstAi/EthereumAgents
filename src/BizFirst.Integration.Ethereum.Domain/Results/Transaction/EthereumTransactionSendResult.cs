namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of TX02 — transaction/send.</summary>
public sealed record EthereumTransactionSendResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumTransactionSendResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumTransactionSendResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
