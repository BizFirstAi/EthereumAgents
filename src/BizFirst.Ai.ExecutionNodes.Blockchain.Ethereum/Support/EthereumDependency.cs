using BizFirst.Ai.ProcessEngine.Service;
namespace BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum;

/// <summary>
/// DI registration for the Ethereum ExecutionNode plugin. Registers all Ethereum services, the
/// executor, and the executor registry entry.
///
/// IMPORTANT — this alone is not sufficient to make the node discoverable at runtime. Per this
/// codebase's plugin-loading mechanism (BizFirst.Ai.Platform.Web.Server.Core/DependencyInjection/
/// Ai/ServiceCollectionExtensionsForAI.cs), an assembly that is only ProjectReference'd but never
/// force-loaded is not guaranteed to be present in AppDomain.CurrentDomain.GetAssemblies() when the
/// assembly-scanning RegisterNodeExecutors() runs — see the explicit
/// "new EthereumDependency().RegisterDefaults(services);" line added to Plugins_RegisterAllNodes(...)
/// in that file. Without both a ProjectReference AND that explicit line, this node will not register
/// (this is the same reason Slack's own registration line is currently commented out there and Slack
/// is presently inert in the running application, despite implementing this interface correctly).
/// </summary>
public sealed class EthereumDependency : INodeExecutorDependency
{
    public void RegisterDefaults(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        // AddDependenciesEthereumServices binds EthereumNetworkOptions lazily via
        // IOptions<T>/BindConfiguration — it resolves the host's IConfiguration from DI only when
        // the options are first requested, not here, so RegisterDefaults never needs to build a
        // service provider mid-registration.
        services.AddDependenciesEthereumServices();
        services.AddScoped<EthereumNodeExecutor>();
        ExecutorRegistry.Register<EthereumNodeExecutor>(EthereumNodeExecutor.NodeTypeName);
    }
}
