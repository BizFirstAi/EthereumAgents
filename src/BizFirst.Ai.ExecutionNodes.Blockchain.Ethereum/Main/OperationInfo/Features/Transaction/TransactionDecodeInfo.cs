using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>TX05 — transaction/decode.</summary>
internal sealed class TransactionDecodeInfo : BaseEthereumOperationInfo
{
    public string? Data { get; private set; }
    public string? Abi { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Data = reader.ReadConfigByKeyDefaultNull("data");
        Abi = reader.ReadConfigByKeyDefaultNull("abi");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Data) ? ("VAL_MISSING_DATA", "Config key 'data' is required for transaction/decode.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["data"] = Data ?? string.Empty };
}
