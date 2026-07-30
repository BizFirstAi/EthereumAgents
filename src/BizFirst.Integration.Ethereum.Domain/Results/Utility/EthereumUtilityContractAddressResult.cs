namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of UTIL08 — utility/contractAddress. Predicts a CREATE (nonce-based) or CREATE2 (salt-based) deployment address.</summary>
public sealed record EthereumUtilityContractAddressResult(
    bool Success,
    string ContractAddress,
    string Method,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityContractAddressResult Ok(string contractAddress, string method) =>
        new(true, contractAddress, method, string.Empty, string.Empty);

    public static EthereumUtilityContractAddressResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, errorCode, errorMessage);
}
