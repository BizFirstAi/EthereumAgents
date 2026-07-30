namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of ENS05 — ens/resolver. Returns only the resolver contract address registered for a name
/// (Registry.resolver(bytes32)) — useful for debugging resolution failures without going further to
/// query the resolver contract itself.
/// </summary>
public sealed record EthereumEnsResolverResult(
    bool Success,
    string ResolverAddress,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumEnsResolverResult Ok(string resolverAddress) =>
        new(true, resolverAddress, string.Empty, string.Empty);

    public static EthereumEnsResolverResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, errorCode, errorMessage);
}
