namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_06 — token/totalSupply.</summary>
public sealed record EthereumTokenTotalSupplyResult(bool Success, string RawTotalSupply, decimal? FormattedTotalSupply, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenTotalSupplyResult Ok(string rawTotalSupply, decimal? formattedTotalSupply) => new(true, rawTotalSupply, formattedTotalSupply, string.Empty, string.Empty);
    public static EthereumTokenTotalSupplyResult Fail(string errorCode, string errorMessage) => new(false, "0", null, errorCode, errorMessage);
}
