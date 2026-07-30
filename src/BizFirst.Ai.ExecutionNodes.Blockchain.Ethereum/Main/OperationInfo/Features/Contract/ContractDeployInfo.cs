using System.Text.Json;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SC04 — contract/deploy. Broadcasts a contract-creation transaction. Signing key resolved via vault (credentialID).</summary>
internal sealed class ContractDeployInfo : BaseEthereumOperationInfo
{
    public string? Bytecode { get; private set; }
    public string? Abi { get; private set; }
    public string? ConstructorArgs { get; private set; }
    public string? Value { get; private set; }
    public string? GasLimit { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Bytecode = reader.ReadConfigByKeyDefaultNull("bytecode");
        Abi = reader.ReadConfigByKeyDefaultNull("abi");
        ConstructorArgs = reader.ReadConfigByKeyDefaultNull("constructorArgs");
        Value = reader.ReadConfigByKeyDefaultNull("value");
        GasLimit = reader.ReadConfigByKeyDefaultNull("gasLimit");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Bytecode)) return ("VAL_MISSING_BYTECODE", "Config key 'bytecode' is required for contract/deploy.");
        if (string.IsNullOrWhiteSpace(Abi)) return ("VAL_MISSING_ABI", "Config key 'abi' is required for contract/deploy.");
        if (!IsValidJson(Abi)) return ("VAL_INVALID_ABI", "Config key 'abi' must be valid JSON.");
        if (!string.IsNullOrWhiteSpace(Value) && !System.Numerics.BigInteger.TryParse(Value, out _)) return ("VAL_INVALID_VALUE", "Config key 'value' must be an integer wei amount.");
        if (!string.IsNullOrWhiteSpace(GasLimit) && !System.Numerics.BigInteger.TryParse(GasLimit, out _)) return ("VAL_INVALID_GAS_LIMIT", "Config key 'gasLimit' must be an integer gas amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["value"] = Value ?? string.Empty,
        ["gasLimit"] = GasLimit ?? string.Empty,
    };

    private static bool IsValidJson(string json)
    {
        try { using var _ = JsonDocument.Parse(json); return true; }
        catch (JsonException) { return false; }
    }
}
