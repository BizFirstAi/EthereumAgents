namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_08 — token/name.</summary>
public sealed record EthereumTokenNameResult(bool Success, string Name, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenNameResult Ok(string name) => new(true, name, string.Empty, string.Empty);
    public static EthereumTokenNameResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
