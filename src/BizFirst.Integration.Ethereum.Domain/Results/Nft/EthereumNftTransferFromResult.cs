namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of NFT03 — nft/transferFrom.</summary>
public sealed record EthereumNftTransferFromResult(bool Success, string TxHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumNftTransferFromResult Ok(string txHash) => new(true, txHash, string.Empty, string.Empty);
    public static EthereumNftTransferFromResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
