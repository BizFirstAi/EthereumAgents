namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of BL01 — block/get.</summary>
public sealed record EthereumBlockGetResult(bool Success, EthereumBlockSummary? Block, string ErrorCode, string ErrorMessage)
{
    public static EthereumBlockGetResult Ok(EthereumBlockSummary block) => new(true, block, string.Empty, string.Empty);
    public static EthereumBlockGetResult Fail(string errorCode, string errorMessage) => new(false, null, errorCode, errorMessage);
}
