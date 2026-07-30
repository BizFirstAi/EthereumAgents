using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// Common per-operation config every Ethereum operation shares: which network to run against.
/// Selects an entry from the "Ethereum:Networks" application configuration (see
/// <see cref="EthereumNetworkOptions"/>) — not a raw RPC URL, so node instances stay portable
/// across environments.
/// </summary>
internal abstract class BaseEthereumOperationInfo : BaseNodeOperationInfo
{
    /// <summary>Network name (e.g. "ethereum", "polygon"). Falls back to the configured default network when omitted.</summary>
    public string? Network { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        Network = reader.ReadConfigByKeyDefaultNull("network");
    }
}
