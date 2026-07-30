namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of ERC20_09 — token/symbol.</summary>
public sealed record EthereumTokenSymbolResult(bool Success, string Symbol, string ErrorCode, string ErrorMessage)
{
    public static EthereumTokenSymbolResult Ok(string symbol) => new(true, symbol, string.Empty, string.Empty);
    public static EthereumTokenSymbolResult Fail(string errorCode, string errorMessage) => new(false, string.Empty, errorCode, errorMessage);
}
