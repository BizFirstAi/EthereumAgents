using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL08 — utility/contractAddress. CREATE via 'nonce'; CREATE2 via 'bytecodeHash' + 'salt'.</summary>
internal sealed class UtilityContractAddressInfo : BaseEthereumOperationInfo
{
    public string? From { get; private set; }
    public long? Nonce { get; private set; }
    public string? BytecodeHash { get; private set; }
    public string? Salt { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        From = reader.ReadConfigByKeyDefaultNull("from");
        Nonce = reader.ReadConfigByKey_Int("nonce") is int n ? n : null;
        BytecodeHash = reader.ReadConfigByKeyDefaultNull("bytecodeHash");
        Salt = reader.ReadConfigByKeyDefaultNull("salt");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(From)) return ("VAL_MISSING_FROM", "Config key 'from' is required for utility/contractAddress.");
        var hasCreate = Nonce.HasValue;
        var hasCreate2 = !string.IsNullOrWhiteSpace(BytecodeHash) && !string.IsNullOrWhiteSpace(Salt);
        if (!hasCreate && !hasCreate2)
            return ("VAL_MISSING_PARAMETERS", "Provide either 'nonce' (for CREATE) or both 'bytecodeHash' and 'salt' (for CREATE2).");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new() { ["from"] = From ?? string.Empty };
}
