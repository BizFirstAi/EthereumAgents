using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ENS03 — ens/avatar. Shorthand for the "avatar" text record (ENSIP-12).</summary>
internal sealed class EnsAvatarInfo : BaseEthereumOperationInfo
{
    public string? Name { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Name = reader.ReadConfigByKeyDefaultNull("name");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Name) ? ("VAL_MISSING_NAME", "Config key 'name' is required for ens/avatar.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["name"] = Name ?? string.Empty,
    };
}
