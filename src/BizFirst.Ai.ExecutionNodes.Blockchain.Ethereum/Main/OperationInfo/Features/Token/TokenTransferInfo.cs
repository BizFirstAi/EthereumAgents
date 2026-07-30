using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_02 — token/transfer. Most popular operation (stablecoin payments). Signing key resolved via vault (credentialID).</summary>
internal sealed class TokenTransferInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string? To { get; private set; }
    public string Amount { get; private set; } = "0";

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        To = reader.ReadConfigByKeyDefaultNull("to");
        Amount = reader.ReadConfigByKey("amount", "0");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/transfer.");
        if (string.IsNullOrWhiteSpace(To)) return ("VAL_MISSING_TO", "Config key 'to' is required for token/transfer.");
        if (!System.Numerics.BigInteger.TryParse(Amount, out _)) return ("VAL_INVALID_AMOUNT", "Config key 'amount' must be an integer token-unit amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["to"] = To ?? string.Empty,
        ["amount"] = Amount,
    };
}
