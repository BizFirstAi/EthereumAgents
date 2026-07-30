namespace BizFirst.Integration.Ethereum.Services;

using Nethereum.HdWallet;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

/// <summary>
/// Wallet resource (addendum v3): local cryptographic signing/verification. No RPC call is
/// involved in any of the 3 operations here — the private key is resolved by the caller (same
/// vault-based credentialID pattern as every other write operation) and signing happens entirely
/// against local key material. <see cref="VerifySignatureAsync"/> needs no private key at all —
/// it's pure signature recovery.
/// </summary>
public sealed class EthereumWalletService
{
    private readonly ILogger<EthereumWalletService> _logger;

    public EthereumWalletService(ILogger<EthereumWalletService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>SIGN01 — wallet/signMessage (EIP-191 personal_sign).</summary>
    public Task<EthereumWalletSignMessageResult> SignMessageAsync(string privateKeyHex, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = new EthECKey(privateKeyHex);
            var signature = new EthereumMessageSigner().EncodeUTF8AndSign(message, key);
            return Task.FromResult(EthereumWalletSignMessageResult.Ok(key.GetPublicAddress(), signature));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "wallet/signMessage unexpected error");
            return Task.FromResult(EthereumWalletSignMessageResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// SIGN02 — wallet/signTypedData (EIP-712). <paramref name="typedDataJson"/> is the full
    /// standard EIP-712 envelope ({types, primaryType, domain, message}) as a raw JSON string —
    /// the caller supplies it directly, no per-dApp C# struct classes are needed.
    /// </summary>
    public Task<EthereumWalletSignTypedDataResult> SignTypedDataAsync(string privateKeyHex, string typedDataJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = new EthECKey(privateKeyHex);
            var signature = new Eip712TypedDataSigner().SignTypedDataV4(typedDataJson, key);
            return Task.FromResult(EthereumWalletSignTypedDataResult.Ok(key.GetPublicAddress(), signature));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "wallet/signTypedData unexpected error");
            return Task.FromResult(EthereumWalletSignTypedDataResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>SIGN03 — wallet/verifySignature. Recovers the signer address; no credential required.</summary>
    public Task<EthereumWalletVerifySignatureResult> VerifySignatureAsync(string message, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var address = new EthereumMessageSigner().EncodeUTF8AndEcRecover(message, signature);
            return Task.FromResult(EthereumWalletVerifySignatureResult.Ok(address));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "wallet/verifySignature unexpected error");
            return Task.FromResult(EthereumWalletVerifySignatureResult.Fail("UNEXPECTED_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// Derives the account-0 private key (hex, 0x-prefixed) from a BIP-39 mnemonic, for
    /// CRYPTO_WALLET credentials that supplied a seed phrase instead of a raw private key.
    /// <paramref name="derivationPath"/> defaults to Nethereum.HdWallet's own standard Ethereum
    /// path (<c>m/44'/60'/0'/0</c>, account index 0 — i.e. the same default the wallet management
    /// design doc specifies as <c>m/44'/60'/0'/0/0</c>). Pure local computation — no RPC call,
    /// synchronous. Throws on a malformed mnemonic/path; the caller wraps this in its own try/catch
    /// alongside the rest of credential resolution.
    /// </summary>
    public string DerivePrivateKeyFromMnemonic(string mnemonic, string? derivationPath)
    {
        var wallet = string.IsNullOrWhiteSpace(derivationPath)
            ? new Wallet(mnemonic, seedPassword: string.Empty)
            : new Wallet(mnemonic, seedPassword: string.Empty, path: derivationPath);

        var account = wallet.GetAccount(0);
        return account.PrivateKey;
    }
}
