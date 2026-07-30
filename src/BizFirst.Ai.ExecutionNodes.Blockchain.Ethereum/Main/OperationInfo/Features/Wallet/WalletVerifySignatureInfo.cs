using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SIGN03 — wallet/verifySignature (addendum v3). No credential required — pure signature recovery.</summary>
internal sealed class WalletVerifySignatureInfo : BaseEthereumOperationInfo
{
    public string? Message { get; private set; }
    public string? Signature { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Message = reader.ReadConfigByKeyDefaultNull("message");
        Signature = reader.ReadConfigByKeyDefaultNull("signature");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(Message)) return ("VAL_MISSING_MESSAGE", "Config key 'message' is required for wallet/verifySignature.");
        if (string.IsNullOrWhiteSpace(Signature)) return ("VAL_MISSING_SIGNATURE", "Config key 'signature' is required for wallet/verifySignature.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["message"] = Message ?? string.Empty,
        ["signature"] = Signature ?? string.Empty,
    };
}
