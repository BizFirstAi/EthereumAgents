namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of AC02 — account/nonce.</summary>
public sealed record EthereumAccountNonceResult(bool Success, long Nonce, string ErrorCode, string ErrorMessage)
{
    public static EthereumAccountNonceResult Ok(long nonce) => new(true, nonce, string.Empty, string.Empty);
    public static EthereumAccountNonceResult Fail(string errorCode, string errorMessage) => new(false, 0, errorCode, errorMessage);
}
