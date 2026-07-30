namespace BizFirst.Integration.Ethereum.Services.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using BizFirst.Integration.Ethereum.Domain;
using BizFirst.Integration.Ethereum.Services;

/// <summary>
/// DI extension methods for Ethereum integration services. Registers network configuration,
/// the shared RPC connection provider (with rate-limit-aware typed HttpClient — Guideline 04
/// Flavour B), and one service per resource.
/// </summary>
public static class EthereumServicesExtensions
{
    public static IServiceCollection AddDependenciesEthereumServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Bound lazily against IConfiguration resolved from DI when EthereumNetworkOptions is first
        // requested — deliberately NOT services.BuildServiceProvider() here, which would be an
        // anti-pattern mid-registration (expensive, and IConfiguration may not be registered yet
        // at the point RegisterDefaults runs).
        services.AddOptions<EthereumNetworkOptions>().BindConfiguration(EthereumNetworkOptions.SectionName);

        services.AddTransient<EthereumRateLimitHandler>();
        services.AddHttpClient(EthereumConnectionProvider.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<EthereumRateLimitHandler>();

        // Connection provider: caches read-only Web3 clients per network (singleton, like RedisConnectionCache).
        services.AddSingleton<EthereumConnectionProvider>();

        // NFT metadata fetcher (addendum v3, NFT12): separate named HttpClient from the RPC one
        // above — different timeout profile (per-call, not shared) and no rate-limit handler,
        // since this hits arbitrary tokenURI hosts/IPFS gateways, not a single RPC provider.
        services.AddHttpClient(EthereumNftMetadataFetcher.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<EthereumNftMetadataFetcher>();

        // Resource services (scoped, concrete classes — matches the Redis convention: no interfaces).
        services.AddScoped<EthereumAccountService>();
        services.AddScoped<EthereumBlockService>();
        services.AddScoped<EthereumTransactionService>();
        services.AddScoped<EthereumTokenService>();
        services.AddScoped<EthereumContractService>();
        services.AddScoped<EthereumEnsService>();
        services.AddScoped<EthereumGasService>();
        services.AddScoped<EthereumUtilityService>();
        services.AddScoped<EthereumNftService>();
        services.AddScoped<EthereumWalletService>();

        return services;
    }
}
