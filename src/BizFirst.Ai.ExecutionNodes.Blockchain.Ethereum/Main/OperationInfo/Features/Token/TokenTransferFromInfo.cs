using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_04 — token/transferFrom.</summary>
internal sealed class TokenTransferFromInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string? From { get; private set; }
    public string? To { get; private set; }
    public string Amount { get; private set; } = "0";

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        From = reader.ReadConfigByKeyDefaultNull("from");
        To = reader.ReadConfigByKeyDefaultNull("to");
        Amount = reader.ReadConfigByKey("amount", "0");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/transferFrom.");
        if (string.IsNullOrWhiteSpace(From)) return ("VAL_MISSING_FROM", "Config key 'from' is required for token/transferFrom.");
        if (string.IsNullOrWhiteSpace(To)) return ("VAL_MISSING_TO", "Config key 'to' is required for token/transferFrom.");
        if (!System.Numerics.BigInteger.TryParse(Amount, out _)) return ("VAL_INVALID_AMOUNT", "Config key 'amount' must be an integer token-unit amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["from"] = From ?? string.Empty,
        ["to"] = To ?? string.Empty,
        ["amount"] = Amount,
    };
}
