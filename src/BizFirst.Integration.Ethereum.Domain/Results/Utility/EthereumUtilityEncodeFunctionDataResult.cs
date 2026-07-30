namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of UTIL04 — utility/encodeFunctionData.</summary>
public sealed record EthereumUtilityEncodeFunctionDataResult(
    bool Success,
    string Data,
    string Selector,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityEncodeFunctionDataResult Ok(string data, string selector) =>
        new(true, data, selector, string.Empty, string.Empty);

    public static EthereumUtilityEncodeFunctionDataResult Fail(string errorCode, string errorMessage) =>
        new(false, "0x", string.Empty, errorCode, errorMessage);
}
