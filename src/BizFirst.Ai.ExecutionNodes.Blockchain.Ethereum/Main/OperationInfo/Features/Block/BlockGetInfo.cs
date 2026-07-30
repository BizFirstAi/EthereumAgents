using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>BL01 — block/get.</summary>
internal sealed class BlockGetInfo : BaseEthereumOperationInfo
{
    public string? BlockNumberOrHash { get; private set; }
    public bool IncludeTransactions { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        BlockNumberOrHash = reader.ReadConfigByKeyDefaultNull("blockNumberOrHash");
        IncludeTransactions = reader.ReadConfigByKeyBool("includeTransactions", false);
    }

    public (string Code, string Message)? Validate() => null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["blockNumberOrHash"] = BlockNumberOrHash ?? "latest",
        ["includeTransactions"] = IncludeTransactions,
    };
}
