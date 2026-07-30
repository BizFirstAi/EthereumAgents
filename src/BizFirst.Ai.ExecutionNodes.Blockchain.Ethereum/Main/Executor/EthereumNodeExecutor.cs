using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;
using BizFirst.Ai.ProcessEngine.Domain.Credentials;

/// <summary>
/// Ethereum ExecutionNode — routes all resource/operation combinations to feature partials.
/// See the Verified Operation Catalog v2 (010_NodeDesign-Engineer/ExecutionNodes/Ethereum) for the
/// full, n8n-checked 55-operation/9-resource design this implements. Triggers (block/transaction/
/// contract-event/interval listeners) are intentionally NOT routed through this action-node switch —
/// see the design docs' open architectural note on trigger-executor architecture.
///
/// Design:
///   EthereumNodeExecutor.cs             — routing switch + constructor        (this file)
///   EthereumNodeExecutor.Config.cs       — settings accessor + port initialisation
///   EthereumNodeExecutor.Credentials.cs  — wallet private-key vault resolution
///   Main/Executor/Features/{Resource}/{Operation}/ — one feature partial per operation
///
/// Registration: BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum.EthereumDependency (Support/).
/// </summary>
public sealed partial class EthereumNodeExecutor : ResourceBasedNodeExecutor, IActionNodeExecution
{
    public const string NodeTypeName = "ethereum";
    public override string ProcessElementTypeCode => NodeTypeName;

    private readonly EthereumAccountService _accountService;
    private readonly EthereumBlockService _blockService;
    private readonly EthereumTransactionService _transactionService;
    private readonly EthereumTokenService _tokenService;
    private readonly EthereumContractService _contractService;
    private readonly EthereumEnsService _ensService;
    private readonly EthereumGasService _gasService;
    private readonly EthereumUtilityService _utilityService;
    private readonly EthereumNftService _nftService;
    private readonly EthereumWalletService _walletService;

