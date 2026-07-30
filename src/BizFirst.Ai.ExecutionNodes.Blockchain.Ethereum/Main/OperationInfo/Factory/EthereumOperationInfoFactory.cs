using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// Maps (resource, operation) config values to the concrete operation DTO, then loads it from
/// the raw config via <see cref="ConfigDataPropertyBag"/>. See the Verified Operation Catalog v2
/// in the design docs for the full 55-operation list this mirrors.
/// </summary>
internal static class EthereumOperationInfoFactory
{
    internal static BaseEthereumOperationInfo? Create(string? resource, string? operation, ConfigDataPropertyBag reader)
    {
        BaseEthereumOperationInfo? info = (resource, operation) switch
        {
            // ── account (4) ──────────────────────────────────────────────────
            ("account", "balance") => new AccountBalanceInfo(),
            ("account", "nonce")   => new AccountNonceInfo(),
            ("account", "history") => new AccountHistoryInfo(),
            ("account", "code")    => new AccountCodeInfo(),

            // ── block (3) ────────────────────────────────────────────────────
            ("block", "get")    => new BlockGetInfo(),
            ("block", "number") => new BlockNumberInfo(),
            ("block", "list")   => new BlockListInfo(),

            // ── transaction (6) ──────────────────────────────────────────────
            ("transaction", "get")     => new TransactionGetInfo(),
            ("transaction", "send")    => new TransactionSendInfo(),
            ("transaction", "receipt") => new TransactionReceiptInfo(),
            ("transaction", "pending") => new TransactionPendingInfo(),
            ("transaction", "decode")  => new TransactionDecodeInfo(),
            ("transaction", "wait")    => new TransactionWaitInfo(),

            // ── token / ERC-20 (9) ───────────────────────────────────────────
            ("token", "balance")      => new TokenBalanceInfo(),
            ("token", "transfer")     => new TokenTransferInfo(),
            ("token", "approve")      => new TokenApproveInfo(),
            ("token", "transferFrom") => new TokenTransferFromInfo(),
            ("token", "allowance")    => new TokenAllowanceInfo(),
            ("token", "totalSupply")  => new TokenTotalSupplyInfo(),
            ("token", "decimals")     => new TokenDecimalsInfo(),
            ("token", "name")         => new TokenNameInfo(),
            ("token", "symbol")       => new TokenSymbolInfo(),
            ("token", "mint")         => new TokenMintInfo(),
            ("token", "burn")         => new TokenBurnInfo(),

            // ── contract (7) ─────────────────────────────────────────────────
            ("contract", "read")            => new ContractReadInfo(),
            ("contract", "write")           => new ContractWriteInfo(),
            ("contract", "simulate")        => new ContractSimulateInfo(),
            ("contract", "deploy")          => new ContractDeployInfo(),
            ("contract", "multicall")       => new ContractMulticallInfo(),
            ("contract", "logs")            => new ContractLogsInfo(),
            ("contract", "detectStandards") => new ContractDetectStandardsInfo(),

            // ── ens (5) ──────────────────────────────────────────────────────
            ("ens", "resolve")  => new EnsResolveInfo(),
            ("ens", "reverse")  => new EnsReverseInfo(),
            ("ens", "avatar")   => new EnsAvatarInfo(),
            ("ens", "text")     => new EnsTextInfo(),
            ("ens", "resolver") => new EnsResolverInfo(),

            // ── gas (5) ──────────────────────────────────────────────────────
            ("gas", "estimate")    => new GasEstimateInfo(),
            ("gas", "price")       => new GasPriceInfo(),
            ("gas", "feeHistory")  => new GasFeeHistoryInfo(),
            ("gas", "priorityFee") => new GasPriorityFeeInfo(),
            ("gas", "optimize")    => new GasOptimizeInfo(),

            // ── nft / ERC-721 (9) ────────────────────────────────────────────
            ("nft", "balance")           => new NftBalanceInfo(),
            ("nft", "ownerOf")           => new NftOwnerOfInfo(),
            ("nft", "transferFrom")      => new NftTransferFromInfo(),
            ("nft", "safeTransferFrom")  => new NftSafeTransferFromInfo(),
            ("nft", "approve")           => new NftApproveInfo(),
            ("nft", "setApprovalForAll") => new NftSetApprovalForAllInfo(),
            ("nft", "getApproved")       => new NftGetApprovedInfo(),
            ("nft", "isApprovedForAll")  => new NftIsApprovedForAllInfo(),
            ("nft", "tokenUri")          => new NftTokenUriInfo(),
            ("nft", "mint")              => new NftMintInfo(),
            ("nft", "listOwnedTokens")   => new NftListOwnedTokensInfo(),
            ("nft", "getMetadata")       => new NftGetMetadataInfo(),

            // ── wallet (3) — addendum v3 ─────────────────────────────────────
            ("wallet", "signMessage")     => new WalletSignMessageInfo(),
            ("wallet", "signTypedData")   => new WalletSignTypedDataInfo(),
            ("wallet", "verifySignature") => new WalletVerifySignatureInfo(),

            // ── utility (8) ──────────────────────────────────────────────────
            ("utility", "validateAddress")     => new UtilityValidateAddressInfo(),
            ("utility", "convertUnits")        => new UtilityConvertUnitsInfo(),
            ("utility", "chainId")             => new UtilityChainIdInfo(),
            ("utility", "encodeFunctionData")  => new UtilityEncodeFunctionDataInfo(),
            ("utility", "decodeFunctionData")  => new UtilityDecodeFunctionDataInfo(),
            ("utility", "encodeEventTopics")   => new UtilityEncodeEventTopicsInfo(),
            ("utility", "decodeEventLog")      => new UtilityDecodeEventLogInfo(),
            ("utility", "contractAddress")     => new UtilityContractAddressInfo(),

            _ => null,
        };

        info?.LoadFrom(reader);
        return info;
    }
}
