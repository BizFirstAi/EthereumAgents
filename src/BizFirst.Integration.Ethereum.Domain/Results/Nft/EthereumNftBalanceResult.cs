namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT01 — nft/balance.</summary>
public sealed record EthereumNftBalanceResult(bool Success, string Balance, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftBalanceResult Ok(string balance) => new(true, balance, string.Empty, string.Empty);
    public static EthereumNftBalanceResult Fail(string errorCode, string errorMessage) => new(false, "0", errorCode, errorMessage);
}
