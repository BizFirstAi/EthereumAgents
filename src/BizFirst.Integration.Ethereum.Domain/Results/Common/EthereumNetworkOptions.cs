namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Static application configuration bound from the "Ethereum" appsettings section.
/// Distinct from per-node-instance config (which selects a network by name via the
/// "network" config key on each operation) — this is the RPC endpoint catalogue.
/// </summary>
public sealed class EthereumNetworkOptions
{
    public const string SectionName = "Ethereum";

    public string DefaultNetwork { get; init; } = "ethereum";
    public Dictionary<string, EthereumNetworkConfig> Networks { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public int RpcTimeoutSeconds { get; init; } = 30;
}

/// <summary>
/// Configuration for a single EVM-compatible network (e.g. "ethereum", "polygon").
///
/// RPC endpoint sourcing — exactly one of these MUST be set:
///   • <see cref="RpcUrl"/> — plain URL in appsettings/env. Only appropriate for endpoints that
///     do NOT embed an API key (e.g. local Anvil/Hardhat/Ganache, self-hosted nodes, public
///     rate-limited endpoints).
///   • <see cref="RpcUrlCredentialId"/> — vault credential ID (SERVICE_URL type) whose Service URL
///     field holds the full RPC URL. Required for any endpoint that embeds a provider API key
///     (Alchemy, Infura, QuickNode, etc). Prevents secret leakage via configuration dumps or
///     source-control commits.
/// </summary>
public sealed class EthereumNetworkConfig
{
    /// <summary>Plain RPC URL. Leave null when <see cref="RpcUrlCredentialId"/> is set.</summary>
    public string? RpcUrl { get; init; }

    /// <summary>
    /// Vault credential ID that resolves to the full RPC URL. The credential must be of SERVICE_URL
    /// type and its Service URL field must contain the complete URL (protocol + host + path + key).
    /// When set, <see cref="RpcUrl"/> is ignored.
    /// </summary>
    public int? RpcUrlCredentialId { get; init; }

    public required long ChainID { get; init; }
    public string? BlockExplorer { get; init; }
}
