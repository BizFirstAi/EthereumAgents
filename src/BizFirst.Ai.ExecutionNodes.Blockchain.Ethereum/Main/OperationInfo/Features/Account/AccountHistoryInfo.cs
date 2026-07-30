using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>AC03 — account/history.</summary>
internal sealed class AccountHistoryInfo : BaseEthereumOperationInfo
{
    public string? Address { get; private set; }
    public long? FromBlock { get; private set; }
    public long? ToBlock { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Address = reader.ReadConfigByKeyDefaultNull("address");
        FromBlock = reader.ReadConfigByKey_Int("fromBlock") is int fb ? fb : null;
        ToBlock = reader.ReadConfigByKey_Int("toBlock") is int tb ? tb : null;
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Address) ? ("VAL_MISSING_ADDRESS", "Config key 'address' is required for account/history.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["address"] = Address ?? string.Empty };
}
