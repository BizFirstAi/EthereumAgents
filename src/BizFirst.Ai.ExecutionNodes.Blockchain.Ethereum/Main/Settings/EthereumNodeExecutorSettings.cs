using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>Typed settings for <see cref="EthereumNodeExecutor"/> — resolves the active operation DTO from (resource, operation).</summary>
internal sealed class EthereumNodeExecutorSettings : ResourceBasedNodeExecutorSettings
{
    public override BaseNodeOperationInfo? CreateActiveInfo() =>
        EthereumOperationInfoFactory.Create(Resource, Operation, ConfigReader);
}
