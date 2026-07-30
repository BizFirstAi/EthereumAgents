namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of SC03 — contract/simulate. Previews what a (typically state-changing) contract function
/// would return or revert with, WITHOUT broadcasting a transaction (an eth_call, optionally with a
/// caller address override). Shape mirrors <see cref="EthereumContractReadResult"/>: decoded outputs
/// are carried as strings since the ABI's actual output types are only known at runtime.
/// </summary>
public sealed record EthereumContractSimulateResult(
    bool Success,
    object? Result,
    IReadOnlyList<string> DecodedResult,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumContractSimulateResult Ok(object? result, IReadOnlyList<string> decodedResult) =>
        new(true, result, decodedResult, string.Empty, string.Empty);

    public static EthereumContractSimulateResult Fail(string errorCode, string errorMessage) =>
        new(false, null, Array.Empty<string>(), errorCode, errorMessage);
}
