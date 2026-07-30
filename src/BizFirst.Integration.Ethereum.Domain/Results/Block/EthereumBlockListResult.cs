namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of BL03 — block/list. Composite operation: fans out to N Get Block calls, not a single RPC primitive.</summary>
public sealed record EthereumBlockListResult(bool Success, IReadOnlyList<EthereumBlockSummary> Blocks, string ErrorCode, string ErrorMessage)
{
    public static EthereumBlockListResult Ok(IReadOnlyList<EthereumBlockSummary> blocks) => new(true, blocks, string.Empty, string.Empty);
    public static EthereumBlockListResult Fail(string errorCode, string errorMessage) => new(false, Array.Empty<EthereumBlockSummary>(), errorCode, errorMessage);
}
