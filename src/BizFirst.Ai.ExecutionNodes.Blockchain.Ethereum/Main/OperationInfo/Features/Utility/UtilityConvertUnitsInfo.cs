using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>UTIL02 — utility/convertUnits.</summary>
internal sealed class UtilityConvertUnitsInfo : BaseEthereumOperationInfo
{
    public string? Value { get; private set; }
    public string? FromUnit { get; private set; }
    public string? ToUnit { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Value = reader.ReadConfigByKeyDefaultNull("value");
        FromUnit = reader.ReadConfigByKeyDefaultNull("fromUnit");
        ToUnit = reader.ReadConfigByKeyDefaultNull("toUnit");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Value)) return ("VAL_MISSING_VALUE", "Config key 'value' is required for utility/convertUnits.");
        if (string.IsNullOrWhiteSpace(FromUnit)) return ("VAL_MISSING_FROM_UNIT", "Config key 'fromUnit' is required for utility/convertUnits.");
        if (string.IsNullOrWhiteSpace(ToUnit)) return ("VAL_MISSING_TO_UNIT", "Config key 'toUnit' is required for utility/convertUnits.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["value"] = Value ?? string.Empty,
        ["fromUnit"] = FromUnit ?? string.Empty,
        ["toUnit"] = ToUnit ?? string.Empty,
    };
}
