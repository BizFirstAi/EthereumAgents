namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT09 — nft/tokenUri. Raw URI as returned by the contract (often "ipfs://..."); resolving it is out of scope.</summary>
public sealed record EthereumNftTokenUriResult(bool Success, string TokenUri, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftTokenUriResult Ok(string tokenUri) => new(true, tokenUri, string.Empty, string.Empty);
    public static EthereumNftTokenUriResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
