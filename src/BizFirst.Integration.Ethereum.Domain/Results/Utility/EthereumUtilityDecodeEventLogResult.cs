namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of UTIL07 — utility/decodeEventLog. LOAD-BEARING: this is what turns a raw
/// <see cref="EthereumEventLogEntry"/> (from a transaction receipt or eth_getLogs) into the
/// decoded event data other parts of the design promise. See EthereumUtilityService.DecodeEventLogAsync.
/// </summary>
public sealed record EthereumUtilityDecodeEventLogResult(
    bool Success,
    string EventName,
    IReadOnlyDictionary<string, string> DecodedArgs,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityDecodeEventLogResult Ok(string eventName, IReadOnlyDictionary<string, string> decodedArgs) =>
        new(true, eventName, decodedArgs, string.Empty, string.Empty);

    public static EthereumUtilityDecodeEventLogResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, new Dictionary<string, string>(), errorCode, errorMessage);
}
