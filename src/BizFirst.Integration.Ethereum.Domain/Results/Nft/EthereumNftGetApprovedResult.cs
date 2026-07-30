namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT07 — nft/getApproved.</summary>
public sealed record EthereumNftGetApprovedResult(bool Success, string Approved, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftGetApprovedResult Ok(string approved) => new(true, approved, string.Empty, string.Empty);
    public static EthereumNftGetApprovedResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
