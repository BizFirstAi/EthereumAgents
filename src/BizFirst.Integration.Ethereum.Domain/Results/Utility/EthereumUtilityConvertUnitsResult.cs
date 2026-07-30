namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of UTIL02 — utility/convertUnits. Conversion always chains through wei as the common base.</summary>
public sealed record EthereumUtilityConvertUnitsResult(
    bool Success,
    string ValueWei,
    decimal ConvertedValue,
    string FromUnit,
    string ToUnit,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumUtilityConvertUnitsResult Ok(string valueWei, decimal convertedValue, string fromUnit, string toUnit) =>
        new(true, valueWei, convertedValue, fromUnit, toUnit, string.Empty, string.Empty);

    public static EthereumUtilityConvertUnitsResult Fail(string errorCode, string errorMessage) =>
        new(false, "0", 0m, string.Empty, string.Empty, errorCode, errorMessage);
}
