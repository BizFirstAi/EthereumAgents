using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_05 — token/allowance.</summary>
internal sealed class TokenAllowanceInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string? Owner { get; private set; }
    public string? Spender { get; private set; }
    public bool FormatDecimals { get; private set; } = true;

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        Owner = reader.ReadConfigByKeyDefaultNull("owner");
        Spender = reader.ReadConfigByKeyDefaultNull("spender");
        FormatDecimals = reader.ReadConfigByKeyBool("formatDecimals", true);
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/allowance.");
        if (string.IsNullOrWhiteSpace(Owner)) return ("VAL_MISSING_OWNER", "Config key 'owner' is required for token/allowance.");
        if (string.IsNullOrWhiteSpace(Spender)) return ("VAL_MISSING_SPENDER", "Config key 'spender' is required for token/allowance.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["owner"] = Owner ?? string.Empty,
        ["spender"] = Spender ?? string.Empty,
    };
}
