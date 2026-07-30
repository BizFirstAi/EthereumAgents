namespace BizFirst.Integration.Ethereum.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using BizFirst.Integration.Ethereum.Domain;
using BizFirst.Ai.ProcessEngine.Domain.Credentials;

/// <summary>
/// Resolves and caches <see cref="IWeb3"/> clients per network name. Read-only clients are cached
/// (analogous to Redis's <c>RedisConnectionCache</c> pattern) since they carry no secrets; signed
/// clients wrap a per-call <see cref="Account"/> built from a caller-supplied private key and are
/// never cached or retained beyond the call that constructs them.
///
/// Named <see cref="HttpClient"/> "EthereumRpc" is created per-Web3-instance via
/// <see cref="IHttpClientFactory"/> so pooling/lifetime and the <see cref="EthereumRateLimitHandler"/>
/// 429-retry behaviour are inherited automatically (Guideline 04, Flavour B) rather than hand-rolled.
///
/// RPC URL sourcing (see <see cref="EthereumNetworkConfig"/>):
///   • Plain <c>RpcUrl</c>          — used verbatim, no vault access.
///   • <c>RpcUrlCredentialId</c>    — a SERVICE_URL-type vault credential resolved once per network via
///                                     <see cref="ICredentialResolver.GetServiceUrlAsync"/>, using a scope
///                                     opened from <see cref="IServiceScopeFactory"/> (safe pattern for a
///                                     singleton consuming a scoped service). Resolved URLs are cached
///                                     in-memory for the process lifetime.
///
/// Nonce-collision protection (<see cref="AcquireAddressLockAsync"/>): every service method that signs
/// and sends a transaction fetches the "pending" nonce and sends within a per-address critical section
/// acquired here. Without this, two concurrent workflow branches signing from the same wallet address
/// (e.g. inside a parallel-fork) can fetch the identical pending nonce and race — one send fails or
/// unintentionally replaces the other.
/// </summary>
public sealed class EthereumConnectionProvider
{
    public const string HttpClientName = "EthereumRpc";

    private readonly ConcurrentDictionary<string, IWeb3> _readOnlyClients = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _resolvedRpcUrls = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _addressLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _resolutionLock = new(1, 1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EthereumNetworkOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EthereumConnectionProvider> _logger;

    public EthereumConnectionProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<EthereumNetworkOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<EthereumConnectionProvider> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Resolves the network CONFIG for a (possibly null/blank) network name, falling back to the configured default. Does NOT resolve credentials.</summary>
    public EthereumNetworkConfig ResolveNetwork(string? network)
    {
        var name = NormalizeName(network);
        if (!_options.Networks.TryGetValue(name, out var config))
        {
            _logger.LogWarning("Requested Ethereum network '{Network}' is not configured.", name);
            throw new EthereumNetworkNotConfiguredException(name);
        }
        return config;
    }

    /// <summary>Gets (or lazily creates and caches) a read-only <see cref="IWeb3"/> client. Async variant that resolves vault-backed RPC URLs when <see cref="EthereumNetworkConfig.RpcUrlCredentialId"/> is set.</summary>
    public async Task<IWeb3> GetReadOnlyClientAsync(string? network, CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(network);
        if (_readOnlyClients.TryGetValue(name, out var cached)) return cached;

        var config = ResolveNetwork(network);
        var rpcUrl = await ResolveRpcUrlAsync(name, config, cancellationToken).ConfigureAwait(false);

        return _readOnlyClients.GetOrAdd(name, _ =>
        {
            var rpcClient = CreateRpcClient(rpcUrl);
            return new Web3(rpcClient);
        });
    }

    /// <summary>Creates a NEW signing-capable <see cref="IWeb3"/> client. Never cached. Async variant that resolves vault-backed RPC URLs.</summary>
    public async Task<IWeb3> GetSignedClientAsync(string? network, string privateKeyHex, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyHex);
        var config = ResolveNetwork(network);
        var name = NormalizeName(network);
        var rpcUrl = await ResolveRpcUrlAsync(name, config, cancellationToken).ConfigureAwait(false);

        var account = new Account(privateKeyHex, config.ChainID);
        var rpcClient = CreateRpcClient(rpcUrl);
        return new Web3(account, rpcClient);
    }

    /// <summary>
    /// Acquires a per-address critical section. Callers MUST hold this for the entire
    /// "fetch nonce → build transaction → sign → send" sequence, not just the send call — the
    /// whole point is to stop two concurrent signers for the SAME address from both reading the
    /// same "pending" nonce. Release by disposing the returned handle (a plain <c>using</c> block
    /// is sufficient; no async disposal needed).
    /// </summary>
    public async Task<IDisposable> AcquireAddressLockAsync(string address, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        var semaphore = _addressLocks.GetOrAdd(address, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new AddressLockHandle(semaphore);
    }

    private sealed class AddressLockHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _released;

        public AddressLockHandle(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
                _semaphore.Release();
        }
    }

