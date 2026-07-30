namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of UTIL03 — utility/chainId (eth_chainId). The only Utility operation that makes an RPC call.</summary>
public sealed record EthereumUtilityChainIdResult(bool Success, long ChainID, string ErrorCode, string ErrorMessage)
{
    public static EthereumUtilityChainIdResult Ok(long chainID) => new(true, chainID, string.Empty, string.Empty);
    public static EthereumUtilityChainIdResult Fail(string errorCode, string errorMessage) => new(false, 0, errorCode, errorMessage);
}
