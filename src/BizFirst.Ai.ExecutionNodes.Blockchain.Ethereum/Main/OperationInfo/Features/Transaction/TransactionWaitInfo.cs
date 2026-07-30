using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>TX06 — transaction/wait. Poll until N confirmations or timeout.</summary>
internal sealed class TransactionWaitInfo : BaseEthereumOperationInfo
{
    public string? Hash { get; private set; }
    public int Confirmations { get; private set; } = 1;
    public int TimeoutMs { get; private set; } = 60000;

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        Hash = reader.ReadConfigByKeyDefaultNull("hash");
        Confirmations = reader.ReadConfigByKey_Int("confirmations", 1);
        TimeoutMs = reader.ReadConfigByKey_Int("timeoutMs", 60000);
    }

    public (string Code, string Message)? Validate() =>
        string.IsNullOrWhiteSpace(Hash) ? ("VAL_MISSING_HASH", "Config key 'hash' is required for transaction/wait.") : null;

    public override Dictionary<string, object> ToDictionary() => new() { ["hash"] = Hash ?? string.Empty };
}
