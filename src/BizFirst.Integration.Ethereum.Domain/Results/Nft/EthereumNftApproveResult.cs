namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT05 — nft/approve.</summary>
public sealed record EthereumNftApproveResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftApproveResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumNftApproveResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
