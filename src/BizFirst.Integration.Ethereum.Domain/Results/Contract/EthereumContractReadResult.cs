namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of SC01 — contract/read. Calls a view/pure contract function. Since the return shape is
/// only known from the caller-supplied ABI at runtime, decoded outputs are carried as strings:
/// <see cref="Result"/> is the first output value (or the full <see cref="DecodedResult"/> list when
/// the function returns more than one value), and <see cref="DecodedResult"/> always carries every
/// decoded output positionally, one string per ABI output parameter.
/// </summary>
public sealed record EthereumContractReadResult(
    bool Success,
    object? Result,
    IReadOnlyList<string> DecodedResult,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumContractReadResult Ok(object? result, IReadOnlyList<string> decodedResult) =>
        new(true, result, decodedResult, string.Empty, string.Empty);

    public static EthereumContractReadResult Fail(string errorCode, string errorMessage) =>
        new(false, null, Array.Empty<string>(), errorCode, errorMessage);
}
