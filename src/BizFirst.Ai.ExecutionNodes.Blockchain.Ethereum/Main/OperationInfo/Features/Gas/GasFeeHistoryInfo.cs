using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>GAS03 — gas/feeHistory.</summary>
internal sealed class GasFeeHistoryInfo : BaseEthereumOperationInfo
{
    public int? BlockCount { get; private set; }
    public string NewestBlock { get; private set; } = "latest";
    public IReadOnlyList<double>? RewardPercentiles { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        BlockCount = reader.ReadConfigByKey_Int("blockCount");
        NewestBlock = reader.ReadConfigByKey("newestBlock", "latest") ?? "latest";
        RewardPercentiles = ParseRewardPercentiles(reader.ReadConfigNodeByKey("rewardPercentiles")?.ToString());
    }

    public (string Code, string Message)? Validate() =>
        BlockCount is null or <= 0
            ? ("VAL_MISSING_BLOCK_COUNT", "Config key 'blockCount' is required for gas/feeHistory and must be a positive integer.")
            : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["blockCount"] = BlockCount ?? 0,
        ["newestBlock"] = NewestBlock,
    };

    /// <summary>Best-effort parse of a JSON array of numbers (e.g. "[10,50,90]"). Returns null on missing/invalid input rather than throwing.</summary>
    private static IReadOnlyList<double>? ParseRewardPercentiles(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<double>>(json);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }
}
