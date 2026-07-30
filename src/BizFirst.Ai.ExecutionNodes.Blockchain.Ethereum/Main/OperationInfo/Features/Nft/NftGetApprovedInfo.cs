using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT07 — nft/getApproved.</summary>
internal sealed class NftGetApprovedInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? TokenID { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        TokenID = reader.ReadConfigByKeyDefaultNull("tokenId");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/getApproved.");
        if (string.IsNullOrWhiteSpace(TokenID)) return ("VAL_MISSING_TOKEN_ID", "Config key 'tokenId' is required for nft/getApproved.");
        if (!System.Numerics.BigInteger.TryParse(TokenID, out _)) return ("VAL_INVALID_TOKEN_ID", "Config key 'tokenId' must be an integer uint256 token ID.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["tokenId"] = TokenID ?? string.Empty,
    };
}
