using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// ERC20_10 — token/mint (addendum v3). Not part of EIP-20 — assumes the common
/// `mint(address,uint256)` shape, overridable via 'functionName'. Signing key resolved via vault (credentialID).
/// </summary>
internal sealed class TokenMintInfo : BaseEthereumOperationInfo
{
    public string? TokenAddress { get; private set; }
    public string? To { get; private set; }
    public string Amount { get; private set; } = "0";
    public string FunctionName { get; private set; } = "mint";

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TokenAddress = reader.ReadConfigByKeyDefaultNull("tokenAddress");
        To = reader.ReadConfigByKeyDefaultNull("to");
        Amount = reader.ReadConfigByKey("amount", "0");
        FunctionName = reader.ReadConfigByKey("functionName", "mint") ?? "mint";
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TokenAddress)) return ("VAL_MISSING_TOKEN_ADDRESS", "Config key 'tokenAddress' is required for token/mint.");
        if (string.IsNullOrWhiteSpace(To)) return ("VAL_MISSING_TO", "Config key 'to' is required for token/mint.");
        if (!System.Numerics.BigInteger.TryParse(Amount, out _)) return ("VAL_INVALID_AMOUNT", "Config key 'amount' must be an integer token-unit amount.");
        if (string.IsNullOrWhiteSpace(FunctionName)) return ("VAL_MISSING_FUNCTION_NAME", "Config key 'functionName' must not be blank for token/mint.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["tokenAddress"] = TokenAddress ?? string.Empty,
        ["to"] = To ?? string.Empty,
        ["amount"] = Amount,
        ["functionName"] = FunctionName,
    };
}
