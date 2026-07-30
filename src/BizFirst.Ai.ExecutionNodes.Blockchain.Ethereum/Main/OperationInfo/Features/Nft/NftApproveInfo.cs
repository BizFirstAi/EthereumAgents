using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT05 — nft/approve. Signing key resolved via vault (credentialID).</summary>
internal sealed class NftApproveInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? Spender { get; private set; }
    public string? TokenID { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        Spender = reader.ReadConfigByKeyDefaultNull("spender");
        TokenID = reader.ReadConfigByKeyDefaultNull("tokenId");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/approve.");
        if (string.IsNullOrWhiteSpace(Spender)) return ("VAL_MISSING_SPENDER", "Config key 'spender' is required for nft/approve.");
        if (string.IsNullOrWhiteSpace(TokenID)) return ("VAL_MISSING_TOKEN_ID", "Config key 'tokenId' is required for nft/approve.");
        if (!System.Numerics.BigInteger.TryParse(TokenID, out _)) return ("VAL_INVALID_TOKEN_ID", "Config key 'tokenId' must be an integer uint256 token ID.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["spender"] = Spender ?? string.Empty,
        ["tokenId"] = TokenID ?? string.Empty,
    };
}