    public EthereumNodeExecutor(
        ILogger<EthereumNodeExecutor> logger,
        EthereumAccountService accountService,
        EthereumBlockService blockService,
        EthereumTransactionService transactionService,
        EthereumTokenService tokenService,
        EthereumContractService contractService,
        EthereumEnsService ensService,
        EthereumGasService gasService,
        EthereumUtilityService utilityService,
        EthereumNftService nftService,
        EthereumWalletService walletService,
        INodeCredentialsFactory? credentialsFactory = null,
        INodeConfigurationParser? configParser = null,
        NodeFormResolver? formResolver = null)
        : base(logger, credentialsFactory, configParser, formResolver)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
        _ensService = ensService ?? throw new ArgumentNullException(nameof(ensService));
        _gasService = gasService ?? throw new ArgumentNullException(nameof(gasService));
        _utilityService = utilityService ?? throw new ArgumentNullException(nameof(utilityService));
        _nftService = nftService ?? throw new ArgumentNullException(nameof(nftService));
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
    }

    protected override async Task<NodeExecutionResult> _ExecuteInternal_Route_Async(
        NodeExecutionContext nodeExecutionContext,
        CancellationToken cancellationToken = default)
    {
        _executionResultManager = NodeResultOperateManager.CreateInstance(nodeExecutionContext);

        var result = (mySettings!.Resource, mySettings!.Operation) switch
        {
            // ── account (4) ──────────────────────────────────────────────────
            ("account", "balance") => await _Ethereum_Account_Balance_Async(nodeExecutionContext, cancellationToken),
            ("account", "nonce")   => await _Ethereum_Account_Nonce_Async(nodeExecutionContext, cancellationToken),
            ("account", "history") => await _Ethereum_Account_History_Async(nodeExecutionContext, cancellationToken),
            ("account", "code")    => await _Ethereum_Account_Code_Async(nodeExecutionContext, cancellationToken),

            // ── block (3) ────────────────────────────────────────────────────
            ("block", "get")    => await _Ethereum_Block_Get_Async(nodeExecutionContext, cancellationToken),
            ("block", "number") => await _Ethereum_Block_Number_Async(nodeExecutionContext, cancellationToken),
            ("block", "list")   => await _Ethereum_Block_List_Async(nodeExecutionContext, cancellationToken),

            // ── transaction (6) ──────────────────────────────────────────────
            ("transaction", "get")     => await _Ethereum_Transaction_Get_Async(nodeExecutionContext, cancellationToken),
            ("transaction", "send")    => await _Ethereum_Transaction_Send_Async(nodeExecutionContext, cancellationToken),
            ("transaction", "receipt") => await _Ethereum_Transaction_Receipt_Async(nodeExecutionContext, cancellationToken),
            ("transaction", "pending") => await _Ethereum_Transaction_Pending_Async(nodeExecutionContext, cancellationToken),
            ("transaction", "decode")  => await _Ethereum_Transaction_Decode_Async(nodeExecutionContext, cancellationToken),
            ("transaction", "wait")    => await _Ethereum_Transaction_Wait_Async(nodeExecutionContext, cancellationToken),

            // ── token / ERC-20 (9) ───────────────────────────────────────────
            ("token", "balance")      => await _Ethereum_Token_Balance_Async(nodeExecutionContext, cancellationToken),
            ("token", "transfer")     => await _Ethereum_Token_Transfer_Async(nodeExecutionContext, cancellationToken),
            ("token", "approve")      => await _Ethereum_Token_Approve_Async(nodeExecutionContext, cancellationToken),
            ("token", "transferFrom") => await _Ethereum_Token_TransferFrom_Async(nodeExecutionContext, cancellationToken),
            ("token", "allowance")    => await _Ethereum_Token_Allowance_Async(nodeExecutionContext, cancellationToken),
            ("token", "totalSupply")  => await _Ethereum_Token_TotalSupply_Async(nodeExecutionContext, cancellationToken),
            ("token", "decimals")     => await _Ethereum_Token_Decimals_Async(nodeExecutionContext, cancellationToken),
            ("token", "name")         => await _Ethereum_Token_Name_Async(nodeExecutionContext, cancellationToken),
            ("token", "symbol")       => await _Ethereum_Token_Symbol_Async(nodeExecutionContext, cancellationToken),
            ("token", "mint")         => await _Ethereum_Token_Mint_Async(nodeExecutionContext, cancellationToken),
            ("token", "burn")         => await _Ethereum_Token_Burn_Async(nodeExecutionContext, cancellationToken),

            // ── contract (7) ─────────────────────────────────────────────────
            ("contract", "read")            => await _Ethereum_Contract_Read_Async(nodeExecutionContext, cancellationToken),
            ("contract", "write")           => await _Ethereum_Contract_Write_Async(nodeExecutionContext, cancellationToken),
            ("contract", "simulate")        => await _Ethereum_Contract_Simulate_Async(nodeExecutionContext, cancellationToken),
            ("contract", "deploy")          => await _Ethereum_Contract_Deploy_Async(nodeExecutionContext, cancellationToken),
            ("contract", "multicall")       => await _Ethereum_Contract_Multicall_Async(nodeExecutionContext, cancellationToken),
            // ("contract", "logs")            => await _Ethereum_Contract_Logs_Async(nodeExecutionContext, cancellationToken),
            ("contract", "detectStandards") => await _Ethereum_Contract_DetectStandards_Async(nodeExecutionContext, cancellationToken),

            // ── ens (5) ──────────────────────────────────────────────────────
            ("ens", "resolve")  => await _Ethereum_Ens_Resolve_Async(nodeExecutionContext, cancellationToken),
            ("ens", "reverse")  => await _Ethereum_Ens_Reverse_Async(nodeExecutionContext, cancellationToken),
            ("ens", "avatar")   => await _Ethereum_Ens_Avatar_Async(nodeExecutionContext, cancellationToken),
            ("ens", "text")     => await _Ethereum_Ens_Text_Async(nodeExecutionContext, cancellationToken),
            ("ens", "resolver") => await _Ethereum_Ens_Resolver_Async(nodeExecutionContext, cancellationToken),

            // ── gas (5) ──────────────────────────────────────────────────────
            ("gas", "estimate")    => await _Ethereum_Gas_Estimate_Async(nodeExecutionContext, cancellationToken),
            ("gas", "price")       => await _Ethereum_Gas_Price_Async(nodeExecutionContext, cancellationToken),
            ("gas", "feeHistory")  => await _Ethereum_Gas_FeeHistory_Async(nodeExecutionContext, cancellationToken),
            ("gas", "priorityFee") => await _Ethereum_Gas_PriorityFee_Async(nodeExecutionContext, cancellationToken),
            ("gas", "optimize")    => await _Ethereum_Gas_Optimize_Async(nodeExecutionContext, cancellationToken),

            // ── nft / ERC-721 (9) ────────────────────────────────────────────
            ("nft", "balance")           => await _Ethereum_Nft_Balance_Async(nodeExecutionContext, cancellationToken),
            ("nft", "ownerOf")           => await _Ethereum_Nft_OwnerOf_Async(nodeExecutionContext, cancellationToken),
            ("nft", "transferFrom")      => await _Ethereum_Nft_TransferFrom_Async(nodeExecutionContext, cancellationToken),
            ("nft", "safeTransferFrom")  => await _Ethereum_Nft_SafeTransferFrom_Async(nodeExecutionContext, cancellationToken),
            ("nft", "approve")           => await _Ethereum_Nft_Approve_Async(nodeExecutionContext, cancellationToken),
            ("nft", "setApprovalForAll") => await _Ethereum_Nft_SetApprovalForAll_Async(nodeExecutionContext, cancellationToken),
            ("nft", "getApproved")       => await _Ethereum_Nft_GetApproved_Async(nodeExecutionContext, cancellationToken),
            ("nft", "isApprovedForAll")  => await _Ethereum_Nft_IsApprovedForAll_Async(nodeExecutionContext, cancellationToken),
            ("nft", "tokenUri")          => await _Ethereum_Nft_TokenUri_Async(nodeExecutionContext, cancellationToken),
            ("nft", "mint")              => await _Ethereum_Nft_Mint_Async(nodeExecutionContext, cancellationToken),
            ("nft", "listOwnedTokens")   => await _Ethereum_Nft_ListOwnedTokens_Async(nodeExecutionContext, cancellationToken),
            ("nft", "getMetadata")       => await _Ethereum_Nft_GetMetadata_Async(nodeExecutionContext, cancellationToken),

            // ── wallet (3) — addendum v3 ─────────────────────────────────────
            ("wallet", "signMessage")     => await _Ethereum_Wallet_SignMessage_Async(nodeExecutionContext, cancellationToken),
            ("wallet", "signTypedData")   => await _Ethereum_Wallet_SignTypedData_Async(nodeExecutionContext, cancellationToken),
            ("wallet", "verifySignature") => await _Ethereum_Wallet_VerifySignature_Async(nodeExecutionContext, cancellationToken),

            // ── utility (8) ──────────────────────────────────────────────────
            ("utility", "validateAddress")    => await _Ethereum_Utility_ValidateAddress_Async(nodeExecutionContext, cancellationToken),
            ("utility", "convertUnits")       => await _Ethereum_Utility_ConvertUnits_Async(nodeExecutionContext, cancellationToken),
            ("utility", "chainId")            => await _Ethereum_Utility_ChainId_Async(nodeExecutionContext, cancellationToken),
            ("utility", "encodeFunctionData") => await _Ethereum_Utility_EncodeFunctionData_Async(nodeExecutionContext, cancellationToken),
            ("utility", "decodeFunctionData") => await _Ethereum_Utility_DecodeFunctionData_Async(nodeExecutionContext, cancellationToken),
            ("utility", "encodeEventTopics")  => await _Ethereum_Utility_EncodeEventTopics_Async(nodeExecutionContext, cancellationToken),
            ("utility", "decodeEventLog")     => await _Ethereum_Utility_DecodeEventLog_Async(nodeExecutionContext, cancellationToken),
            ("utility", "contractAddress")    => await _Ethereum_Utility_ContractAddress_Async(nodeExecutionContext, cancellationToken),

            _ => await base._ExecuteInternal_Route_Async(nodeExecutionContext, cancellationToken)
        };

        LogActivity_Status($"{NodeTypeName} operation executed", result.IsSuccess);
        return result;
    }
}
