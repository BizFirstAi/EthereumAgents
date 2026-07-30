using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_01 — token/balance.</summary>
internal sealed class TokenBalanceInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string? OwnerAddress { get; private set; }
    public bool FormatDecimals { get; private set; } = true;

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        OwnerAddress = reader.ReadConfigByKeyDefaultNull("ownerAddress");
        FormatDecimals = reader.ReadConfigByKeyBool("formatDecimals", true);
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/balance.");
        if (string.IsNullOrWhiteSpace(OwnerAddress)) return ("VAL_MISSING_OWNER_ADDRESS", "Config key 'ownerAddress' is required for token/balance.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["ownerAddress"] = OwnerAddress ?? string.Empty,
    };
}
