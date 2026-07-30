using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// GAS05 — gas/optimize. Composite feature (not a raw RPC primitive) — see EthereumGasService.OptimizeTransactionAsync.
/// </summary>
internal sealed class GasOptimizeInfo : BaseEthereumOperationInfo
{
    private static readonly HashSet<string> ValidStrategies = new(StringComparer.OrdinalIgnoreCase) { "safe", "standard", "fast" };

    /// <summary>Raw JSON array of pending call descriptions (<c>{"to","value","data","from"}</c>), or null. Parsed downstream by the service.</summary>
    public string? Operations { get; private set; }
    public string Strategy { get; private set; } = "standard";

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Operations = reader.ReadConfigNodeByKey("operations")?.ToString();
        Strategy = reader.ReadConfigByKey("strategy", "standard") ?? "standard";
    }

    public (string Code, string Message)? Validate() =>
        !ValidStrategies.Contains(Strategy)
            ? ("VAL_INVALID_STRATEGY", $"Config key 'strategy' must be one of 'safe', 'standard', 'fast' for gas/optimize — got '{Strategy}'.")
            : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["strategy"] = Strategy,
        ["operations"] = Operations ?? string.Empty,
    };
}
