namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_07 — token/decimals.</summary>
public sealed record EthereumTokenDecimalsResult(bool Success, int Decimals, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenDecimalsResult Ok(int decimals) => new(true, decimals, string.Empty, string.Empty);
    public static EthereumTokenDecimalsResult Fail(string errorCode, string errorMessage) => new(false, 0, errorCode, errorMessage);
}
