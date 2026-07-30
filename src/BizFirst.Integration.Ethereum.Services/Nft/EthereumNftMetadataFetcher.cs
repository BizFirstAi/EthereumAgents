namespace BizFirst.Integration.Ethereum.Services;

/// <summary>
/// Resolves and fetches an NFT's tokenURI content (NFT12 — nft/getMetadata, addendum v3). This is
/// the Ethereum node's first outbound HTTP call that isn't a JSON-RPC request — the URI comes
/// straight from on-chain contract data, which an attacker (the contract's deployer) fully
/// controls, so this mirrors the SSRF-guard pattern already established in
/// <c>BizFirst.Integration.HttpRequest.Services.Http.HttpRequestService</c> (same blocked-host
/// list) rather than inventing a new security posture. No configurable allowlist here — this
/// fetcher only ever runs against a URI a smart contract emitted, never a workflow-author-supplied URL.
/// </summary>
public sealed class EthereumNftMetadataFetcher
{
    public const string HttpClientName = "EthereumNftMetadata";

    private static readonly string[] _ssrfBlockedHosts =
    [
        "localhost", "127.0.0.1", "::1",
        "169.254.169.254",
        "metadata.google.internal",
    ];

    private const string IpfsGateway = "https://ipfs.io/ipfs/";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EthereumNftMetadataFetcher> _logger;

    public EthereumNftMetadataFetcher(IHttpClientFactory httpClientFactory, ILogger<EthereumNftMetadataFetcher> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves ipfs:// URIs to the ipfs.io public gateway (hardcoded, not configurable in this
    /// pass — a known limitation, documented the same way as account/history's indexer gap), then
    /// fetches and returns the raw response body. Throws on SSRF block, non-2xx, or timeout —
    /// the caller (<see cref="EthereumNftService.GetMetadataAsync"/>) maps exceptions to error codes.
    /// </summary>
    public async Task<string> FetchAsync(string tokenUri, int timeoutSeconds, CancellationToken cancellationToken)
    {
        var resolvedUrl = ResolveUri(tokenUri);

        if (!Uri.TryCreate(resolvedUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new InvalidOperationException($"tokenURI '{tokenUri}' does not resolve to a fetchable http(s) URL.");

        await CheckSsrfAsync(uri, cancellationToken).ConfigureAwait(false);

        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

        var response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ResolveUri(string tokenUri) =>
        tokenUri.StartsWith("ipfs://", StringComparison.OrdinalIgnoreCase)
            ? IpfsGateway + tokenUri["ipfs://".Length..].TrimStart('/')
            : tokenUri;

    private async Task CheckSsrfAsync(Uri uri, CancellationToken cancellationToken)
    {
        var host = uri.Host.ToLowerInvariant();

        foreach (var blocked in _ssrfBlockedHosts)
        {
            if (host == blocked)
                throw new InvalidOperationException($"Requests to '{host}' are blocked for security reasons.");
        }

        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
            foreach (var ip in addresses)
            {
                if (IsPrivateIp(ip))
                    throw new InvalidOperationException("Requests to private/internal IP addresses are blocked for security reasons.");
            }
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            _logger.LogWarning(ex, "nft/getMetadata DNS resolution failed for {Host} — letting HttpClient surface the real error", host);
        }
    }

    private static bool IsPrivateIp(System.Net.IPAddress ip)
    {
        if (ip.Equals(System.Net.IPAddress.IPv6Loopback)) return true;
        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;

        var b = ip.GetAddressBytes();
        return b[0] == 10
            || b[0] == 127
            || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
            || (b[0] == 192 && b[1] == 168)
            || (b[0] == 169 && b[1] == 254);
    }
}
