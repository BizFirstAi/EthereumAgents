using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// ERC20_11 — token/burn (addendum v3). Matches the real OpenZeppelin ERC20Burnable extension:
/// 'from' omitted burns the caller's own balance; supplied burns via allowance (burnFrom).
/// Signing key resolved via vault (credentialID).
/// </summary>
internal sealed class TokenBurnInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string Amount { get; private set; } = "0";
    public string? From { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        Amount = reader.ReadConfigByKey("amount", "0");
        From = reader.ReadConfigByKeyDefaultNull("from");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/burn.");
        if (!System.Numerics.BigInteger.TryParse(Amount, out _)) return ("VAL_INVALID_AMOUNT", "Config key 'amount' must be an integer token-unit amount.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["amount"] = Amount,
        ["from"] = From ?? string.Empty,
    };
}
