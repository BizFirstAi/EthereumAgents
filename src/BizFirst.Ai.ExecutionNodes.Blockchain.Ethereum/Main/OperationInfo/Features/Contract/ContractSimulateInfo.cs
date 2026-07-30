using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SC03 — contract/simulate. Previews what a function would return/revert with via eth_call, without broadcasting a transaction.</summary>
internal sealed class ContractSimulateInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? Abi { get; private set; }
    public string? FunctionName { get; private set; }
    public string? FunctionArgs { get; private set; }
    public string? Value { get; private set; }
    public string? From { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        Abi = reader.ReadConfigByKeyDefaultNull("abi");
        FunctionName = reader.ReadConfigByKeyDefaultNull("functionName");
        FunctionArgs = reader.ReadConfigByKeyDefaultNull("functionArgs");
        Value = reader.ReadConfigByKeyDefaultNull("value");
        From = reader.ReadConfigByKeyDefaultNull("from");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for contract/simulate.");
        if (string.IsNullOrWhiteSpace(Abi)) return ("VAL_MISSING_ABI", "Config key 'abi' is required for contract/simulate.");
        if (!IsValidJson(Abi)) return ("VAL_INVALID_ABI", "Config key 'abi' must be valid JSON.");
        if (string.IsNullOrWhiteSpace(FunctionName)) return ("VAL_MISSING_FUNCTION_NAME", "Config key 'functionName' is required for contract/simulate.");
        if (!string.IsNullOrWhiteSpace(Value) && !System.Numerics.BigInteger.TryParse(Value, out _)) return ("VAL_INVALID_VALUE", "Config key 'value' must be an integer wei amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["functionName"] = FunctionName ?? string.Empty,
        ["from"] = From ?? string.Empty,
    };

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
