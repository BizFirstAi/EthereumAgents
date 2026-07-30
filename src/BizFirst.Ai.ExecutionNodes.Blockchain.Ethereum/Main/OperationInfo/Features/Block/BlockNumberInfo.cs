using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>BL02 — block/number.</summary>
internal sealed class BlockNumberInfo : BaseEthereumOperationInfo
{
    public (string Code, string Message)? Validate() => null;

    public override Dictionary<string, object> ToDictionary() => new();
}
