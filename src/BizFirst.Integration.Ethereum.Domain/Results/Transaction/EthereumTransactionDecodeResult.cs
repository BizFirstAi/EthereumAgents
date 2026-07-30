namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of TX05 — transaction/decode.</summary>
public sealed record EthereumTransactionDecodeResult(
    bool Success,
    string FunctionName,
    string FunctionSignature,
    IReadOnlyDictionary<string, string> DecodedArgs,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumTransactionDecodeResult Ok(string functionName, string functionSignature, IReadOnlyDictionary<string, string> decodedArgs) =>
        new(true, functionName, functionSignature, decodedArgs, string.Empty, string.Empty);

    public static EthereumTransactionDecodeResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, string.Empty, new Dictionary<string, string>(), errorCode, errorMessage);
}
