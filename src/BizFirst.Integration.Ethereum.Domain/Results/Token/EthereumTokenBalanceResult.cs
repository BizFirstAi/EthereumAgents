namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_01 — token/balance.</summary>
public sealed record EthereumTokenBalanceResult(bool Success, string RawBalance, decimal? FormattedBalance, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenBalanceResult Ok(string rawBalance, decimal? formattedBalance) => new(true, rawBalance, formattedBalance, string.Empty, string.Empty);
    public static EthereumTokenBalanceResult Fail(string errorCode, string errorMessage) => new(false, "0", null, errorCode, errorMessage);
}
