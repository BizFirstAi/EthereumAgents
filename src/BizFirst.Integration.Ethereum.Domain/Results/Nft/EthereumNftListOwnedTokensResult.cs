namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of NFT11 — nft/listOwnedTokens. Gated on the real ERC-721 Enumerable extension
/// (interface ID 0x780e9d63) — fails clearly with NOT_SUPPORTED rather than attempting an
/// unreliable Transfer-event log scan when the contract doesn't implement it.
/// </summary>
public sealed record EthereumNftListOwnedTokensResult(bool Success, IReadOnlyList<string> TokenIds, long TotalBalance, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftListOwnedTokensResult Ok(IReadOnlyList<string> tokenIds, long totalBalance) =>
        new(true, tokenIds, totalBalance, string.Empty, string.Empty);

    public static EthereumNftListOwnedTokensResult Fail(string errorCode, string errorMessage) =>
        new(false, Array.Empty<string>(), 0, errorCode, errorMessage);
}
