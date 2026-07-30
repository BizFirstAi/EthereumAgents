using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>GAS04 — gas/priorityFee. No operation-specific config; network selection only (see base).</summary>
internal sealed class GasPriorityFeeInfo : BaseEthereumOperationInfo
{
    public (string Code, string Message)? Validate() => null;

    public override Dictionary<string, object> ToDictionary() => new();
}
