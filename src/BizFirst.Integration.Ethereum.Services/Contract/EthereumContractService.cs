namespace BizFirst.Integration.Ethereum.Services;

using System.Text.Json;
using Nethereum.ABI.ABIDeserialisation;
using Nethereum.ABI.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;

/// <summary>
/// Contract resource: generic ABI-driven calls (read/write/simulate), deployment, batched reads
/// (multicall), and historical event-log queries. Unlike Token/NFT (fixed ABIs baked into
/// <see cref="StandardAbis"/>), every operation here takes the contract's ABI as a caller-supplied
/// JSON string, since the whole point of this resource is calling arbitrary contracts.
///
/// JSON positional-argument convention (functionArgs/constructorArgs): a JSON array where each
/// element maps to one positional ABI input — JSON strings pass through as-is (address/string/bytes-hex
/// ABI types all take a string), JSON numbers convert to <see cref="BigInteger"/> (assumes integer-typed
/// ABI parameters: uintN/intN — a JSON number with a decimal point or exponent will throw), JSON
/// true/false convert to bool, and nested JSON arrays convert recursively to object[] (for ABI array
/// types like uint256[]/address[]). Verified against Nethereum 6.0.0's ABI encoder: a heterogeneous
/// object[] built this way encodes byte-identically to a strongly-typed array for the same values.
/// </summary>
public sealed class EthereumContractService
{
    /// <summary>Fallback gas limit for contract/deploy when the caller supplies neither 'gasLimit' nor a large enough estimate is otherwise available.</summary>
    private const long DefaultDeployGasLimit = 3_000_000;

    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumContractService> _logger;

