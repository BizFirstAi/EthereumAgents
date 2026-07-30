using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ENS05 — ens/resolver. Returns just the resolver contract address registered for a name.</summary>
internal sealed class EnsResolverInfo : BaseEthereumOperationInfo
{
    public string? Name { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Name = reader.ReadConfigByKeyDefaultNull("name");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Name) ? ("VAL_MISSING_NAME", "Config key 'name' is required for ens/resolver.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["name"] = Name ?? string.Empty,
    };
}
