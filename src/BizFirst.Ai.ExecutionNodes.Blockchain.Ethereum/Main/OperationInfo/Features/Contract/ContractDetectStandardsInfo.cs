using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SC07 — contract/detectStandards (addendum v3). Real ERC-165 probe + ERC-20 heuristic.</summary>
internal sealed class ContractDetectStandardsInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(ContractAddress) ? ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for contract/detectStandards.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
    };
}
