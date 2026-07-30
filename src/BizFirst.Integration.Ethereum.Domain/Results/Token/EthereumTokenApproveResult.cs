namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_03 — token/approve.</summary>
public sealed record EthereumTokenApproveResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenApproveResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumTokenApproveResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
