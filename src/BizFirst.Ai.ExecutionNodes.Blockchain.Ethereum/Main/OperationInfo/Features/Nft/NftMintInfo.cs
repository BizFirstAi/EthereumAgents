using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// NFT10 — nft/mint (addendum v3). Not part of EIP-721 — 'tokenId' supplied assumes
/// `{functionName}(address,uint256)`; omitted assumes `{functionName}(address)` (auto-incrementing
/// mint). 'functionName' defaults to "safeMint". Signing key resolved via vault (credentialID).
/// </summary>
internal sealed class NftMintInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? To { get; private set; }
    public string? TokenID { get; private set; }
    public string FunctionName { get; private set; } = "safeMint";

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        To = reader.ReadConfigByKeyDefaultNull("to");
        TokenID = reader.ReadConfigByKeyDefaultNull("tokenId");
        FunctionName = reader.ReadConfigByKey("functionName", "safeMint") ?? "safeMint";
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/mint.");
        if (string.IsNullOrWhiteSpace(To)) return ("VAL_MISSING_TO", "Config key 'to' is required for nft/mint.");
        if (TokenID is not null && !System.Numerics.BigInteger.TryParse(TokenID, out _)) return ("VAL_INVALID_TOKEN_ID", "Config key 'tokenId' must be an integer uint256 token ID when supplied.");
        if (string.IsNullOrWhiteSpace(FunctionName)) return ("VAL_MISSING_FUNCTION_NAME", "Config key 'functionName' must not be blank for nft/mint.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["to"] = To ?? string.Empty,
        ["tokenId"] = TokenID ?? string.Empty,
        ["functionName"] = FunctionName,
    };
}
