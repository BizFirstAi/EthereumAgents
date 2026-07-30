using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>NFT03 — nft/transferFrom. Signing key resolved via vault (credentialID).</summary>
internal sealed class NftTransferFromInfo : BaseEthereumOperationInfo
{
    public string? ContractAddress { get; private set; }
    public string? From { get; private set; }
    public string? To { get; private set; }
    public string? TokenID { get; private set; }

    public override void LoadFrom(ConfigDataPropertyBag reader)
    {
        base.LoadFrom(reader);
        ContractAddress = reader.ReadConfigByKeyDefaultNull("contractAddress");
        From = reader.ReadConfigByKeyDefaultNull("from");
        To = reader.ReadConfigByKeyDefaultNull("to");
        TokenID = reader.ReadConfigByKeyDefaultNull("tokenId");
    }

    public (string Code, string Message)? Validate()
    {
        if (string.IsNullOrWhiteSpace(ContractAddress)) return ("VAL_MISSING_CONTRACT_ADDRESS", "Config key 'contractAddress' is required for nft/transferFrom.");
        if (string.IsNullOrWhiteSpace(From)) return ("VAL_MISSING_FROM", "Config key 'from' is required for nft/transferFrom.");
        if (string.IsNullOrWhiteSpace(To)) return ("VAL_MISSING_TO", "Config key 'to' is required for nft/transferFrom.");
        if (string.IsNullOrWhiteSpace(TokenID)) return ("VAL_MISSING_TOKEN_ID", "Config key 'tokenId' is required for nft/transferFrom.");
        if (!System.Numerics.BigInteger.TryParse(TokenID, out _)) return ("VAL_INVALID_TOKEN_ID", "Config key 'tokenId' must be an integer uint256 token ID.");
        return null;
    }

    public override Dictionary<string, object> ToDictionary() => new()
    {
        ["contractAddress"] = ContractAddress ?? string.Empty,
        ["from"] = From ?? string.Empty,
        ["to"] = To ?? string.Empty,
        ["tokenId"] = TokenID ?? string.Empty,
    };
}
