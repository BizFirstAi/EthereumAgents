using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_03 — token/approve.</summary>
internal sealed class TokenApproveInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string? Spender { get; private set; }
    public string Amount { get; private set; } = "0";

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        Spender = reader.ReadConfigByKeyDefaultNull("spender");
        Amount = reader.ReadConfigByKey("amount", "0");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/approve.");
        if (string.IsNullOrWhiteSpace(Spender)) return ("VAL_MISSING_SPENDER", "Config key 'spender' is required for token/approve.");
        if (!System.Numerics.BigInteger.TryParse(Amount, out _)) return ("VAL_INVALID_AMOUNT", "Config key 'amount' must be an integer token-unit amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["spender"] = Spender ?? string.Empty,
        ["amount"] = Amount,
    };
}
