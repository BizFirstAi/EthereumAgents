namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>One transaction row as returned by AC03 — account/history.</summary>
public sealed record EthereumTransactionHistoryRecord(
    string Hash,
    long BlockNumber,
    string From,
    string To,
    string ValueWei,
    long TimestampUnix,
    bool IsError);
