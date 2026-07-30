using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ENS04 — ens/text. Arbitrary text record lookup (e.g. "email", "url", "com.twitter").</summary>
internal sealed class EnsTextInfo : BaseEthereumOperationInfo
{
    public string? Name { get; private set; }
    public string? Key { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Name = reader.ReadConfigByKeyDefaultNull("name");
        Key = reader.ReadConfigByKeyDefaultNull("key");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return ("VAL_MISSING_NAME", "Config key 'name' is required for ens/text.");
        if (string.IsNullOrWhiteSpace(Key))
            return ("VAL_MISSING_KEY", "Config key 'key' is required for ens/text.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["name"] = Name ?? string.Empty,
        ["key"] = Key ?? string.Empty,
    };
}
