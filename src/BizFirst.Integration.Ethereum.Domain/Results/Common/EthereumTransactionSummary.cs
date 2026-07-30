namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Transaction metadata shared by Transaction-resource operations.</summary>
public sealed record EthereumTransactionSummary(
    string Hash,
    long? BlockNumber,
    string From,
    string? To,
    string ValueWei,
    string GasLimit,
    string? GasPriceWei,
    long Nonce,
    string Input,
    bool IsPending);
