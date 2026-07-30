namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of UTIL06 — utility/encodeEventTopics. <see cref="Topics"/>[0] is always the event
/// signature hash (topic0); subsequent entries are either an encoded filter value or null,
/// matching the eth_getLogs topics-array wildcard convention.
/// </summary>
public sealed record EthereumUtilityEncodeEventTopicsResult(
    bool Success,
    IReadOnlyList<string?> Topics,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityEncodeEventTopicsResult Ok(IReadOnlyList<string?> topics) =>
        new(true, topics, string.Empty, string.Empty);

    public static EthereumUtilityEncodeEventTopicsResult Fail(string errorCode, string errorMessage) =>
        new(false, Array.Empty<string?>(), errorCode, errorMessage);
}
