namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ENS02 — ens/reverse. Reverse resolution: address → ENS name via the Reverse Registrar.</summary>
public sealed record EthereumEnsReverseResult(
    bool Success,
    string Name,
    string ResolverAddress,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumEnsReverseResult Ok(string name, string resolverAddress) =>
        new(true, name, resolverAddress, string.Empty, string.Empty);

    public static EthereumEnsReverseResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
