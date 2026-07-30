namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of GAS02 — gas/price (legacy eth_gasPrice).</summary>
public sealed record EthereumGasPriceResult(
    bool Success,
    string GasPriceWei,
    decimal GasPriceGwei,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumGasPriceResult Ok(string gasPriceWei, decimal gasPriceGwei) =>
        new(true, gasPriceWei, gasPriceGwei, string.Empty, string.Empty);

    public static EthereumGasPriceResult Fail(string errorCode, string errorMessage) =>
        new(false, "0", 0m, errorCode, errorMessage);
}
