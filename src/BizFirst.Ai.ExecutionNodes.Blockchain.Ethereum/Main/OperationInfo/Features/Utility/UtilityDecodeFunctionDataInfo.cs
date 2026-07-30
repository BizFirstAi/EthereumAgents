using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL05 — utility/decodeFunctionData.</summary>
internal sealed class UtilityDecodeFunctionDataInfo : BaseEthereumOperationInfo
{
    public string? Abi { get; private set; }
    public string? Data { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Abi = reader.ReadConfigByKeyDefaultNull("abi");
        Data = reader.ReadConfigByKeyDefaultNull("data");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Abi)) return ("VAL_MISSING_ABI", "Config key 'abi' is required for utility/decodeFunctionData.");
        if (!IsValidJson(Abi)) return ("VAL_INVALID_ABI", "Config key 'abi' must be valid JSON.");
        if (string.IsNullOrWhiteSpace(Data)) return ("VAL_MISSING_DATA", "Config key 'data' is required for utility/decodeFunctionData.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new() { ["data"] = Data ?? string.Empty };

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
