namespace BizFirst.Integration.Ethereum.Services;

using System.Text.Json;
using Nethereum.ABI.ABIDeserialisation;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.Util;

/// <summary>
/// Utility resource: address validation, unit conversion, chain ID, ABI encode/decode for both
/// function calls and event logs, and CREATE/CREATE2 address prediction. Every operation is a
/// pure computation except UTIL03 (chainId), which is the only one that makes an RPC call.
///
/// <see cref="DecodeEventLogAsync"/> (UTIL07) is load-bearing, not optional polish: it's what turns
/// a raw <see cref="EthereumEventLogEntry"/> (from SC06 contract/logs or a future contract-event
/// trigger) into the decoded event data this design promises elsewhere.
/// </summary>
public sealed class EthereumUtilityService
{
    private readonly EthereumConnectionProvider _connectionProvider;
    private readonly ILogger<EthereumUtilityService> _logger;

    public EthereumUtilityService(EthereumConnectionProvider connectionProvider, ILogger<EthereumUtilityService> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>UTIL01 — utility/validateAddress. Format + EIP-55 checksum validation.</summary>
    public Task<EthereumUtilityValidateAddressResult> ValidateAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            var util = AddressUtil.Current;
            var isValid = util.IsValidEthereumAddressHexFormat(address);
            if (!isValid)
                return Task.FromResult(EthereumUtilityValidateAddressResult.Ok(false, false, string.Empty));

            var isChecksumValid = util.IsChecksumAddress(address);
            var checksummed = util.ConvertToChecksumAddress(address);
            return Task.FromResult(EthereumUtilityValidateAddressResult.Ok(true, isChecksumValid, checksummed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/validateAddress unexpected error for {Address}", address);
            return Task.FromResult(EthereumUtilityValidateAddressResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>UTIL02 — utility/convertUnits. Chains through wei as the common base (matches n8n's separate Format/Parse Units, unified into one bidirectional operation).</summary>
    public Task<EthereumUtilityConvertUnitsResult> ConvertUnitsAsync(string value, string fromUnit, string toUnit, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromEnum = ParseUnit(fromUnit);
            var toEnum = ParseUnit(toUnit);
            if (fromEnum is null) return Task.FromResult(EthereumUtilityConvertUnitsResult.Fail("VAL_INVALID_FROM_UNIT", $"'fromUnit' must be 'wei', 'gwei', or 'ether' — got '{fromUnit}'."));
            if (toEnum is null) return Task.FromResult(EthereumUtilityConvertUnitsResult.Fail("VAL_INVALID_TO_UNIT", $"'toUnit' must be 'wei', 'gwei', or 'ether' — got '{toUnit}'."));

            if (!decimal.TryParse(value, out var decimalValue))
                return Task.FromResult(EthereumUtilityConvertUnitsResult.Fail("VAL_INVALID_VALUE", $"'value' is not a valid number: '{value}'."));

            var wei = UnitConversion.Convert.ToWei(decimalValue, fromEnum.Value);
            var converted = UnitConversion.Convert.FromWei(wei, toEnum.Value);

            return Task.FromResult(EthereumUtilityConvertUnitsResult.Ok(wei.ToString(), converted, NormalizeUnit(fromUnit), NormalizeUnit(toUnit)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/convertUnits unexpected error converting {Value} {From}->{To}", value, fromUnit, toUnit);
            return Task.FromResult(EthereumUtilityConvertUnitsResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>UTIL03 — utility/chainId (eth_chainId). The only Utility operation that makes an RPC call.</summary>
    public async Task<EthereumUtilityChainIdResult> GetChainIdAsync(string? network, CancellationToken cancellationToken = default)
    {
        try
        {
            var web3 = await _connectionProvider.GetReadOnlyClientAsync(network, cancellationToken).ConfigureAwait(false);
            var chainID = await web3.Eth.ChainId.SendRequestAsync().ConfigureAwait(false);
            return EthereumUtilityChainIdResult.Ok((long)chainID.Value);
        }
        catch (OperationCanceledException) { throw; }
        catch (EthereumNetworkNotConfiguredException ex)
        {
            return EthereumUtilityChainIdResult.Fail("NETWORK_NOT_CONFIGURED", ex.Message);
        }
        catch (RpcResponseException ex)
        {
            _logger.LogError(ex, "utility/chainId RPC error on {Network}", network);
            return EthereumUtilityChainIdResult.Fail("RPC_ERROR", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/chainId unexpected error on {Network}", network);
            return EthereumUtilityChainIdResult.Fail("UNEXPECTED_ERROR", ex.Message);
        }
    }

    /// <summary>UTIL04 — utility/encodeFunctionData. Builds raw calldata for a contract call without sending one (feeds SC01-SC03/TX02's 'data' field).</summary>
    public Task<EthereumUtilityEncodeFunctionDataResult> EncodeFunctionDataAsync(string abiJson, string functionName, string? argsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = new Contract(null, abiJson, string.Empty);
            var function = contract.GetFunction(functionName);
            var args = ParseJsonArgs(argsJson);

            var data = function.GetData(args);
            var selector = data.Length >= 10 ? data.Substring(0, 10) : data;

            return Task.FromResult(EthereumUtilityEncodeFunctionDataResult.Ok(data, selector));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(EthereumUtilityEncodeFunctionDataResult.Fail("VAL_INVALID_ARGS", $"Config key 'args' is not valid JSON: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/encodeFunctionData unexpected error for {Function}", functionName);
            return Task.FromResult(EthereumUtilityEncodeFunctionDataResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>UTIL05 — utility/decodeFunctionData. Same selector-matching + FunctionCallDecoder approach as TX05 (transaction/decode), exposed as its own operation.</summary>
    public Task<EthereumUtilityDecodeFunctionDataResult> DecodeFunctionDataAsync(string abiJson, string data, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(data) || data.Length < 10)
                return Task.FromResult(EthereumUtilityDecodeFunctionDataResult.Fail("VAL_INVALID_DATA", "data must be at least a 4-byte function selector (0x + 8 hex chars)."));

            var selector = data.Substring(0, 10);
            var selectorHex = selector.Substring(2);

            var contract = new Contract(null, abiJson, string.Empty);
            var function = contract.ContractBuilder.ContractABI.Functions
                .FirstOrDefault(f => string.Equals(f.Sha3Signature, selectorHex, StringComparison.OrdinalIgnoreCase));

            if (function is null)
                return Task.FromResult(EthereumUtilityDecodeFunctionDataResult.Fail("FUNCTION_NOT_FOUND", $"No function in the supplied ABI matches selector {selector}."));

            var decoder = new FunctionCallDecoder();
            var outputs = decoder.DecodeInput(function, data);

            var decodedArgs = function.InputParameters
                .Select((p, i) => new { p.Name, Value = outputs.ElementAtOrDefault(i)?.Result?.ToString() ?? string.Empty })
                .ToDictionary(x => x.Name, x => x.Value);

            return Task.FromResult(EthereumUtilityDecodeFunctionDataResult.Ok(function.Name, selector, decodedArgs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/decodeFunctionData failed");
            return Task.FromResult(EthereumUtilityDecodeFunctionDataResult.Fail("DECODE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// UTIL06 — utility/encodeEventTopics. Topic0 is always the event signature hash. Indexed
    /// filter values: address/uintN/bool encode as a 32-byte left-padded hex value (the ABI
    /// "static type" topic encoding); string/bytes indexed values encode as their own keccak256
    /// hash per the ABI spec for indexed dynamic types (the raw value is never recoverable from
    /// a topic for those types — this is standard EVM behaviour, not a limitation of this code).
    ///
    /// // VERIFY: Nethereum has no dedicated "EventTopicEncoder" helper class (confirmed absent from
    /// Nethereum.ABI.FunctionEncoding) — the static/dynamic-type topic encoding below is implemented
    /// directly against the ABI spec rather than delegated to a Nethereum API, unlike every other
    /// encode/decode operation in this resource. Verify against a real indexed-parameter event
    /// (e.g. ERC-20 Transfer's indexed 'to' address) before relying on this for anything beyond
    /// simple address/uint/bool filters.
    /// </summary>
    public Task<EthereumUtilityEncodeEventTopicsResult> EncodeEventTopicsAsync(string eventAbiJson, string? argsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventAbi = ParseSingleEventAbi(eventAbiJson);
            var topic0 = "0x" + eventAbi.Sha3Signature;

            var topics = new List<string?> { topic0 };
            var args = string.IsNullOrWhiteSpace(argsJson) ? Array.Empty<JsonElement>() : ParseJsonElementArray(argsJson);
            var indexedParams = eventAbi.InputParameters.Where(p => p.Indexed).ToArray();

            for (var i = 0; i < args.Length && i < indexedParams.Length; i++)
            {
                topics.Add(args[i].ValueKind == JsonValueKind.Null ? null : EncodeIndexedTopicValue(indexedParams[i].Type, args[i]));
            }

            return Task.FromResult(EthereumUtilityEncodeEventTopicsResult.Ok(topics));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(EthereumUtilityEncodeEventTopicsResult.Fail("VAL_INVALID_EVENT_ABI", $"Config key 'eventAbi'/'args' is not valid JSON: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/encodeEventTopics unexpected error");
            return Task.FromResult(EthereumUtilityEncodeEventTopicsResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// UTIL07 — utility/decodeEventLog. LOAD-BEARING. Decodes a raw log's topics/data into named
    /// event arguments via Nethereum's <see cref="EventTopicDecoder"/>. Topics[0] (the event
    /// signature hash) is excluded before decoding — Nethereum's decoder expects only the
    /// value-carrying topics (indexed params) plus the ABI-encoded non-indexed data.
    /// </summary>
    public Task<EthereumUtilityDecodeEventLogResult> DecodeEventLogAsync(string eventAbiJson, string topicsJson, string data, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventAbi = ParseSingleEventAbi(eventAbiJson);
            var allTopics = ParseJsonArgs(topicsJson).Select(t => t?.ToString() ?? string.Empty).ToArray();
            if (allTopics.Length == 0)
                return Task.FromResult(EthereumUtilityDecodeEventLogResult.Fail("VAL_MISSING_TOPICS", "Config key 'topics' must contain at least the event signature hash (topic0)."));

            // topics[0] is the event signature hash — already identified by 'eventAbi', not decoded as a value.
            var indexedTopics = allTopics.Skip(1).Cast<object>().ToArray();

            var decoder = new EventTopicDecoder();
            var outputs = decoder.DecodeDefaultTopics(eventAbi, indexedTopics, data);

            var decodedArgs = eventAbi.InputParameters
                .Select((p, i) => new { p.Name, Value = outputs.ElementAtOrDefault(i)?.Result?.ToString() ?? string.Empty })
                .ToDictionary(x => x.Name, x => x.Value);

            return Task.FromResult(EthereumUtilityDecodeEventLogResult.Ok(eventAbi.Name, decodedArgs));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(EthereumUtilityDecodeEventLogResult.Fail("VAL_INVALID_EVENT_ABI", $"Config key 'eventAbi'/'topics' is not valid JSON: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/decodeEventLog failed");
            return Task.FromResult(EthereumUtilityDecodeEventLogResult.Fail("DECODE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// UTIL08 — utility/contractAddress. CREATE (nonce-based) via <see cref="ContractUtils.CalculateContractAddress(string, BigInteger)"/>;
    /// CREATE2 (salt-based) via <see cref="ContractUtils.CalculateCreate2AddressUsingByteCodeHash"/> when
    /// 'bytecodeHash'+'salt' are supplied instead of 'nonce'.
    /// </summary>
    public Task<EthereumUtilityContractAddressResult> GetContractAddressAsync(
        string from, long? nonce, string? bytecodeHash, string? salt, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(bytecodeHash) && !string.IsNullOrWhiteSpace(salt))
            {
                var create2Address = ContractUtils.CalculateCreate2AddressUsingByteCodeHash(from, salt, bytecodeHash);
                return Task.FromResult(EthereumUtilityContractAddressResult.Ok(create2Address, "CREATE2"));
            }

            if (nonce.HasValue)
            {
                var createAddress = ContractUtils.CalculateContractAddress(from, new BigInteger(nonce.Value));
                return Task.FromResult(EthereumUtilityContractAddressResult.Ok(createAddress, "CREATE"));
            }

            return Task.FromResult(EthereumUtilityContractAddressResult.Fail(
                "VAL_MISSING_PARAMETERS", "Provide either 'nonce' (for CREATE) or both 'bytecodeHash' and 'salt' (for CREATE2)."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "utility/contractAddress unexpected error for {From}", from);
            return Task.FromResult(EthereumUtilityContractAddressResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    // ── shared helpers ──────────────────────────────────────────────────────

    private static UnitConversion.EthUnit? ParseUnit(string unit) => NormalizeUnit(unit) switch
    {
        "wei" => UnitConversion.EthUnit.Wei,
        "gwei" => UnitConversion.EthUnit.Gwei,
        "ether" => UnitConversion.EthUnit.Ether,
        _ => null,
    };

    private static string NormalizeUnit(string? unit) => string.IsNullOrWhiteSpace(unit) ? string.Empty : unit.Trim().ToLowerInvariant();

    private static object[] ParseJsonArgs(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<object>();

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a JSON array.");

        return doc.RootElement.EnumerateArray().Select(ConvertJsonElement).ToArray();
    }

    private static JsonElement[] ParseJsonElementArray(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a JSON array.");
        return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToArray();
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

    /// <summary>See the // VERIFY: note on <see cref="EncodeEventTopicsAsync"/>.</summary>
    private static string EncodeIndexedTopicValue(string abiType, JsonElement value)
    {
        // Dynamic types (string, bytes, arrays): the topic is the keccak256 hash of the ABI-encoded value.
        if (abiType is "string" or "bytes" || abiType.EndsWith("[]", StringComparison.Ordinal))
        {
            var raw = value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();
            var hashBytes = Sha3Keccack.Current.CalculateHashFromHex(raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? raw : StringToHex(raw));
            return "0x" + hashBytes;
        }

        // Static types (address, uintN, intN, bool, bytesN): left-pad the ABI-encoded value to 32 bytes.
        if (abiType == "address")
        {
            var hex = value.GetString()?.Replace("0x", "", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            return "0x" + hex.PadLeft(64, '0');
        }

        if (abiType == "bool")
        {
            var boolValue = value.ValueKind == JsonValueKind.True;
            return "0x" + (boolValue ? "1" : "0").PadLeft(64, '0');
        }

        // uintN / intN / bytesN
        var numeric = value.ValueKind switch
        {
            JsonValueKind.Number => new HexBigInteger(BigInteger.Parse(value.GetRawText())).HexValue,
            JsonValueKind.String => value.GetString() ?? "0x0",
            _ => "0x0",
        };
        var numericHex = numeric.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? numeric.Substring(2) : numeric;
        return "0x" + numericHex.PadLeft(64, '0');
    }

    private static string StringToHex(string value) =>
        "0x" + Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(value));
}
