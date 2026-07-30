using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>SIGN01 — wallet/signMessage (addendum v3, EIP-191 personal_sign). Signing key resolved via vault (credentialID).</summary>
internal sealed class WalletSignMessageInfo : BaseEthereumOperationInfo
{
    public string? Message { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Message = reader.ReadConfigByKeyDefaultNull("message");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Message) ? ("VAL_MISSING_MESSAGE", "Config key 'message' is required for wallet/signMessage.") : null;

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["message"] = Message ?? string.Empty,
    };
}
