using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>ERC20_08 — token/name.</summary>
internal sealed class TokenNameInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(TokenAddress) ? ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/name.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["tokenAddress"] = TokenAddress ?? string.Empty };
}
