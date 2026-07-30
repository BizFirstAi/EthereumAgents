using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SC05 — contract/multicall. Batches multiple independent read calls into one node execution. Partial success: one call's failure doesn't fail the batch — see EthereumContractService.MulticallAsync.</summary>
internal sealed class ContractMulticallInfo : BaseEthereumOperationInfo
{
    public string? Calls { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Calls = reader.ReadConfigByKeyDefaultNull("calls");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Calls)) return ("VAL_MISSING_CALLS", "Config key 'calls' is required for contract/multicall.");
        if (!IsValidJsonArray(Calls)) return ("VAL_INVALID_CALLS", "Config key 'calls' must be a JSON array of call objects.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["calls"] = Calls ?? string.Empty,
    };

    private static bool IsValidJsonArray(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException) { return false; }
    }
}
