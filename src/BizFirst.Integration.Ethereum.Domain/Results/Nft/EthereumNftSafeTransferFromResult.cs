namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT04 — nft/safeTransferFrom.</summary>
public sealed record EthereumNftSafeTransferFromResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftSafeTransferFromResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumNftSafeTransferFromResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
