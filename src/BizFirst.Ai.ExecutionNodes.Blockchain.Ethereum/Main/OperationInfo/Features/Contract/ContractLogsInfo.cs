using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SC06 — contract/logs. One-shot historical eth_getLogs query for a single event type (identified by 'eventAbi', hashed down to the topic0 filter).</summary>
internal sealed class ContractLogsInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? EventAbi { get; private set; }
    public string? FromBlock { get; private set; }
    public string? ToBlock { get; private set; }
    public string? Topics { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        EventAbi = reader.ReadConfigByKeyDefaultNull("eventAbi");
        FromBlock = reader.ReadConfigByKeyDefaultNull("fromBlock");
        ToBlock = reader.ReadConfigByKeyDefaultNull("toBlock");
        Topics = reader.ReadConfigByKeyDefaultNull("topics");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(EventAbi)) return ("VAL_MISSING_EVENT_ABI", "Config key 'eventAbi' is required for contract/logs.");
        if (!IsValidJson(EventAbi)) return ("VAL_INVALID_EVENT_ABI", "Config key 'eventAbi' must be valid JSON.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["fromBlock"] = FromBlock ?? string.Empty,
        ["toBlock"] = ToBlock ?? string.Empty,
    };

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
