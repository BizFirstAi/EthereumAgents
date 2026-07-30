using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>TX03 — transaction/receipt (renamed from v1's "transaction/status" — real eth_getTransactionReceipt primitive).</summary>
internal sealed class TransactionReceiptInfo : BaseEthereumOperationInfo
{
    public string? Hash { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Hash = reader.ReadConfigByKeyDefaultNull("hash");
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Hash) ? ("VAL_MISSING_HASH", "Config key 'hash' is required for transaction/receipt.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["hash"] = Hash ?? string.Empty };
}
