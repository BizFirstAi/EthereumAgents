namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_02 — token/transfer. Most popular operation (stablecoin payments).</summary>
public sealed record EthereumTokenTransferResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenTransferResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumTokenTransferResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
