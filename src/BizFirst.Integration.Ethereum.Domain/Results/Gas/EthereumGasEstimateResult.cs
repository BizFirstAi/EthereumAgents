namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of GAS01 — gas/estimate (eth_estimateGas).</summary>
public sealed record EthereumGasEstimateResult(
    bool Success,
    string GasUnits,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumGasEstimateResult Ok(string gasUnits) =>
        new(true, gasUnits, string.Empty, string.Empty);

    public static EthereumGasEstimateResult Fail(string errorCode, string errorMessage) =>
        new(false, "0", errorCode, errorMessage);
}
