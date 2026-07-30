namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of AC01 — account/balance.</summary>
public sealed record EthereumAccountBalanceResult(
    bool Success,
    string BalanceWei,
    decimal BalanceFormatted,
    string Unit,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumAccountBalanceResult Ok(string balanceWei, decimal balanceFormatted, string unit) =>
        new(true, balanceWei, balanceFormatted, unit, string.Empty, string.Empty);

    public static EthereumAccountBalanceResult Fail(string errorCode, string errorMessage) =>
        new(false, "0", 0m, "wei", errorCode, errorMessage);
}
