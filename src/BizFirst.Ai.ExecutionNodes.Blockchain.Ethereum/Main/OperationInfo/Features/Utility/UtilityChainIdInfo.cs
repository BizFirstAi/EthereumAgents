using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL03 — utility/chainId.</summary>
internal sealed class UtilityChainIdInfo : BaseEthereumOperationInfo
{
    public (string Code, string Message)? Validate() => null;

    public override Dictionary<string, object> ToDictionary() => new();
}
