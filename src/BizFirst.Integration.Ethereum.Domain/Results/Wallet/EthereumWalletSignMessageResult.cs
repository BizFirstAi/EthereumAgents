namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of SIGN01 — wallet/signMessage (EIP-191 personal_sign).</summary>
public sealed record EthereumWalletSignMessageResult(bool Success, string Address, string Signature, string ErrorCode, string ErrorMessage)
{
    public static EthereumWalletSignMessageResult Ok(string address, string signature) => new(true, address, signature, string.Empty, string.Empty);
    public static EthereumWalletSignMessageResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
