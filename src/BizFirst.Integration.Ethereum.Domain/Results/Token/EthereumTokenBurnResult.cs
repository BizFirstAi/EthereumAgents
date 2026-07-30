namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of ERC20_11 — token/burn. Matches the real, widely-adopted OpenZeppelin ERC20Burnable
/// extension: `burn(uint256)` (self) when no `from` is supplied, `burnFrom(address,uint256)`
/// (via allowance) when it is.
/// </summary>
public sealed record EthereumTokenBurnResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenBurnResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumTokenBurnResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
