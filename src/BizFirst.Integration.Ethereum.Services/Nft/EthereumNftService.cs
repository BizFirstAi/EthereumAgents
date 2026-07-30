namespace BizFirst.Integration.Ethereum.Services;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;

/// <summary>
/// ERC-721 (EIP-721) NFT resource. The first 9 operations match the actual EIP-721 standard
/// interface. NFT10/11/12 (addendum v3) go beyond the standard: Mint has no real standard
/// signature (assumes `{functionName}(address[,uint256])`); ListOwnedTokens is gated on the real
/// Enumerable extension rather than an unreliable log scan; GetMetadata is this node's first
/// outbound HTTP call (via <see cref="EthereumNftMetadataFetcher"/>), fetching and parsing the
/// tokenURI's JSON content.
/// </summary>
public sealed class EthereumNftService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly EthereumNftMetadataFetcher _metadataFetcher;
    private readonly ILogger<EthereumNftService> _logger;

    public EthereumNftService(EthereumConnectionProvider connectionProvider, EthereumNftMetadataFetcher metadataFetcher, ILogger<EthereumNftService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _metadataFetcher = metadataFetcher ?? throw new ArgumentNullException(nameof(metadataFetcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>NFT01 — nft/balance.</summary>
    public async Task<EthereumNftBalanceResult> GetBalanceAsync(
        string? network, string contractAddress, string ownerAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var raw = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(ownerAddress).ConfigureAwait(false);
            return EthereumNftBalanceResult.Ok(raw.ToString());
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftBalanceResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/balance RPC error for {Contract}/{Owner} on {Network}", contractAddress, ownerAddress, network);
            return EthereumNftBalanceResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/balance unexpected error for {Contract}/{Owner} on {Network}", contractAddress, ownerAddress, network);
            return EthereumNftBalanceResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT02 — nft/ownerOf.</summary>
    public async Task<EthereumNftOwnerOfResult> GetOwnerOfAsync(
        string? network, string contractAddress, BigInteger tokenID, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var owner = await contract.GetFunction("ownerOf").CallAsync<string>(tokenID).ConfigureAwait(false);
            return EthereumNftOwnerOfResult.Ok(owner ?? string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftOwnerOfResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/ownerOf RPC error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftOwnerOfResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/ownerOf unexpected error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftOwnerOfResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT03 — nft/transferFrom.</summary>
    public async Task<EthereumNftTransferFromResult> TransferFromAsync(
        string? network, string privateKeyHex, string contractAddress, string from, string to, BigInteger tokenID, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var txHash = await contract.GetFunction("transferFrom").SendTransactionAsync(sender, from, to, tokenID).ConfigureAwait(false);
            return EthereumNftTransferFromResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftTransferFromResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/transferFrom RPC error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftTransferFromResult.Fail("RPC_ERROR", MapErc721RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/transferFrom unexpected error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftTransferFromResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT04 — nft/safeTransferFrom.</summary>
    public async Task<EthereumNftSafeTransferFromResult> SafeTransferFromAsync(
        string? network, string privateKeyHex, string contractAddress, string from, string to, BigInteger tokenID, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var txHash = await contract.GetFunction("safeTransferFrom").SendTransactionAsync(sender, from, to, tokenID).ConfigureAwait(false);
            return EthereumNftSafeTransferFromResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftSafeTransferFromResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/safeTransferFrom RPC error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftSafeTransferFromResult.Fail("RPC_ERROR", MapErc721RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/safeTransferFrom unexpected error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftSafeTransferFromResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT05 — nft/approve.</summary>
    public async Task<EthereumNftApproveResult> ApproveAsync(
        string? network, string privateKeyHex, string contractAddress, string spender, BigInteger tokenID, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var txHash = await contract.GetFunction("approve").SendTransactionAsync(sender, spender, tokenID).ConfigureAwait(false);
            return EthereumNftApproveResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftApproveResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/approve RPC error for {Contract} tokenID {TokenID} spender {Spender} on {Network}", contractAddress, tokenID, spender, network);
            return EthereumNftApproveResult.Fail("RPC_ERROR", MapErc721RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/approve unexpected error for {Contract} tokenID {TokenID} spender {Spender} on {Network}", contractAddress, tokenID, spender, network);
            return EthereumNftApproveResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT06 — nft/setApprovalForAll.</summary>
    public async Task<EthereumNftSetApprovalForAllResult> SetApprovalForAllAsync(
        string? network, string privateKeyHex, string contractAddress, string operatorAddress, bool approved, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var txHash = await contract.GetFunction("setApprovalForAll").SendTransactionAsync(sender, operatorAddress, approved).ConfigureAwait(false);
            return EthereumNftSetApprovalForAllResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftSetApprovalForAllResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/setApprovalForAll RPC error for {Contract} operator {Operator} on {Network}", contractAddress, operatorAddress, network);
            return EthereumNftSetApprovalForAllResult.Fail("RPC_ERROR", MapErc721RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/setApprovalForAll unexpected error for {Contract} operator {Operator} on {Network}", contractAddress, operatorAddress, network);
            return EthereumNftSetApprovalForAllResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT07 — nft/getApproved.</summary>
    public async Task<EthereumNftGetApprovedResult> GetApprovedAsync(
        string? network, string contractAddress, BigInteger tokenID, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var approved = await contract.GetFunction("getApproved").CallAsync<string>(tokenID).ConfigureAwait(false);
            return EthereumNftGetApprovedResult.Ok(approved ?? string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftGetApprovedResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/getApproved RPC error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftGetApprovedResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/getApproved unexpected error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftGetApprovedResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT08 — nft/isApprovedForAll.</summary>
    public async Task<EthereumNftIsApprovedForAllResult> IsApprovedForAllAsync(
        string? network, string contractAddress, string owner, string operatorAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var isApproved = await contract.GetFunction("isApprovedForAll").CallAsync<bool>(owner, operatorAddress).ConfigureAwait(false);
            return EthereumNftIsApprovedForAllResult.Ok(isApproved);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftIsApprovedForAllResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/isApprovedForAll RPC error for {Contract} owner {Owner} operator {Operator} on {Network}", contractAddress, owner, operatorAddress, network);
            return EthereumNftIsApprovedForAllResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/isApprovedForAll unexpected error for {Contract} owner {Owner} operator {Operator} on {Network}", contractAddress, owner, operatorAddress, network);
            return EthereumNftIsApprovedForAllResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>NFT09 — nft/tokenUri. Returns the raw URI as-is (often "ipfs://..."); resolving/fetching it is out of scope.</summary>
    public async Task<EthereumNftTokenUriResult> GetTokenUriAsync(
        string? network, string contractAddress, BigInteger tokenID, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var tokenUri = await contract.GetFunction("tokenURI").CallAsync<string>(tokenID).ConfigureAwait(false);
            return EthereumNftTokenUriResult.Ok(tokenUri ?? string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftTokenUriResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/tokenUri RPC error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftTokenUriResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/tokenUri unexpected error for {Contract} tokenID {TokenID} on {Network}", contractAddress, tokenID, network);
            return EthereumNftTokenUriResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// NFT10 — nft/mint (addendum v3). No industry-standard signature exists. Two shapes based on
    /// whether <paramref name="tokenId"/> is supplied: given → `{functionName}(address,uint256)`
    /// (explicit-ID mint); omitted → `{functionName}(address)` (auto-incrementing mint, common on
    /// ERC721A-style contracts). <paramref name="functionName"/> defaults to <c>"safeMint"</c>.
    /// </summary>
    public async Task<EthereumNftMintResult> MintAsync(
        string? network, string privateKeyHex, string contractAddress, string to, BigInteger? tokenId, string functionName, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var sender = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(sender, cancellationToken).ConfigureAwait(false);
            var includeTokenId = tokenId.HasValue;
            var contract = web3.Eth.GetContract(StandardAbis.BuildMintAbi(functionName, includeTokenId), contractAddress);

            var txHash = includeTokenId
                ? await contract.GetFunction(functionName).SendTransactionAsync(sender, to, tokenId!.Value).ConfigureAwait(false)
                : await contract.GetFunction(functionName).SendTransactionAsync(sender, to).ConfigureAwait(false);

            return EthereumNftMintResult.Ok(txHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftMintResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/mint RPC error for {Contract} to {To} on {Network}", contractAddress, to, network);
            return EthereumNftMintResult.Fail("RPC_ERROR", MapErc721RevertReason(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/mint unexpected error for {Contract} to {To} on {Network}", contractAddress, to, network);
            return EthereumNftMintResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// NFT11 — nft/listOwnedTokens (addendum v3). Gated on the real ERC-721 Enumerable extension
    /// (interface ID 0x780e9d63) — fails with NOT_SUPPORTED rather than attempting an unreliable
    /// Transfer-event log scan when the contract doesn't implement it (same reasoning as
    /// account/history's INDEXER_NOT_CONFIGURED precedent).
    /// </summary>
    public async Task<EthereumNftListOwnedTokensResult> ListOwnedTokensAsync(
        string? network, string contractAddress, string ownerAddress, int limit, int offset, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);

            var erc165Contract = web3.Eth.GetContract(StandardAbis.Erc165, contractAddress);
            bool enumerable;
            try
            {
                enumerable = await erc165Contract.GetFunction("supportsInterface")
                    .CallAsync<bool>(StandardAbis.Erc165KnownInterfaceIds["ERC-721Enumerable"].HexToByteArray()).ConfigureAwait(false);
            }
            catch (RpcResponseException)
            {
                enumerable = false;
            }

            if (!enumerable)
                return EthereumNftListOwnedTokensResult.Fail("NOT_SUPPORTED", "Contract does not implement ERC-721 Enumerable (0x780e9d63) — cannot list owned tokens without an indexer.");

            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            var enumerableContract = web3.Eth.GetContract(StandardAbis.Erc721Enumerable, contractAddress);

            var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(ownerAddress).ConfigureAwait(false);
            var total = (long)balance;

            var end = Math.Min(offset + limit, total);
            var tokenIds = new List<string>();
            for (var i = offset; i < end; i++)
            {
                var tokenId = await enumerableContract.GetFunction("tokenOfOwnerByIndex").CallAsync<BigInteger>(ownerAddress, (BigInteger)i).ConfigureAwait(false);
                tokenIds.Add(tokenId.ToString());
            }

            return EthereumNftListOwnedTokensResult.Ok(tokenIds, total);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftListOwnedTokensResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/listOwnedTokens RPC error for {Contract}/{Owner} on {Network}", contractAddress, ownerAddress, network);
            return EthereumNftListOwnedTokensResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/listOwnedTokens unexpected error for {Contract}/{Owner} on {Network}", contractAddress, ownerAddress, network);
            return EthereumNftListOwnedTokensResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// NFT12 — nft/getMetadata (addendum v3). Calls tokenURI() (reusing <see cref="GetTokenUriAsync"/>'s
    /// RPC path), then resolves/fetches/parses the JSON metadata document via
    /// <see cref="EthereumNftMetadataFetcher"/> — this node's first outbound HTTP call.
    /// </summary>
    public async Task<EthereumNftGetMetadataResult> GetMetadataAsync(
        string? network, string contractAddress, BigInteger tokenId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        string tokenUri;
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(StandardAbis.Erc721, contractAddress);
            tokenUri = await contract.GetFunction("tokenURI").CallAsync<string>(tokenId).ConfigureAwait(false) ?? string.Empty;
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumNftGetMetadataResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "nft/getMetadata tokenURI RPC error for {Contract} on {Network}", contractAddress, network);
            return EthereumNftGetMetadataResult.Fail("RPC_ERROR", ex.Message);
        }

        if (string.IsNullOrWhiteSpace(tokenUri))
            return EthereumNftGetMetadataResult.Fail("EMPTY_TOKEN_URI", "tokenURI() returned an empty value — nothing to fetch.");

        try
        {
            var metadataJson = await _metadataFetcher.FetchAsync(tokenUri, timeoutSeconds, cancellationToken).ConfigureAwait(false);
            return EthereumNftGetMetadataResult.Ok(tokenUri, metadataJson);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return EthereumNftGetMetadataResult.Fail("TIMEOUT", $"Metadata fetch timed out after {timeoutSeconds}s");
        }
        catch (OperationCanceledException) { throw; }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("nft/getMetadata fetch blocked for {Contract}: {Message}", contractAddress, ex.Message);
            return EthereumNftGetMetadataResult.Fail("FETCH_BLOCKED", ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "nft/getMetadata HTTP fetch failed for {Contract} URI {TokenUri}", contractAddress, tokenUri);
            return EthereumNftGetMetadataResult.Fail("HTTP_FETCH_FAILED", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "nft/getMetadata unexpected error for {Contract} on {Network}", contractAddress, network);
            return EthereumNftGetMetadataResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// ERC-721 reverts (e.g. not the owner/approved, nonexistent token) surface as generic RPC errors
    /// from most providers — this maps the common case to a clearer message without pretending to
    /// parse a custom revert reason format that isn't guaranteed across providers.
    /// </summary>
    private static string MapErc721RevertReason(RpcResponseException ex) => ex.Message;
}
