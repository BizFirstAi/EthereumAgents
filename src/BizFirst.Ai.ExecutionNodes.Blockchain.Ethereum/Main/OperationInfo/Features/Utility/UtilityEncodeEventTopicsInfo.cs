using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL06 — utility/encodeEventTopics.</summary>
internal sealed class UtilityEncodeEventTopicsInfo : BaseEthereumOperationInfo
{
    public string? EventAbi { get; private set; }
    public string? Args { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        EventAbi = reader.ReadConfigByKeyDefaultNull("eventAbi");
        Args = reader.ReadConfigByKeyDefaultNull("args");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(EventAbi)) return ("VAL_MISSING_EVENT_ABI", "Config key 'eventAbi' is required for utility/encodeEventTopics.");
        if (!IsValidJson(EventAbi)) return ("VAL_INVALID_EVENT_ABI", "Config key 'eventAbi' must be valid JSON.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new();

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
