using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT01 — nft/balance.</summary>
internal sealed class NftBalanceInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? OwnerAddress { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        OwnerAddress = reader.ReadConfigByKeyDefaultNull("ownerAddress");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/balance.");
        if (string.IsNullOrWhiteSpace(OwnerAddress)) return ("VAL_MISSING_OWNER_ADDRESS", "Config key 'ownerAddress' is required for nft/balance.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["ownerAddress"] = OwnerAddress ?? string.Empty,
    };
}
