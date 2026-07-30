using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>TX02 — transaction/send. Core operation. Signing key is resolved separately via the vault (credentialID), not a config field.</summary>
internal sealed class TransactionSendInfo : BaseEthereumOperationInfo
{
    public string? To { get; private set; }
    public string ValueWei { get; private set; } = "0";
    public string? Data { get; private set; }
    public string? GasLimit { get; private set; }
    public string? MaxFeePerGasWei { get; private set; }
    public string? MaxPriorityFeePerGasWei { get; private set; }
    public long? Nonce { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        To = reader.ReadConfigByKeyDefaultNull("to");
        ValueWei = reader.ReadConfigByKey("value", "0");
        Data = reader.ReadConfigByKeyDefaultNull("data");
        GasLimit = reader.ReadConfigByKeyDefaultNull("gasLimit");
        MaxFeePerGasWei = reader.ReadConfigByKeyDefaultNull("maxFeePerGas");
        MaxPriorityFeePerGasWei = reader.ReadConfigByKeyDefaultNull("maxPriorityFeePerGas");
        Nonce = reader.ReadConfigByKey_Int("nonce") is int n ? n : null;
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(To)) return ("VAL_MISSING_TO", "Config key 'to' is required for transaction/send.");
        if (!System.Numerics.BigInteger.TryParse(ValueWei, out _)) return ("VAL_INVALID_VALUE", "Config key 'value' must be an integer wei amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["to"] = To ?? string.Empty,
        ["value"] = ValueWei,
    };
}
