using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT08 — nft/isApprovedForAll.</summary>
internal sealed class NftIsApprovedForAllInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? Owner { get; private set; }
    public string? Operator { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        Owner = reader.ReadConfigByKeyDefaultNull("owner");
        Operator = reader.ReadConfigByKeyDefaultNull("operator");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/isApprovedForAll.");
        if (string.IsNullOrWhiteSpace(Owner)) return ("VAL_MISSING_OWNER", "Config key 'owner' is required for nft/isApprovedForAll.");
        if (string.IsNullOrWhiteSpace(Operator)) return ("VAL_MISSING_OPERATOR", "Config key 'operator' is required for nft/isApprovedForAll.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["owner"] = Owner ?? string.Empty,
        ["operator"] = Operator ?? string.Empty,
    };
}
