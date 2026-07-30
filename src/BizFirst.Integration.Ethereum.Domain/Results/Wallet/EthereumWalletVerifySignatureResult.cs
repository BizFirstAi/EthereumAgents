namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of SIGN03 — wallet/verifySignature. No credential required — pure signature recovery.</summary>
public sealed record EthereumWalletVerifySignatureResult(bool Success, string Address, string ErrorCode, string ErrorMessage)
{
    public static EthereumWalletVerifySignatureResult Ok(string address) => new(true, address, string.Empty, string.Empty);
    public static EthereumWalletVerifySignatureResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
