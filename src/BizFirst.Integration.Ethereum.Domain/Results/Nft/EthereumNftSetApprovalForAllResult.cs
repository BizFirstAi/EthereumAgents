namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT06 — nft/setApprovalForAll.</summary>
public sealed record EthereumNftSetApprovalForAllResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftSetApprovalForAllResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumNftSetApprovalForAllResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
