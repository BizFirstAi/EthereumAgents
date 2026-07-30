using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL07 — utility/decodeEventLog. Load-bearing — see EthereumUtilityService.DecodeEventLogAsync.</summary>
internal sealed class UtilityDecodeEventLogInfo : BaseEthereumOperationInfo
{
    public string? EventAbi { get; private set; }
    public string? Topics { get; private set; }
    public string? Data { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        EventAbi = reader.ReadConfigByKeyDefaultNull("eventAbi");
        Topics = reader.ReadConfigByKeyDefaultNull("topics");
        Data = reader.ReadConfigByKeyDefaultNull("data");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(EventAbi)) return ("VAL_MISSING_EVENT_ABI", "Config key 'eventAbi' is required for utility/decodeEventLog.");
        if (!IsValidJson(EventAbi)) return ("VAL_INVALID_EVENT_ABI", "Config key 'eventAbi' must be valid JSON.");
        if (string.IsNullOrWhiteSpace(Topics)) return ("VAL_MISSING_TOPICS", "Config key 'topics' is required for utility/decodeEventLog.");
        if (!IsValidJson(Topics)) return ("VAL_INVALID_TOPICS", "Config key 'topics' must be a valid JSON array.");
        if (string.IsNullOrWhiteSpace(Data)) return ("VAL_MISSING_DATA", "Config key 'data' is required for utility/decodeEventLog.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new() { ["data"] = Data ?? string.Empty };

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
