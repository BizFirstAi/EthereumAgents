namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of BL02 — block/number.</summary>
public sealed record EthereumBlockNumberResult(bool Success, long BlockNumber, string ErrorCode, string ErrorMessage)
{
    public static EthereumBlockNumberResult Ok(long blockNumber) => new(true, blockNumber, string.Empty, string.Empty);
    public static EthereumBlockNumberResult Fail(string errorCode, string errorMessage) => new(false, 0, errorCode, errorMessage);
}
