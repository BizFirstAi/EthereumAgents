namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of NFT10 — nft/mint. Minting is not part of the ERC-721 standard (EIP-721) — this
/// assumes `{functionName}(address,uint256)` when a `tokenId` is supplied (explicit-ID mint) or
/// `{functionName}(address)` when it isn't (auto-incrementing mint), `functionName` defaulting
/// to `"safeMint"`.
/// </summary>
public sealed record EthereumNftMintResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftMintResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumNftMintResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
