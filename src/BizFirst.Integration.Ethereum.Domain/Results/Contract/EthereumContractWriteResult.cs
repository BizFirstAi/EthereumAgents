namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of SC02 — contract/write. Sends a state-changing transaction to a contract function; signing key resolved via vault (credentialID).</summary>
public sealed record EthereumContractWriteResult(bool Success, string TransactionHash, string ErrorCode, string ErrorMessage)
{
    public static EthereumContractWriteResult Ok(string transactionHash) =>
        new(true, transactionHash, string.Empty, string.Empty);

    public static EthereumContractWriteResult Fail(string errorCode, string errorMessage) =>
        new(false, string.Empty, errorCode, errorMessage);
}
