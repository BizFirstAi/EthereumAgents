// <summary>
// Code review guidelines: 020_NodeServerProject-Engineer/Guidelines/14_node-executor-integration-code/guideline.md
// </summary>
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

//IMPORTANT: "code-step" comments must not be changed. This is a coding checklist used as a template.
// STAR OPERATION: token/transfer is the most popular operation on this node (stablecoin
// payments). Requires a signing private key resolved from the vault (credentialID) via
// _ResolveSigningKeyAsync — never a config field.
public sealed partial class EthereumNodeExecutor
{
    private async Task<NodeExecutionResult> _Ethereum_Token_Transfer_Async(
        NodeExecutionContext nodeExecutionContext,
        CancellationToken cancellationToken = default)
    {
        //code-step: 1.1 - Validate settings exist and cast to TokenTransferInfo
        if (mySettings?.ActiveInfo is not TokenTransferInfo info)
            return SimpleErrorOperationUnfound();

        //code-step: 1.2 - Create result manager for output handling
        var resultManager = NodeResultOperateManager.CreateInstance(nodeExecutionContext);

        var error = info.Validate();
        if (error is not null)
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, error.Value.Message, this);

        // Resolve the wallet signing key from the vault before attempting the transfer.
        var (privateKey, credError) = await _ResolveSigningKeyAsync(cancellationToken);
        if (credError is not null)
            return credError;

        try
        {
            //code-step: 1.3 - Call Ethereum token service to transfer tokens
            var r = await _tokenService.TransferAsync(
                info.Network, privateKey!, info.TokenAddress!, info.To!, System.Numerics.BigInteger.Parse(info.Amount), cancellationToken);

            if (!r.Success)
                return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, r.ErrorMessage, this);

            //code-step: 1.4 - Report progress milestone to execution context
            await ReportNodeProgress_ResourceOperation(nodeExecutionContext, "IntegrationCallCompleted");

            //code-step: 1.5 - Extract token transfer record from result
            var transfer = new Dictionary<string, object> { { "txHash", r.TxHash } };

            //code-step: 1.6 - Build output metadata dictionary
            var outputData = resultManager.GetOrCreateOutputData();
            outputData["status"] = "success";
            outputData["resource"] = "token";
            outputData["operation"] = "transfer";

            //code-step: 1.7 - Convert token transfer record to standard items array
            outputData.TryGetValue(ExecutionConstants.OutputFieldNameConstants.CONST_items, out var existingItemsValue);
            outputData[ExecutionConstants.OutputFieldNameConstants.CONST_items] = ApplyOutputItemsMerge(existingItemsValue, WrapJsonIntoItems(transfer, nodeExecutionContext));

            //code-step: 1.8 - Write output (handles TargetDataPath writes + items downstream)
            return await WriteOutputData(ExecutionConstants.OutputPorts.Success, outputData, transfer, nodeExecutionContext, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            //code-step: 1.9 - Catch exceptions and return error with context
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, $"token/transfer failed for {info.TokenAddress} to {info.To}: {ex.Message}", this);
        }
    }
}
