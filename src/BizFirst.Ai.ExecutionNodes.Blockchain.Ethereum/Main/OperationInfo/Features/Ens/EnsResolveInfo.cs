using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ENS01 — ens/resolve. Forward resolution: ENS name (e.g. "vitalik.eth") → address.</summary>
internal sealed class EnsResolveInfo : BaseEthereumOperationInfo
{
    public string? Name { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Name = reader.ReadConfigByKeyDefaultNull("name");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Name) ? ("VAL_MISSING_NAME", "Config key 'name' is required for ens/resolve.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["name"] = Name ?? string.Empty,
    };
}
