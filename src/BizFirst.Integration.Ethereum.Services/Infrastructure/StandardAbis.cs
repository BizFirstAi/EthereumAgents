namespace BizFirst.Integration.Ethereum.Services;

/// <summary>Minimal standard ABIs used for generic contract calls, avoiding a dependency on the optional Nethereum.Contracts.Standards package.</summary>
internal static class StandardAbis
{
    /// <summary>ERC-20 (EIP-20) — the 9 functions this node calls. No mint/burn: those aren't part of the standard.</summary>
    public const string Erc20 = @"[
      {""constant"":true,""inputs"":[{""name"":""owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""spender"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""from"",""type"":""address""},{""name"":""to"",""type"":""address""},{""name"":""amount"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""},
      {""constant"":true,""inputs"":[{""name"":""owner"",""type"":""address""},{""name"":""spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""},
      {""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""},
      {""constant"":true,""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""type"":""function""},
      {""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""},
      {""constant"":true,""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""}
    ]";

    /// <summary>ERC-721 (EIP-721) — the 9 functions the NFT resource calls.</summary>
    public const string Erc721 = @"[
      {""constant"":true,""inputs"":[{""name"":""owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""},
      {""constant"":true,""inputs"":[{""name"":""tokenId"",""type"":""uint256""}],""name"":""ownerOf"",""outputs"":[{""name"":"""",""type"":""address""}],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""from"",""type"":""address""},{""name"":""to"",""type"":""address""},{""name"":""tokenId"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""from"",""type"":""address""},{""name"":""to"",""type"":""address""},{""name"":""tokenId"",""type"":""uint256""}],""name"":""safeTransferFrom"",""outputs"":[],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""tokenId"",""type"":""uint256""}],""name"":""approve"",""outputs"":[],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""operator"",""type"":""address""},{""name"":""approved"",""type"":""bool""}],""name"":""setApprovalForAll"",""outputs"":[],""type"":""function""},
      {""constant"":true,""inputs"":[{""name"":""tokenId"",""type"":""uint256""}],""name"":""getApproved"",""outputs"":[{""name"":"""",""type"":""address""}],""type"":""function""},
      {""constant"":true,""inputs"":[{""name"":""owner"",""type"":""address""},{""name"":""operator"",""type"":""address""}],""name"":""isApprovedForAll"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""},
      {""constant"":true,""inputs"":[{""name"":""tokenId"",""type"":""uint256""}],""name"":""tokenURI"",""outputs"":[{""name"":"""",""type"":""string""}],""type"":""function""}
    ]";

    /// <summary>
    /// ERC-165 (EIP-165) — the single `supportsInterface` lookup function. Used by SC07
    /// (contract/detectStandards) and NFT11 (nft/listOwnedTokens, to gate on Enumerable support).
    /// </summary>
    public const string Erc165 = @"[
      {""constant"":true,""inputs"":[{""name"":""interfaceId"",""type"":""bytes4""}],""name"":""supportsInterface"",""outputs"":[{""name"":"""",""type"":""bool""}],""type"":""function""}
    ]";

    /// <summary>
    /// ERC-20 Burnable extension (OpenZeppelin's real, widely-adopted convention — not part of
    /// EIP-20 itself, but standardized enough to hardcode, unlike Mint). Used by ERC20_11 (token/burn).
    /// </summary>
    public const string Erc20Burnable = @"[
      {""constant"":false,""inputs"":[{""name"":""value"",""type"":""uint256""}],""name"":""burn"",""outputs"":[],""type"":""function""},
      {""constant"":false,""inputs"":[{""name"":""account"",""type"":""address""},{""name"":""value"",""type"":""uint256""}],""name"":""burnFrom"",""outputs"":[],""type"":""function""}
    ]";

    /// <summary>
    /// ERC-721 Enumerable extension (interface ID 0x780e9d63) — `tokenOfOwnerByIndex`. `balanceOf`
    /// is already covered by <see cref="Erc721"/>. Used by NFT11 (nft/listOwnedTokens).
    /// </summary>
    public const string Erc721Enumerable = @"[
      {""constant"":true,""inputs"":[{""name"":""owner"",""type"":""address""},{""name"":""index"",""type"":""uint256""}],""name"":""tokenOfOwnerByIndex"",""outputs"":[{""name"":"""",""type"":""uint256""}],""type"":""function""}
    ]";

    /// <summary>
    /// Known ERC-165 interface IDs (verified against the real EIPs, 2026-07-21) — name to probe.
    /// ERC-20 is deliberately absent: EIP-20 predates ERC-165 and has no `supportsInterface` at
    /// all, so it can never be detected this way (see the ERC-20 heuristic probe instead).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Erc165KnownInterfaceIds = new Dictionary<string, string>
    {
        ["ERC-165"] = "0x01ffc9a7",
        ["ERC-721"] = "0x80ac58cd",
        ["ERC-721Metadata"] = "0x5b5e139f",
        ["ERC-721Enumerable"] = "0x780e9d63",
        ["ERC-1155"] = "0xd9b67a26",
        ["ERC-2981"] = "0x2a55205a",
    };

    /// <summary>
    /// Builds a minimal single-function ABI fragment for a mint call whose exact function name
    /// isn't standardized (ERC20_10/NFT10 — see <c>OPERATIONS_ADDENDUM_v3</c>). <paramref name="includeTokenId"/>
    /// selects between the 2-arg `(address,uint256)` and 1-arg `(address)` shapes.
    /// </summary>
    public static string BuildMintAbi(string functionName, bool includeTokenId)
    {
        var inputs = includeTokenId
            ? @"[{""name"":""to"",""type"":""address""},{""name"":""tokenIdOrAmount"",""type"":""uint256""}]"
            : @"[{""name"":""to"",""type"":""address""}]";
        return $@"[{{""constant"":false,""inputs"":{inputs},""name"":""{functionName}"",""outputs"":[],""type"":""function""}}]";
    }
}
