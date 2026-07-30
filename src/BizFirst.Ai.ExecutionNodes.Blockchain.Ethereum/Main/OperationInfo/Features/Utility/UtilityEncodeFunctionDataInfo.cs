using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL04 — utility/encodeFunctionData.</summary>
internal sealed class UtilityEncodeFunctionDataInfo : BaseEthereumOperationInfo
{
    public string? Abi { get; private set; }
    public string? FunctionName { get; private set; }
    public string? Args { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Abi = reader.ReadConfigByKeyDefaultNull("abi");
        FunctionName = reader.ReadConfigByKeyDefaultNull("functionName");
        Args = reader.ReadConfigByKeyDefaultNull("args");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Abi)) return ("VAL_MISSING_ABI", "Config key 'abi' is required for utility/encodeFunctionData.");
        if (!IsValidJson(Abi)) return ("VAL_INVALID_ABI", "Config key 'abi' must be valid JSON.");
        if (string.IsNullOrWhiteSpace(FunctionName)) return ("VAL_MISSING_FUNCTION_NAME", "Config key 'functionName' is required for utility/encodeFunctionData.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["functionName"] = FunctionName ?? string.Empty,
    };

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
