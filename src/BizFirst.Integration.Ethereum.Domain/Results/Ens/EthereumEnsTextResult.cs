namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of ENS04 — ens/text (EIP-634 / ENSIP-5 text records, e.g. "email", "url", "com.twitter").
/// An empty <see cref="Value"/> with <see cref="Success"/> true means the name resolves but has no
/// value set for that key — that is normal ENS behaviour, not a failure.
/// </summary>
public sealed record EthereumEnsTextResult(
    bool Success,
    string Key,
    string Value,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumEnsTextResult Ok(string key, string value) =>
        new(true, key, value, string.Empty, string.Empty);

    public static EthereumEnsTextResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
