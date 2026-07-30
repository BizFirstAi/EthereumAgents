using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// NFT12 — nft/getMetadata (addendum v3). Calls tokenURI() then HTTP-fetches and parses the
/// (possibly ipfs://-resolved) JSON metadata document — this node's first outbound HTTP call.
/// </summary>
internal sealed class NftGetMetadataInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? TokenID { get; private set; }
    public int TimeoutSeconds { get; private set; } = 10;

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        TokenID = reader.ReadConfigByKeyDefaultNull("tokenId");
        TimeoutSeconds = reader.ReadConfigByKey_Int("timeoutSeconds", 10);
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/getMetadata.");
        if (string.IsNullOrWhiteSpace(TokenID)) return ("VAL_MISSING_TOKEN_ID", "Config key 'tokenId' is required for nft/getMetadata.");
        if (!System.Numerics.BigInteger.TryParse(TokenID, out _)) return ("VAL_INVALID_TOKEN_ID", "Config key 'tokenId' must be an integer uint256 token ID.");
        if (TimeoutSeconds is <= 0 or > 60) return ("VAL_INVALID_TIMEOUT", "Config key 'timeoutSeconds' must be between 1 and 60.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["tokenId"] = TokenID ?? string.Empty,
        ["timeoutSeconds"] = TimeoutSeconds,
    };
}
