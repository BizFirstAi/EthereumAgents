# Ethereum ExecutionNode — Development History

## Initial build (2026-07-20)
- `EthereumNodeExecutor` — routing for 55 action operations across 9 resources (`NodeTypeName = "ethereum"`, Area `Blockchain`)
- `BaseEthereumOperationInfo` / `EthereumOperationInfoFactory` — config-parsing layer (this was flagged as entirely missing from the original design docs; built from scratch here)
- Wallet private key resolved via the vault credential system (`credentialID` → `ReadCredentialValuePrimaryAsync`), never a config field
- `EthereumDependency : INodeExecutorDependency` — also requires an explicit force-load registration line in the platform host (see project README)
- Feature partials follow the platform's live 9-step `//code-step:` convention (Guideline 14), confirmed against real Smtp/Slack/Docker/Redis code, not just guideline prose
