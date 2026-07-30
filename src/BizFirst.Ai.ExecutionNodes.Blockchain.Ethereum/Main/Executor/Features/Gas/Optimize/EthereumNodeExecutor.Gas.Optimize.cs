// <summary>
// Code review guidelines: 020_NodeServerProject-Engineer/Guidelines/14_node-executor-integration-code/guideline.md
// </summary>
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

//IMPORTANT: "code-step" comments must not be changed. This is a coding checklist used as a template.
public sealed partial class EthereumNodeExecutor
{
    private async Task<NodeExecutionResult> _Ethereum_Gas_Optimize_Async(
        NodeExecutionContext nodeExecutionContext,
        CancellationToken cancellationToken = default)
    {
        //code-step: 1.1 - Validate settings exist and cast to GasOptimizeInfo
        if (mySettings?.ActiveInfo is not GasOptimizeInfo info)
            return SimpleErrorOperationUnfound();

        //code-step: 1.2 - Create result manager for output handling
        var resultManager = NodeResultOperateManager.CreateInstance(nodeExecutionContext);

        var error = info.Validate();
        if (error is not null)
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, error.Value.Message, this);

        try
        {
            //code-step: 1.3 - Call Ethereum gas service to compute a recommended fee pair (composite of gas/price + gas/feeHistory + gas/priorityFee)
            var r = await _gasService.OptimizeTransactionAsync(info.Network, info.Operations, info.Strategy, cancellationToken);

            if (!r.Success)
                return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, r.ErrorMessage, this);

            //code-step: 1.4 - Report progress milestone to execution context
            await ReportNodeProgress_ResourceOperation(nodeExecutionContext, "IntegrationCallCompleted");

            //code-step: 1.5 - Extract fee recommendation record from result
            var recommendation = new Dictionary<string, object>
            {
                { "strategy", r.Strategy },
                { "baseFeePerGasWei", r.BaseFeePerGasWei },
                { "maxPriorityFeePerGasWei", r.MaxPriorityFeePerGasWei },
                { "maxFeePerGasWei", r.MaxFeePerGasWei },
                { "estimatedGasUnits", r.EstimatedGasUnits ?? string.Empty },
                { "estimatedTotalCostWei", r.EstimatedTotalCostWei ?? string.Empty },
            };

            //code-step: 1.6 - Build output metadata dictionary
            var outputData = resultManager.GetOrCreateOutputData();
            outputData["status"] = "success";
            outputData["resource"] = "gas";
            outputData["operation"] = "optimize";

            //code-step: 1.7 - Convert fee recommendation record to standard items array
            outputData.TryGetValue(ExecutionConstants.OutputFieldNameConstants.CONST_items, out var existingItemsValue);
            outputData[ExecutionConstants.OutputFieldNameConstants.CONST_items] = ApplyOutputItemsMerge(existingItemsValue, WrapJsonIntoItems(recommendation, nodeExecutionContext));

            //code-step: 1.8 - Write output (handles TargetDataPath writes + items downstream)
            return await WriteOutputData(ExecutionConstants.OutputPorts.Success, outputData, recommendation, nodeExecutionContext, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            //code-step: 1.9 - Catch exceptions and return error with context
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, $"gas/optimize failed on {info.Network}: {ex.Message}", this);
        }
    }
}
