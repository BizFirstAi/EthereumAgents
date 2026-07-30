using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_06 — token/totalSupply.</summary>
internal sealed class TokenTotalSupplyInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public bool FormatDecimals { get; private set; } = true;

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        FormatDecimals = reader.ReadConfigByKeyBool("formatDecimals", true);
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(TokenAddress) ? ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/totalSupply.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["tokenAddress"] = TokenAddress ?? string.Empty };
}
