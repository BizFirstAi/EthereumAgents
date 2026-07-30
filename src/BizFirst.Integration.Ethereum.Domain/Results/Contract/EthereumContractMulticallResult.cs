namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of SC05 — contract/multicall. Batches multiple independent read calls (each potentially
/// against a different contract/ABI/function) into a single node execution. <see cref="Success"/>
/// reflects whether the batch itself could be parsed and executed at all (e.g. malformed "calls"
/// JSON fails the whole operation); individual call failures are partial and carried per-entry in
/// <see cref="Results"/> — see <see cref="EthereumContractMulticallEntry"/>.
/// </summary>
public sealed record EthereumContractMulticallResult(
    bool Success,
    IReadOnlyList<EthereumContractMulticallEntry> Results,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumContractMulticallResult Ok(IReadOnlyList<EthereumContractMulticallEntry> results) =>
        new(true, results, string.Empty, string.Empty);

    public static EthereumContractMulticallResult Fail(string errorCode, string errorMessage) =>
        new(false, Array.Empty<EthereumContractMulticallEntry>(), errorCode, errorMessage);
}
