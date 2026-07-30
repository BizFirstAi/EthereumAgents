namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ENS01 — ens/resolve. Forward resolution: ENS name → address.</summary>
public sealed record EthereumEnsResolveResult(
    bool Success,
    string Address,
    string ResolverAddress,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumEnsResolveResult Ok(string address, string resolverAddress) =>
        new(true, address, resolverAddress, string.Empty, string.Empty);

    public static EthereumEnsResolveResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
