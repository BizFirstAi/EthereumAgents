using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// SIGN02 — wallet/signTypedData (addendum v3, EIP-712). Signing key resolved via vault (credentialID).
/// </summary>
internal sealed class WalletSignTypedDataInfo : BaseEthereumOperationInfo
{
    /// <summary>Full standard EIP-712 envelope as raw JSON: {types, primaryType, domain, message}.</summary>
    public string? TypedDataJson { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        TypedDataJson = reader.ReadConfigNodeByKey("typedDataJson")?.ToString();
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(TypedDataJson)) return ("VAL_MISSING_TYPED_DATA", "Config key 'typedDataJson' is required for wallet/signTypedData.");
        try
        {
            using var _ = System.Text.Json.JsonDocument.Parse(TypedDataJson);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return ("VAL_INVALID_TYPED_DATA", $"Config key 'typedDataJson' is not valid JSON: {ex.Message}");
        }
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["typedDataJson"] = TypedDataJson ?? string.Empty,
    };
}
