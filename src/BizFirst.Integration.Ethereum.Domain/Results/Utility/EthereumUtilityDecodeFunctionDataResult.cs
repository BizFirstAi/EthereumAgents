namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of UTIL05 — utility/decodeFunctionData. Mirrors TX05 (transaction/decode) shape.</summary>
public sealed record EthereumUtilityDecodeFunctionDataResult(
    bool Success,
    string FunctionName,
    string Selector,
    IReadOnlyDictionary<string, string> DecodedArgs,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityDecodeFunctionDataResult Ok(string functionName, string selector, IReadOnlyDictionary<string, string> decodedArgs) =>
        new(true, functionName, selector, decodedArgs, string.Empty, string.Empty);

    public static EthereumUtilityDecodeFunctionDataResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, new Dictionary<string, string>(), errorCode, errorMessage);
}
