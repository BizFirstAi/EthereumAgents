namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of GAS04 — gas/priorityFee (eth_maxPriorityFeePerGas). This is a non-standard-but-widely-supported
/// method with no strongly-typed Nethereum 6.0.0 wrapper — see EthereumGasService.GetMaxPriorityFeeAsync.
/// </summary>
public sealed record EthereumGasPriorityFeeResult(
    bool Success,
    string PriorityFeeWei,
    decimal PriorityFeeGwei,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumGasPriorityFeeResult Ok(string priorityFeeWei, decimal priorityFeeGwei) =>
        new(true, priorityFeeWei, priorityFeeGwei, string.Empty, string.Empty);

    public static EthereumGasPriorityFeeResult Fail(string errorCode, string errorMessage) =>
        new(false, "0", 0m, errorCode, errorMessage);
}
