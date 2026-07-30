// <summary>
// Code review guidelines: 020_NodeServerProject-Engineer/Guidelines/14_node-executor-integration-code/guideline.md
// </summary>
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

//IMPORTANT: "code-step" comments must not be changed. This is a coding checklist used as a template.
public sealed partial class EthereumNodeExecutor
{
    private async Task<NodeExecutionResult> _Ethereum_Contract_Simulate_Async(
        NodeExecutionContext nodeExecutionContext,
        CancellationToken cancellationToken = default)
    {
        //code-step: 1.1 - Validate settings exist and cast to ContractSimulateInfo
        if (mySettings?.ActiveInfo is not ContractSimulateInfo info)
            return SimpleErrorOperationUnfound();

        //code-step: 1.2 - Create result manager for output handling
        var resultManager = NodeResultOperateManager.CreateInstance(nodeExecutionContext);

        var error = info.Validate();
        if (error is not null)
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, error.Value.Message, this);

        try
        {
            //code-step: 1.3 - Call Ethereum contract service to simulate the function call
            var r = await _contractService.SimulateAsync(info.Network, info.ContractAddress!, info.Abi!, info.FunctionName!, info.FunctionArgs, info.Value, info.From, cancellationToken);

            if (!r.Success)
                return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, r.ErrorMessage, this);

            //code-step: 1.4 - Report progress milestone to execution context
            await ReportNodeProgress_ResourceOperation(nodeExecutionContext, "IntegrationCallCompleted");

            //code-step: 1.5 - Extract decoded simulation record from result
            var simulate = new Dictionary<string, object>
            {
                { "result", r.Result ?? string.Empty },
                { "decodedResult", r.DecodedResult },
            };

            //code-step: 1.6 - Build output metadata dictionary
            var outputData = resultManager.GetOrCreateOutputData();
            outputData["status"] = "success";
            outputData["resource"] = "contract";
            outputData["operation"] = "simulate";

            //code-step: 1.7 - Convert simulation record to standard items array
            outputData.TryGetValue(ExecutionConstants.OutputFieldNameConstants.CONST_items, out var existingItemsValue);
            outputData[ExecutionConstants.OutputFieldNameConstants.CONST_items] = ApplyOutputItemsMerge(existingItemsValue, WrapJsonIntoItems(simulate, nodeExecutionContext));

            //code-step: 1.8 - Write output (handles TargetDataPath writes + items downstream)
            return await WriteOutputData(ExecutionConstants.OutputPorts.Success, outputData, simulate, nodeExecutionContext, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            //code-step: 1.9 - Catch exceptions and return error with context
            return resultManager.SetResultAsError(ExecutionConstants.OutputPorts.Error, $"contract/simulate failed for {info.ContractAddress}.{info.FunctionName}: {ex.Message}", this);
        }
    }
}
