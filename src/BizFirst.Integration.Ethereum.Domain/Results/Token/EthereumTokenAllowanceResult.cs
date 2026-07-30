namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_05 — token/allowance.</summary>
public sealed record EthereumTokenAllowanceResult(bool Success, string RawAllowance, decimal? FormattedAllowance, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenAllowanceResult Ok(string rawAllowance, decimal? formattedAllowance) => new(true, rawAllowance, formattedAllowance, string.Empty, string.Empty);
    public static EthereumTokenAllowanceResult Fail(string errorCode, string errorMessage) => new(false, "0", null, errorCode, errorMessage);
}
