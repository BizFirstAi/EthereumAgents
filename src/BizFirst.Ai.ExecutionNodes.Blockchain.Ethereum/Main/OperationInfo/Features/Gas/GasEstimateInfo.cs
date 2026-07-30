using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>GAS01 — gas/estimate.</summary>
internal sealed class GasEstimateInfo : BaseEthereumOperationInfo
{
    public string? To { get; private set; }
    public string? Value { get; private set; }
    public string? Data { get; private set; }
    public string? From { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        To = reader.ReadConfigByKeyDefaultNull("to");
        Value = reader.ReadConfigByKeyDefaultNull("value");
        Data = reader.ReadConfigByKeyDefaultNull("data");
        From = reader.ReadConfigByKeyDefaultNull("from");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(To) ? ("VAL_MISSING_TO", "Config key 'to' is required for gas/estimate.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["to"] = To ?? string.Empty,
        ["value"] = Value ?? string.Empty,
        ["data"] = Data ?? string.Empty,
        ["from"] = From ?? string.Empty,
    };
}