    public EthereumContractService(EthereumConnectionProvider connectionProvider, ILogger<EthereumContractService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>SC01 — contract/read. Calls a view/pure function via eth_call and decodes every output positionally.</summary>
    public async Task<EthereumContractReadResult> ReadAsync(
        string? network, string contractAddress, string abi, string functionName, string? functionArgsJson, string? block,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);
            var functionArgs = ParseJsonArgs(functionArgsJson);
            var blockParameter = EthereumAccountService.ParseBlockParameter(block);

            var outputs = await function.CallDecodingToDefaultAsync(blockParameter, functionArgs).ConfigureAwait(false);
            var decoded = outputs.Select(o => o.Result?.ToString() ?? string.Empty).ToArray();
            object? result = decoded.Length switch { 0 => null, 1 => decoded[0], _ => decoded };

            return EthereumContractReadResult.Ok(result, decoded);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumContractReadResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (JsonException ex)
        {
            return EthereumContractReadResult.Fail("VAL_INVALID_FUNCTION_ARGS", $"Config key 'functionArgs' is not valid JSON: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return EthereumContractReadResult.Fail("VAL_INVALID_FUNCTION_ARGS", $"Config key 'functionArgs' contains a value that could not be converted: {ex.Message}");
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "contract/read RPC error for {Contract}.{Function} on {Network}", contractAddress, functionName, network);
            return EthereumContractReadResult.Fail(ClassifyRpcErrorCode(ex), ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/read unexpected error for {Contract}.{Function} on {Network}", contractAddress, functionName, network);
            return EthereumContractReadResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>SC02 — contract/write. Sends a state-changing transaction to a contract function. Signing key resolved by the caller via vault (credentialID) and passed in as <paramref name="privateKeyHex"/>.</summary>
    public async Task<EthereumContractWriteResult> WriteAsync(
        string? network, string privateKeyHex, string contractAddress, string abi, string functionName,
        string? functionArgsJson, string? valueWei, string? gasLimit, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var from = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(from, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);
            var functionArgs = ParseJsonArgs(functionArgsJson);

            var gas = ParseHexBigIntegerOrNull(gasLimit);
            var value = ParseHexBigIntegerOrNull(valueWei);

            string transactionHash;
            if (gas is null && value is null)
            {
                transactionHash = await function.SendTransactionAsync(from, functionArgs).ConfigureAwait(false);
            }
            else
            {
                var effectiveValue = value ?? new HexBigInteger(BigInteger.Zero);
                if (gas is null)
                {
                    // VERIFY: Function.EstimateGasAsync(from, gas, value, functionInput) itself takes a
                    // "gas" argument (a cap/hint for the eth_estimateGas call, not the estimate's result).
                    // Passing a generous cap (30M, a typical mainnet block gas limit) here is a best-effort
                    // default so the estimation call isn't artificially constrained for gas-heavy functions;
                    // this exact overload/parameter semantics was reflected against the Nethereum 6.0.0 API
                    // surface but not exercised against a live node, since only 'value' (no 'gasLimit') was
                    // supplied by the caller in this branch.
                    gas = await function.EstimateGasAsync(from, new HexBigInteger(new BigInteger(30_000_000)), effectiveValue, functionArgs).ConfigureAwait(false);
                }

                transactionHash = await function.SendTransactionAsync(from, gas, effectiveValue, functionArgs).ConfigureAwait(false);
            }

            return EthereumContractWriteResult.Ok(transactionHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumContractWriteResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (JsonException ex)
        {
            return EthereumContractWriteResult.Fail("VAL_INVALID_FUNCTION_ARGS", $"Config key 'functionArgs' is not valid JSON: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return EthereumContractWriteResult.Fail("VAL_INVALID_FUNCTION_ARGS", $"Config key 'functionArgs'/'value'/'gasLimit' contains a value that could not be converted: {ex.Message}");
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "contract/write RPC error for {Contract}.{Function} on {Network}", contractAddress, functionName, network);
            return EthereumContractWriteResult.Fail(ClassifyRpcErrorCode(ex), ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/write unexpected error for {Contract}.{Function} on {Network}", contractAddress, functionName, network);
            return EthereumContractWriteResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// SC03 — contract/simulate. Previews what a function would return/revert with via eth_call, WITHOUT
    /// broadcasting a transaction — optionally as if sent from a specific 'from' address (e.g. to preview
    /// an onlyOwner function's outcome). Verified locally: <see cref="Nethereum.Contracts.Function.CreateCallInput(object[])"/>
    /// already populates <see cref="CallInput.To"/> and <see cref="CallInput.Data"/> from the bound contract/function,
    /// so only 'From' and 'Value' need to be layered on here.
    /// </summary>
    public async Task<EthereumContractSimulateResult> SimulateAsync(
        string? network, string contractAddress, string abi, string functionName, string? functionArgsJson,
        string? valueWei, string? from, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var contract = web3.Eth.GetContract(abi, contractAddress);
            var function = contract.GetFunction(functionName);
            var functionArgs = ParseJsonArgs(functionArgsJson);

            var callInput = function.CreateCallInput(functionArgs);
            if (!string.IsNullOrWhiteSpace(from))
                callInput.From = from;

            var value = ParseHexBigIntegerOrNull(valueWei);
            if (value is not null)
                callInput.Value = value;

            var outputs = await function.CallDecodingToDefaultAsync(callInput).ConfigureAwait(false);
            var decoded = outputs.Select(o => o.Result?.ToString() ?? string.Empty).ToArray();
            object? result = decoded.Length switch { 0 => null, 1 => decoded[0], _ => decoded };

            return EthereumContractSimulateResult.Ok(result, decoded);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumContractSimulateResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (JsonException ex)
        {
            return EthereumContractSimulateResult.Fail("VAL_INVALID_FUNCTION_ARGS", $"Config key 'functionArgs' is not valid JSON: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return EthereumContractSimulateResult.Fail("VAL_INVALID_FUNCTION_ARGS", $"Config key 'functionArgs'/'value' contains a value that could not be converted: {ex.Message}");
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "contract/simulate RPC error for {Contract}.{Function} on {Network}", contractAddress, functionName, network);
            return EthereumContractSimulateResult.Fail(ClassifyRpcErrorCode(ex), ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/simulate unexpected error for {Contract}.{Function} on {Network}", contractAddress, functionName, network);
            return EthereumContractSimulateResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>SC04 — contract/deploy. Broadcasts a contract-creation transaction. Signing key resolved by the caller via vault (credentialID) and passed in as <paramref name="privateKeyHex"/>. Returns the deployment TRANSACTION hash — not the resulting contract address (see <see cref="EthereumContractDeployResult"/>).</summary>
    public async Task<EthereumContractDeployResult> DeployAsync(
        string? network, string privateKeyHex, string bytecode, string abi, string? constructorArgsJson,
        string? valueWei, string? gasLimit, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetSignedClientAsync(network, privateKeyHex, cancellationToken).ConfigureAwait(false);
            var from = web3.TransactionManager.Account.Address;
            using var addressLock = await _connectionProvider.AcquireAddressLockAsync(from, cancellationToken).ConfigureAwait(false);
            var constructorArgs = ParseJsonArgs(constructorArgsJson);

            var gas = ParseHexBigIntegerOrNull(gasLimit);
            var value = ParseHexBigIntegerOrNull(valueWei);

            string transactionHash;
            if (gas is null && value is null)
            {
                transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, bytecode, from, constructorArgs).ConfigureAwait(false);
            }
            else
            {
                var effectiveGas = gas ?? new HexBigInteger(new BigInteger(DefaultDeployGasLimit));
                transactionHash = value is null
                    ? await web3.Eth.DeployContract.SendRequestAsync(abi, bytecode, from, effectiveGas, constructorArgs).ConfigureAwait(false)
                    : await web3.Eth.DeployContract.SendRequestAsync(abi, bytecode, from, effectiveGas, value, constructorArgs).ConfigureAwait(false);
            }

            return EthereumContractDeployResult.Ok(transactionHash);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumContractDeployResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (JsonException ex)
        {
            return EthereumContractDeployResult.Fail("VAL_INVALID_CONSTRUCTOR_ARGS", $"Config key 'constructorArgs' is not valid JSON: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return EthereumContractDeployResult.Fail("VAL_INVALID_CONSTRUCTOR_ARGS", $"Config key 'constructorArgs'/'value'/'gasLimit' contains a value that could not be converted: {ex.Message}");
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "contract/deploy RPC error on {Network}", network);
            return EthereumContractDeployResult.Fail(ClassifyRpcErrorCode(ex), ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/deploy unexpected error on {Network}", network);
            return EthereumContractDeployResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// SC05 — contract/multicall. Batches independent read calls (each potentially against a different
    /// contract/ABI/function) into one node execution by delegating each entry to <see cref="ReadAsync"/>.
    /// Partial success by design: a malformed 'calls' JSON fails the whole batch (nothing could be
    /// executed), but once the batch is parsed, one call's failure (bad address, revert, RPC error, ...)
    /// only marks that entry — every other call still executes and reports its own outcome.
    /// </summary>
    public async Task<EthereumContractMulticallResult> MulticallAsync(
        string? network, string callsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var calls = ParseMulticallSpecs(callsJson);
            var results = new List<EthereumContractMulticallEntry>(calls.Count);

            foreach (var call in calls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var r = await ReadAsync(network, call.ContractAddress, call.Abi, call.FunctionName, call.FunctionArgs, block: null, cancellationToken)
                    .ConfigureAwait(false);

                results.Add(r.Success
                    ? EthereumContractMulticallEntry.Ok(call.ContractAddress, call.FunctionName, r.Result, r.DecodedResult)
                    : EthereumContractMulticallEntry.Fail(call.ContractAddress, call.FunctionName, r.ErrorCode, r.ErrorMessage));
            }

            return EthereumContractMulticallResult.Ok(results);
        }
        catch (OperationCanceledException) { throw; }
        catch (JsonException ex)
        {
            return EthereumContractMulticallResult.Fail("VAL_INVALID_CALLS", $"Config key 'calls' is not valid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/multicall unexpected error on {Network}", network);
            return EthereumContractMulticallResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// SC06 — contract/logs. A one-shot historical eth_getLogs query for a single event type. The event
    /// ABI is hashed down to the topic0 filter (verified locally against the well-known ERC-20 Transfer
    /// event signature hash). Optional 'topics' entries describe indexed-parameter filters AFTER topic0
    /// (which is always derived from 'eventAbi', never user-suppliable) — topics[0] here becomes
    /// eth_getLogs topic1, topics[1] becomes topic2, and so on; this ordering is a deliberate design
    /// choice (the event signature always identifies "which event", so it can't be overridden by 'topics')
    /// rather than a Nethereum API constraint.
    /// </summary>
    public async Task<EthereumContractLogsResult> GetLogsAsync(
        string? network, string? contractAddress, string eventAbiJson, string? fromBlock, string? toBlock, string? topicsJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var eventAbi = ParseSingleEventAbi(eventAbiJson);
            var topic0 = "0x" + eventAbi.Sha3Signature;

            var topics = new List<object> { topic0 };
            if (!string.IsNullOrWhiteSpace(topicsJson))
                topics.AddRange(ParseTopicsArray(topicsJson));

            var filterInput = new NewFilterInput
            {
                Address = string.IsNullOrWhiteSpace(contractAddress) ? null : new[] { contractAddress },
                // No lower bound supplied by the caller defaults to a genesis-to-latest search (the
                // natural reading of "fromBlock omitted"), not to EthereumAccountService.ParseBlockParameter's
                // own null-default of "latest" (which would make an omitted fromBlock degenerate to a
                // zero-width, essentially-empty range together with the toBlock default below).
                FromBlock = EthereumAccountService.ParseBlockParameter(string.IsNullOrWhiteSpace(fromBlock) ? "earliest" : fromBlock),
                ToBlock = EthereumAccountService.ParseBlockParameter(string.IsNullOrWhiteSpace(toBlock) ? "latest" : toBlock),
                Topics = topics.ToArray(),
            };

            var logs = await web3.Eth.Filters.GetLogs.SendRequestAsync(filterInput).ConfigureAwait(false);

            var mapped = logs.Select(l => new EthereumEventLogEntry(
                l.Address,
                (l.Topics ?? Array.Empty<object>()).Select(t => t?.ToString() ?? string.Empty).ToList(),
                l.Data,
                l.BlockNumber is null ? null : (long?)l.BlockNumber.Value,
                l.TransactionHash,
                l.LogIndex is null ? null : (int?)l.LogIndex.Value
            )).ToList();

            return EthereumContractLogsResult.Ok(mapped);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumContractLogsResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (JsonException ex)
        {
            return EthereumContractLogsResult.Fail("VAL_INVALID_EVENT_ABI", $"Config key 'eventAbi'/'topics' is not valid JSON: {ex.Message}");
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "contract/logs RPC error for {Contract} on {Network}", contractAddress, network);
            return EthereumContractLogsResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/logs unexpected error for {Contract} on {Network}", contractAddress, network);
            return EthereumContractLogsResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// SC07 — contract/detectStandards (addendum v3). Two independent probes: real ERC-165
    /// <c>supportsInterface</c> lookups against <see cref="StandardAbis.Erc165KnownInterfaceIds"/>,
    /// and an ERC-20 heuristic (decimals/symbol/totalSupply all succeed) since ERC-20 predates
    /// ERC-165 and has no supportsInterface at all.
    /// </summary>
    public async Task<EthereumContractDetectStandardsResult> DetectStandardsAsync(
        string? network, string contractAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var erc165Contract = web3.Eth.GetContract(StandardAbis.Erc165, contractAddress);

            var erc165Supported = false;
            var supported = new List<string>();
            foreach (var (name, interfaceId) in StandardAbis.Erc165KnownInterfaceIds)
            {
                try
                {
                    var isSupported = await erc165Contract.GetFunction("supportsInterface")
                        .CallAsync<bool>(interfaceId.HexToByteArray()).ConfigureAwait(false);

                    if (name == "ERC-165") erc165Supported = isSupported;
                    if (isSupported) supported.Add(name);
                }
                catch (RpcResponseException)
                {
                    // Reverts (no supportsInterface at all) are expected for non-ERC-165 contracts —
                    // treated as "not supported", not a hard failure of the whole detection.
                }
            }

            var erc20Heuristic = await ProbeErc20HeuristicAsync(web3, contractAddress).ConfigureAwait(false);

            return EthereumContractDetectStandardsResult.Ok(erc165Supported, supported, erc20Heuristic);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumContractDetectStandardsResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "contract/detectStandards unexpected error for {Contract} on {Network}", contractAddress, network);
            return EthereumContractDetectStandardsResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>ERC-20 has no ERC-165 interface ID — this is the closest thing to a real probe: three standard getters must all succeed without reverting.</summary>
    private static async Task<bool> ProbeErc20HeuristicAsync(Nethereum.Web3.IWeb3 web3, string contractAddress)
    {
        try
        {
            var contract = web3.Eth.GetContract(StandardAbis.Erc20, contractAddress);
            await contract.GetFunction("decimals").CallAsync<byte>().ConfigureAwait(false);
            await contract.GetFunction("symbol").CallAsync<string>().ConfigureAwait(false);
            await contract.GetFunction("totalSupply").CallAsync<BigInteger>().ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Converts a JSON array string (functionArgs/constructorArgs) into positional args for Nethereum's object[]-based function-input APIs. See the class-level doc for the exact coercion rules.</summary>
    private static object[] ParseJsonArgs(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<object>();

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a JSON array of positional arguments.");

        return doc.RootElement.EnumerateArray().Select(ConvertJsonElement).ToArray();
    }

    private static object ConvertJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number => BigInteger.Parse(element.GetRawText()),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToArray(),
        JsonValueKind.Null => null!,
        _ => element.GetRawText(),
    };

    private static HexBigInteger? ParseHexBigIntegerOrNull(string? decimalString) =>
        string.IsNullOrWhiteSpace(decimalString) ? null : new HexBigInteger(BigInteger.Parse(decimalString));

    /// <summary>ERC-20/reverts (e.g. "execution reverted: ...") surface as generic RPC errors from most providers — a substring match is a heuristic best-effort classification, not a guaranteed parse of every provider's revert message format.</summary>
    private static string ClassifyRpcErrorCode(RpcResponseException ex) =>
        ex.Message.Contains("revert", StringComparison.OrdinalIgnoreCase) ? "CONTRACT_CALL_REVERTED" : "RPC_ERROR";

    private static EventABI ParseSingleEventAbi(string eventAbiJson)
    {
        var trimmed = eventAbiJson.Trim();
        var wrapped = trimmed.StartsWith('[') ? trimmed : $"[{trimmed}]";
        var contractAbi = ABIDeserialiserFactory.DeserialiseContractABI(wrapped);
        var eventAbi = contractAbi.Events?.FirstOrDefault();
        if (eventAbi is null)
            throw new JsonException("Config key 'eventAbi' does not contain a valid event ABI definition.");
        return eventAbi;
    }

    /// <summary>Each 'topics' entry is a single topic hex string, a JSON array of alternative hex strings (an OR filter for that position, per the eth_getLogs spec), or null (meaning "any value" at that position).</summary>
    private static IEnumerable<object> ParseTopicsArray(string topicsJson)
    {
        using var doc = JsonDocument.Parse(topicsJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Config key 'topics' must be a JSON array.");

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            yield return element.ValueKind switch
            {
                JsonValueKind.Null => null!,
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Array => element.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray(),
                _ => throw new JsonException("Each 'topics' entry must be a hex string, an array of hex strings, or null."),
            };
        }
    }

    private sealed record MulticallCallSpec(string ContractAddress, string Abi, string FunctionName, string? FunctionArgs);

    private static IReadOnlyList<MulticallCallSpec> ParseMulticallSpecs(string callsJson)
    {
        using var doc = JsonDocument.Parse(callsJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Config key 'calls' must be a JSON array of call objects.");

        var specs = new List<MulticallCallSpec>();
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var contractAddress = item.TryGetProperty("contractAddress", out var ca) ? ca.GetString() : null;
            var abi = item.TryGetProperty("abi", out var ab) ? ab.GetString() : null;
            var functionName = item.TryGetProperty("functionName", out var fn) ? fn.GetString() : null;
            var functionArgs = item.TryGetProperty("functionArgs", out var fa) ? fa.GetRawText() : null;

            if (string.IsNullOrWhiteSpace(contractAddress) || string.IsNullOrWhiteSpace(abi) || string.IsNullOrWhiteSpace(functionName))
                throw new JsonException("Each entry in 'calls' requires 'contractAddress', 'abi', and 'functionName'.");

            specs.Add(new MulticallCallSpec(contractAddress!, abi!, functionName!, functionArgs));
        }

        return specs;
    }
}
