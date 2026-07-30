using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>AC02 — account/nonce.</summary>
internal sealed class AccountNonceInfo : BaseEthereumOperationInfo
{
    public string? Address { get; private set; }
    public string? Block { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Address = reader.ReadConfigByKeyDefaultNull("address");
        Block = reader.ReadConfigByKeyDefaultNull("block");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Address) ? ("VAL_MISSING_ADDRESS", "Config key 'address' is required for account/nonce.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["address"] = Address ?? string.Empty };
}
