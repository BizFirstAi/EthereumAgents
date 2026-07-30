namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of UTIL01 — utility/validateAddress.</summary>
public sealed record EthereumUtilityValidateAddressResult(
    bool Success,
    bool IsValid,
    bool IsChecksumValid,
    string ChecksummedAddress,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityValidateAddressResult Ok(bool isValid, bool isChecksumValid, string checksummedAddress) =>
        new(true, isValid, isChecksumValid, checksummedAddress, string.Empty, string.Empty);

    public static EthereumUtilityValidateAddressResult Fail(string errorCode, string errorMessage) =>
        new(false, false, false, string.Empty, errorCode, errorMessage);
}
