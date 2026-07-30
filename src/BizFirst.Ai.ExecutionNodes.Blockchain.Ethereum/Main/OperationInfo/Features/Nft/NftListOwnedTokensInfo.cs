using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// NFT11 — nft/listOwnedTokens (addendum v3). Gated on the real ERC-721 Enumerable extension —
/// fails with NOT_SUPPORTED rather than an unreliable log scan when the contract doesn't implement it.
/// </summary>
internal sealed class NftListOwnedTokensInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? OwnerAddress { get; private set; }
    public int Limit { get; private set; } = 100;
    public int Offset { get; private set; } = 0;

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        OwnerAddress = reader.ReadConfigByKeyDefaultNull("ownerAddress");
        Limit = reader.ReadConfigByKey_Int("limit", 100);
        Offset = reader.ReadConfigByKey_Int("offset", 0);
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/listOwnedTokens.");
        if (string.IsNullOrWhiteSpace(OwnerAddress)) return ("VAL_MISSING_OWNER_ADDRESS", "Config key 'ownerAddress' is required for nft/listOwnedTokens.");
        if (Limit is <= 0 or > 1000) return ("VAL_INVALID_LIMIT", "Config key 'limit' must be between 1 and 1000.");
        if (Offset < 0) return ("VAL_INVALID_OFFSET", "Config key 'offset' must not be negative.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["ownerAddress"] = OwnerAddress ?? string.Empty,
        ["limit"] = Limit,
        ["offset"] = Offset,
    };
}
