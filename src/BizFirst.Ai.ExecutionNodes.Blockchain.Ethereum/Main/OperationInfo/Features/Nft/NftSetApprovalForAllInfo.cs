using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT06 — nft/setApprovalForAll. Signing key resolved via vault (credentialID).</summary>
internal sealed class NftSetApprovalForAllInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? Operator { get; private set; }
    public bool Approved { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        Operator = reader.ReadConfigByKeyDefaultNull("operator");
        Approved = reader.ReadConfigByKeyBool("approved", false);
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/setApprovalForAll.");
        if (string.IsNullOrWhiteSpace(Operator)) return ("VAL_MISSING_OPERATOR", "Config key 'operator' is required for nft/setApprovalForAll.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["operator"] = Operator ?? string.Empty,
        ["approved"] = Approved,
    };
}