    /// <summary>
    /// Legacy sync accessor. Only valid for networks whose <see cref="EthereumNetworkConfig.RpcUrl"/>
    /// is a plain string. Throws if the network is configured to use a vault-backed URL — callers
    /// on that path must use <see cref="GetReadOnlyClientAsync"/>. Kept for test-fixture support.
    /// </summary>
    public IWeb3 GetReadOnlyClient(string? network)
    {
        var config = ResolveNetwork(network);
        if (config.RpcUrlCredentialId is not null)
            throw new InvalidOperationException(
                $"Ethereum network '{NormalizeName(network)}' uses a vault-backed RPC URL (RpcUrlCredentialId={config.RpcUrlCredentialId}). " +
                $"Call {nameof(GetReadOnlyClientAsync)} instead — the sync overload cannot resolve vault credentials.");

        var name = NormalizeName(network);
        return _readOnlyClients.GetOrAdd(name, _ =>
        {
            var rpcUrl = RequirePlainRpcUrl(name, config);
            var rpcClient = CreateRpcClient(rpcUrl);
            return new Web3(rpcClient);
        });
    }

    /// <summary>Legacy sync accessor. See <see cref="GetReadOnlyClient"/> for constraints.</summary>
    public IWeb3 GetSignedClient(string? network, string privateKeyHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyHex);
        var config = ResolveNetwork(network);
        if (config.RpcUrlCredentialId is not null)
            throw new InvalidOperationException(
                $"Ethereum network '{NormalizeName(network)}' uses a vault-backed RPC URL (RpcUrlCredentialId={config.RpcUrlCredentialId}). " +
                $"Call {nameof(GetSignedClientAsync)} instead — the sync overload cannot resolve vault credentials.");

        var name = NormalizeName(network);
        var rpcUrl = RequirePlainRpcUrl(name, config);
        var account = new Account(privateKeyHex, config.ChainID);
        var rpcClient = CreateRpcClient(rpcUrl);
        return new Web3(account, rpcClient);
    }

    // ── Internals ────────────────────────────────────────────────────────────

    private string NormalizeName(string? network) =>
        string.IsNullOrWhiteSpace(network) ? _options.DefaultNetwork : network.Trim();

    private static string RequirePlainRpcUrl(string network, EthereumNetworkConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.RpcUrl))
            throw new InvalidOperationException(
                $"Ethereum network '{network}' has no RpcUrl configured and no RpcUrlCredentialId. Set exactly one.");
        return config.RpcUrl;
    }

    private async Task<string> ResolveRpcUrlAsync(string network, EthereumNetworkConfig config, CancellationToken cancellationToken)
    {
        if (_resolvedRpcUrls.TryGetValue(network, out var cached)) return cached;

        // Fast path — plain URL, no vault access needed.
        if (config.RpcUrlCredentialId is null)
        {
            var plain = RequirePlainRpcUrl(network, config);
            WarnIfInsecureScheme(network, plain);
            _resolvedRpcUrls[network] = plain;
            return plain;
        }

        // Vault path — serialize the first resolution per-process to avoid multiple concurrent lookups.
        await _resolutionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_resolvedRpcUrls.TryGetValue(network, out cached)) return cached;

            using var scope = _scopeFactory.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<ICredentialResolver>();

            var credentialId = config.RpcUrlCredentialId!.Value;
            var url = await resolver.GetServiceUrlAsync(credentialId, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException(
                    $"Ethereum network '{network}' RpcUrlCredentialId {credentialId} did not resolve to a value. " +
                    $"Check that the credential exists, is of SERVICE_URL type, and its Service URL field contains the full RPC URL.");

            WarnIfInsecureScheme(network, url);
            _resolvedRpcUrls[network] = url;
            return url;
        }
        finally
        {
            _resolutionLock.Release();
        }
    }

    private void WarnIfInsecureScheme(string network, string rpcUrl)
    {
        if (rpcUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return;
        if (IsLocalhostUrl(rpcUrl)) return; // Anvil/Hardhat/Ganache — http://localhost:8545 is fine in dev.

        _logger.LogWarning(
            "Ethereum network '{Network}' RPC URL does not use HTTPS. Signed transactions and wallet addresses will be transmitted in plaintext. Confirm this is not a production endpoint.",
            network);
    }

    private static bool IsLocalhostUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        var host = uri.Host;
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || host == "127.0.0.1"
            || host == "::1"
            || string.Equals(host, "host.docker.internal", StringComparison.OrdinalIgnoreCase);
    }

    private IClient CreateRpcClient(string rpcUrl)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        return new RpcClient(new Uri(rpcUrl), httpClient);
    }
}
