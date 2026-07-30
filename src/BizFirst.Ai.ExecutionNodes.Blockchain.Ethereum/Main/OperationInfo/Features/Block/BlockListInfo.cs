using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>BL03 — block/list. Composite operation: fans out to N Get Block calls.</summary>
internal sealed class BlockListInfo : BaseEthereumOperationInfo
{
    public int Count { get; private set; } = 10;
    public long? FromBlock { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Count = reader.ReadConfigByKey_Int("count", 10);
        FromBlock = reader.ReadConfigByKey_Int("fromBlock") is int fb ? fb : null;
    }

    public (string Code, string Message)? Validate() =>
        Count is <= 0 or > 100 ? ("VAL_INVALID_COUNT", "Config key 'count' must be between 1 and 100.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["count"] = Count };
}
