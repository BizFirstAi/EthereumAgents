namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// A single raw, undecoded event log entry (as returned by a transaction receipt or eth_getLogs).
/// Pair with UTIL07 (utility/decodeEventLog) plus an ABI to resolve <see cref="Topics"/>/<see cref="Data"/>
/// into named event arguments — this type intentionally carries no decoded fields.
/// </summary>
public sealed record EthereumEventLogEntry(
    string Address,
    IReadOnlyList<string> Topics,
    string Data,
    long? BlockNumber,
    string? TransactionHash,
    int? LogIndex);
