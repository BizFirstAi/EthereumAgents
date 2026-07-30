namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of SIGN02 — wallet/signTypedData (EIP-712).</summary>
public sealed record EthereumWalletSignTypedDataResult(bool Success, string Address, string Signature, string ErrorCode, string ErrorMessage)
{
    public static EthereumWalletSignTypedDataResult Ok(string address, string signature) => new(true, address, signature, string.Empty, string.Empty);
    public static EthereumWalletSignTypedDataResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
