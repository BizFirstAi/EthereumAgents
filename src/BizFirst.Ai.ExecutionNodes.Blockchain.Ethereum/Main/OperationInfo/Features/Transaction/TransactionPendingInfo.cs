using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>TX04 — transaction/pending. Mempool visibility is RPC-provider-dependent.</summary>
internal sealed class TransactionPendingInfo : BaseEthereumOperationInfo
{
    public string? AddressFilter { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        AddressFilter = reader.ReadConfigByKeyDefaultNull("addressFilter");
    }

    public (string Code, string Message)? Validate() => null;

    public override Dictionary<string, object> ToDictionary() => new();
}
