namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of ENS03 — ens/avatar. Shorthand for the "avatar" text record (ENSIP-12). An empty
/// <see cref="AvatarUri"/> with <see cref="Success"/> true means the name resolves but has no avatar
/// text record set — that is normal ENS behaviour, not a failure.
/// </summary>
public sealed record EthereumEnsAvatarResult(
    bool Success,
    string AvatarUri,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumEnsAvatarResult Ok(string avatarUri) =>
        new(true, avatarUri, string.Empty, string.Empty);

    public static EthereumEnsAvatarResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, errorCode, errorMessage);
}
