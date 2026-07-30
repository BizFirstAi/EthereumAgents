namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of ERC20_10 — token/mint. Minting is not part of the ERC-20 standard (EIP-20) — this
/// assumes the common `mint(address,uint256)` shape used by OpenZeppelin-style access-controlled
/// tokens, overridable via the operation's `functionName` field.
/// </summary>
public sealed record EthereumTokenMintResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenMintResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumTokenMintResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
