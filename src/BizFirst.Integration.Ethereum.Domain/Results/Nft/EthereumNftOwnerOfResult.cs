namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT02 — nft/ownerOf.</summary>
public sealed record EthereumNftOwnerOfResult(bool Success, string Owner, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftOwnerOfResult Ok(string owner) => new(true, owner, string.Empty, string.Empty);
    public static EthereumNftOwnerOfResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
