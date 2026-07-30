using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT09 — nft/tokenUri.</summary>
internal sealed class NftTokenUriInfo : BaseEthereumOperationInfo
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
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/tokenUri.");
        if (string.IsNullOrWhiteSpace(TokenID)) return ("VAL_MISSING_TOKEN_ID", "Config key 'tokenId' is required for nft/tokenUri.");
        if (!System.Numerics.BigInteger.TryParse(TokenID, out _)) return ("VAL_INVALID_TOKEN_ID", "Config key 'tokenId' must be an integer uint256 token ID.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["tokenId"] = TokenID ?? string.Empty,
    };
}
