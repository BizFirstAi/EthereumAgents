namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of SC04 — contract/deploy. Broadcasts a contract-creation transaction; signing key resolved
/// via vault (credentialID). Carries the deployment TRANSACTION hash only — resolving the deployed
/// CONTRACT address requires waiting for and inspecting the transaction receipt, which is a separate,
/// heavier operation (see transaction/wait + transaction/receipt) intentionally not folded in here so
/// contract/deploy stays a fast, one-shot broadcast like every other action-node write operation.
/// </summary>
public sealed record EthereumContractDeployResult(bool Success, string TransactionHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumContractDeployResult Ok(string transactionHash) =>
        new(true, transactionHash, string.Empty, string.Empty);

    public static EthereumContractDeployResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, errorCode, errorMessage);
}
