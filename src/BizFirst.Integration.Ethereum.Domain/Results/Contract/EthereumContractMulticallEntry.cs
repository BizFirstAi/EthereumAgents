namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// One call's outcome within SC05 — contract/multicall. A single failing call does NOT fail the whole
/// batch (partial success): <see cref="Success"/> is false and <see cref="ErrorCode"/>/<see cref="ErrorMessage"/>
/// are populated for just that entry, while the rest of the batch's results are still returned.
/// </summary>
public sealed record EthereumContractMulticallEntry(
    bool Success,
    string ContractAddress,
    string FunctionName,
    object? Result,
    IReadOnlyList<string> DecodedResult,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static EthereumContractMulticallEntry Ok(string contractAddress, string functionName, object? result, IReadOnlyList<string> decodedResult) =>
        new(true, contractAddress, functionName, result, decodedResult, null, null);

    public static EthereumContractMulticallEntry Fail(string contractAddress, string functionName, string errorCode, string errorMessage) =>
        new(false, contractAddress, functionName, null, Array.Empty<string>(), errorCode, errorMessage);
}
