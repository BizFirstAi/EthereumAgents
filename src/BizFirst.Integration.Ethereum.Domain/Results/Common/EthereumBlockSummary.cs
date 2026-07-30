namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Block metadata shared by all Block-resource operations.</summary>
public sealed record EthereumBlockSummary(
    long Number,
    string Hash,
    string ParentHash,
    long TimestampUnix,
    string Miner,
    string GasLimit,
    string GasUsed,
    int TransactionCount,
    IReadOnlyList<string>? TransactionHashes);
