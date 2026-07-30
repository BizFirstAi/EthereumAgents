using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ENS02 — ens/reverse. Reverse resolution: address → ENS name via the Reverse Registrar.</summary>
internal sealed class EnsReverseInfo : BaseEthereumOperationInfo
{
    public string? Address { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Address = reader.ReadConfigByKeyDefaultNull("address");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Address) ? ("VAL_MISSING_ADDRESS", "Config key 'address' is required for ens/reverse.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["address"] = Address ?? string.Empty,
    };
}
