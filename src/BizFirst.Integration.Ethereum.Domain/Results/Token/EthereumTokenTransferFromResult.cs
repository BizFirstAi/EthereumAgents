namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_04 — token/transferFrom (moves tokens via an existing allowance).</summary>
public sealed record EthereumTokenTransferFromResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenTransferFromResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumTokenTransferFromResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
