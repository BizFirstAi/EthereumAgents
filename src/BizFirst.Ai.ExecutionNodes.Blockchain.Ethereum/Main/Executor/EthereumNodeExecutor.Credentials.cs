using BizFirst.Ai.AIExtension.Domain.Entities.CredentialParsers;
using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// Wallet credential resolution for write operations (transaction/send, token/transfer, etc.).
/// The private key/mnemonic is never a config field — it is resolved from the vault via the
/// standard "credentialID" config key (auto-wired by BaseNodeExecutor.RegisterCredentialsFromConfig).
/// Read here with <see cref="BaseNodeExecutor.ReadCredentialRawPrimaryAsync"/> (not the plain-string
/// helper) specifically so a CRYPTO_WALLET credential's <see cref="CryptoWalletRecord.SeedPhrase"/>
/// is reachable — the plain-string accessor only ever returns <see cref="CryptoWalletRecord.PrivateKey"/>,
/// silently discarding a seed-phrase-only credential.
/// It is never included in ToDictionary() or output data, and SensitiveDataScrubber already
/// redacts any config key containing "private" so nothing needs to be added for log safety.
/// </summary>
public sealed partial class EthereumNodeExecutor
{
    private async Task<(string? PrivateKey, NodeExecutionResult? Error)> _ResolveSigningKeyAsync(CancellationToken cancellationToken)
    {
        var raw = await ReadCredentialRawPrimaryAsync(isMandatory: true, cancellationToken).ConfigureAwait(false);

        string? privateKey;
        try
        {
            privateKey = raw switch
            {
                // Real CRYPTO_WALLET credential: prefer the raw private key when present.
                CryptoWalletRecord { PrivateKey.Length: > 0 } cw => cw.PrivateKey,
                // No private key, but a seed phrase was supplied — derive account 0's key from it.
                CryptoWalletRecord { SeedPhrase.Length: > 0 } cw => _walletService.DerivePrivateKeyFromMnemonic(cw.SeedPhrase!, cw.DerivationPath),
                // Wallet credential with neither field set.
                CryptoWalletRecord => null,
                // Back-compat: tolerate a differently-typed credential holding the raw key/password.
                ApiKeyRecord ak => ak.ApiKey,
                PasswordRecord pw => pw.Password,
                BasicAuthRecord ba => ba.Password,
                _ => null,
            };
        }
        catch (Exception ex)
        {
            var derivationError = resultManager.SetResultAsError(
                ExecutionConstants.OutputPorts.Error,
                $"Configured wallet credential's seed phrase could not be used to derive a signing key: {ex.Message}",
                this);
            return (null, derivationError);
        }

        if (string.IsNullOrWhiteSpace(privateKey))
        {
            var error = resultManager.SetResultAsError(
                ExecutionConstants.OutputPorts.Error,
                "No signing credential configured. Set 'credentialID' on this node to a vault entry (CRYPTO_WALLET type) containing a private key or seed phrase.",
                this);
            return (null, error);
        }

        return (privateKey, null);
    }
}
