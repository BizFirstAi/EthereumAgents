// <summary>
// Code review guidelines: 020_NodeServerProject-Engineer/Guidelines/14_node-executor-integration-code/guideline.md
// </summary>
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

//IMPORTANT: "code-step" comments must not be changed. This is a coding checklist used as a template.
// CORE OPERATION: transaction/send moves real value on-chain. Requires a signing private key
// resolved from the vault (credentialID) via _ResolveSigningKeyAsync — never a config field.
public sealed partial class EthereumNodeExecutor
{
    private async Task<NodeExecutionResult> _Ethereum_Transaction_Send_Async(
        NodeExecutionContext nodeExecutionContext,
        CancellationToken cancellationToken = default)
    {
        //code-step: 1.1 - Validate settings exist and cast to TransactionSendInfo
        if (mySettings?.ActiveInfo is not TransactionSendInfo info)
            return SimpleErrorOperationUnfound();

        //code-step: 1.2 - Create result manager for output handling
        var resultManager = NodeResultOperateManager.CreateInstance(nodeExecutionContext);

        var error = info.Validate();
        if (error is not null)
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, error.Value.Message, this);

        // Resolve the wallet signing key from the vault before attempting to send.
        var (privateKey, credError) = await _ResolveSigningKeyAsync(cancellationToken);
        if (credError is not null)
            return credError;

        try
        {
            //code-step: 1.3 - Call Ethereum transaction service to send transaction
            var r = await _transactionService.SendTransactionAsync(
                info.Network,
                privateKey!,
                info.To!,
                System.Numerics.BigInteger.Parse(info.ValueWei),
                info.Data,
                info.GasLimit is null ? null : System.Numerics.BigInteger.Parse(info.GasLimit),
                info.MaxFeePerGasWei is null ? null : System.Numerics.BigInteger.Parse(info.MaxFeePerGasWei),
                info.MaxPriorityFeePerGasWei is null ? null : System.Numerics.BigInteger.Parse(info.MaxPriorityFeePerGasWei),
                info.Nonce,
                cancellationToken);

            if (!r.Success)
                return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, r.ErrorMessage, this);

            //code-step: 1.4 - Report progress milestone to execution context
            await ReportNodeProgress_ResourceOperation(nodeExecutionContext, "IntegrationCallCompleted");

            //code-step: 1.5 - Extract sent transaction record from result
            var transaction = new Dictionary<string, object> { { "txHash", r.TxHash } };

            //code-step: 1.6 - Build output metadata dictionary
            var outputData = resultManager.GetOrCreateOutputData();
            outputData["status"] = "success";
            outputData["resource"] = "transaction";
            outputData["operation"] = "send";

            //code-step: 1.7 - Convert sent transaction record to standard items array
            outputData.TryGetValue(ExecutionConstants.OutputFieldNameConstants.CONST_items, out var existingItemsValue);
            outputData[ExecutionConstants.OutputFieldNameConstants.CONST_items] = ApplyOutputItemsMerge(existingItemsValue, WrapJsonIntoItems(transaction, nodeExecutionContext));

            //code-step: 1.8 - Write output (handles TargetDataPath writes + items downstream)
            return await WriteOutputData(ExecutionConstants.OutputPorts.Success, outputData, transaction, nodeExecutionContext, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            //code-step: 1.9 - Catch exceptions and return error with context
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, $"transaction/send failed for {info.To}: {ex.Message}", this);
        }
    }
}
