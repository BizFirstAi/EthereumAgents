namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT08 — nft/isApprovedForAll.</summary>
public sealed record EthereumNftIsApprovedForAllResult(bool Success, bool IsApproved, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftIsApprovedForAllResult Ok(bool isApproved) => new(true, isApproved, string.Empty, string.Empty);
    public static EthereumNftIsApprovedForAllResult Fail(string errorCode, string errorMessage) => new(false, false, errorCode, errorMessage);
}
