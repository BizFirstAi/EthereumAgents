namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of SC06 — contract/logs. A one-shot historical eth_getLogs query for a single event type
/// (identified by the caller-supplied event ABI, which is hashed down to the topic0 filter) — distinct
/// from live event subscriptions, which are out of scope for this action-node resource and belong to a
/// separate trigger executor. Logs are returned raw/undecoded; pair with UTIL07 (utility/decodeEventLog)
/// to resolve <see cref="EthereumEventLogEntry.Topics"/>/<see cref="EthereumEventLogEntry.Data"/> into
/// named event arguments.
/// </summary>
public sealed record EthereumContractLogsResult(
    bool Success,
    IReadOnlyList<EthereumEventLogEntry> Logs,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumContractLogsResult Ok(IReadOnlyList<EthereumEventLogEntry> logs) =>
        new(true, logs, string.Empty, string.Empty);

    public static EthereumContractLogsResult Fail(string errorCode, string errorMessage) =>
        new(false, Array.Empty<EthereumEventLogEntry>(), errorCode, errorMessage);
}
