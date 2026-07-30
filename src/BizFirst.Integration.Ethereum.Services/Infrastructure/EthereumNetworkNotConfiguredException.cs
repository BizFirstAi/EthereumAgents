namespace BizFirst.Integration.Ethereum.Services;

/// <summary>Thrown when a node operation references a network name that has no entry under the "Ethereum:Networks" configuration section.</summary>
public sealed class EthereumNetworkNotConfiguredException : Exception
{
    public string NetworkName { get; }

    public EthereumNetworkNotConfiguredException(string networkName)
        : base($"No RPC configuration found for network '{networkName}'. Add it under the 'Ethereum:Networks' application configuration section.")
    {
        NetworkName = networkName;
    }
}
