namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of NFT12 — nft/getMetadata. Calls tokenURI(), resolves ipfs:// to the ipfs.io public
/// gateway, HTTP-fetches the (possibly resolved) URL through this codebase's existing
/// SSRF-guarded fetch pattern, and parses the response as JSON. <see cref="TokenUri"/> is the raw
/// on-chain value; <see cref="MetadataJson"/> is the fetched/parsed document (raw JSON text).
/// </summary>
public sealed record EthereumNftGetMetadataResult(bool Success, string TokenUri, string MetadataJson, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftGetMetadataResult Ok(string tokenUri, string metadataJson) =>
        new(true, tokenUri, metadataJson, string.Empty, string.Empty);

    public static EthereumNftGetMetadataResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
