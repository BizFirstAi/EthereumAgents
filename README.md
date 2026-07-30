# EthereumAgents

[![BizFirst.Ai](https://www.bizfirstai.com/website/assets/Logo/logo.png)](https://bizfirstai.com)

Ethereum community node for [BizFirst.Ai](https://bizfirstai.com) — a ProcessEngine
`ExecutionNode` (`ethereum`) that exposes Ethereum and any EVM-compatible chain (Polygon, Arbitrum,
Optimism, Base, and more) as drag-and-drop steps in [BizFirst.Ai](https://bizfirstai.com) workflow
automations, via [Nethereum](https://nethereum.com/) over JSON-RPC.

## What it does

`EthereumAgents` lets a BizFirst.Ai workflow read from and write to Ethereum (or any EVM-compatible
chain) without touching an SDK or managing a node connection. Unlike many read-only community
blockchain nodes, this one already ships real **signed writes** — token transfers, contract calls,
NFT transfers, contract deployment, and off-chain message signing — alongside a large read surface,
across 10 resources and 64 defined operations (63 currently reachable; see
[Known gaps](docs/README.md#known-gaps)).

| Resource | Operations | Description |
|---|---|---|
| `account` | `balance`, `nonce`, `code`, `history` | ETH balances, nonces, EOA/contract detection. |
| `block` | `get`, `number`, `list` | Block metadata and chain tip. |
| `transaction` | `get`, `send`, `receipt`, `pending`, `decode`, `wait` | Lookup, broadcast, decode, wait. |
| `token` (ERC-20) | `balance`, `transfer`, `approve`, `transferFrom`, `allowance`, `totalSupply`, `decimals`, `name`, `symbol`, `mint`, `burn` | Fungible token balances, transfers, and metadata. |
| `nft` (ERC-721) | `balance`, `ownerOf`, `transferFrom`, `safeTransferFrom`, `approve`, `setApprovalForAll`, `getApproved`, `isApprovedForAll`, `tokenUri`, `mint`, `listOwnedTokens`, `getMetadata` | Ownership, transfers, approvals, minting, metadata. |
| `contract` | `read`, `write`, `simulate`, `deploy`, `multicall`, `detectStandards` | Arbitrary ABI-driven calls, deployment, batched reads. |
| `ens` | `resolve`, `reverse`, `avatar`, `text`, `resolver` | Ethereum Name Service resolution. |
| `gas` | `estimate`, `price`, `feeHistory`, `priorityFee`, `optimize` | Fee estimation and recommendation. |
| `wallet` | `signMessage`, `signTypedData`, `verifySignature` | Off-chain message signing/verification (EIP-191/EIP-712). |
| `utility` | `validateAddress`, `convertUnits`, `chainId`, `encodeFunctionData`, `decodeFunctionData`, `encodeEventTopics`, `decodeEventLog`, `contractAddress` | Local ABI/unit/address helpers. |

Every operation accepts a `network` config key, defaulting to `Ethereum:DefaultNetwork` in
application settings. Write operations additionally require a `credentialID` config key pointing at
a vault `CRYPTO_WALLET` credential — the signing key is never a plain config field.

## Documentation

| Page | Published URL |
|---|---|
| Operation reference | https://bizfirstai.github.io/EthereumAgents/ |
| Guide: Overview | https://bizfirstai.github.io/EthereumAgents/guide/ |
| Guide: Configuration | https://bizfirstai.github.io/EthereumAgents/guide/01-configuration.html |
| Guide: Networks | https://bizfirstai.github.io/EthereumAgents/guide/02-networks.html |
| Guide: Account Operations | https://bizfirstai.github.io/EthereumAgents/guide/03-account-operations.html |
| Guide: Block Operations | https://bizfirstai.github.io/EthereumAgents/guide/04-block-operations.html |
| Guide: Transaction Operations | https://bizfirstai.github.io/EthereumAgents/guide/05-transaction-operations.html |
| Guide: Token Operations (ERC-20) | https://bizfirstai.github.io/EthereumAgents/guide/06-token-operations.html |
| Guide: NFT Operations (ERC-721) | https://bizfirstai.github.io/EthereumAgents/guide/07-nft-operations.html |
| Guide: Contract Operations | https://bizfirstai.github.io/EthereumAgents/guide/08-contract-operations.html |
| Guide: ENS Operations | https://bizfirstai.github.io/EthereumAgents/guide/09-ens-operations.html |
| Guide: Gas Operations | https://bizfirstai.github.io/EthereumAgents/guide/10-gas-operations.html |
| Guide: Wallet Operations | https://bizfirstai.github.io/EthereumAgents/guide/11-wallet-operations.html |
| Guide: Utility Operations | https://bizfirstai.github.io/EthereumAgents/guide/12-utility-operations.html |
| Guide: Input & Output | https://bizfirstai.github.io/EthereumAgents/guide/13-input-output.html |
| Guide: Examples | https://bizfirstai.github.io/EthereumAgents/guide/14-examples.html |
| Guide: Troubleshooting | https://bizfirstai.github.io/EthereumAgents/guide/15-troubleshooting.html |
| Guide: Roadmap | https://bizfirstai.github.io/EthereumAgents/guide/16-roadmap.html |

Same guide, also published in the portal: [bizfirstai.github.io/UserGuides/Nodes/Ethereum](https://bizfirstai.github.io/UserGuides/Nodes/Ethereum/)
Full developer portal: [docs.bizfirstai.com](https://docs.bizfirstai.com)

## Project layout

```
src/
├── BizFirst.Integration.Ethereum.Domain     # Result records + network options (zero deps)
├── BizFirst.Integration.Ethereum.Services   # Nethereum-backed JSON-RPC client + resource services
└── BizFirst.Ai.ExecutionNodes.Blockchain.Ethereum  # Executor: routing, config, operation DTOs
```

Targets **.NET 9**. Built on [Nethereum](https://nethereum.com/) 6.0.0.

## Configuration

```json
"Ethereum": {
  "DefaultNetwork": "ethereum",
  "RpcTimeoutSeconds": 30,
  "Networks": {
    "ethereum": { "RpcUrl": "https://eth-mainnet.g.alchemy.com/v2/YOUR_KEY", "ChainId": 1,   "BlockExplorer": "https://etherscan.io" },
    "polygon":  { "RpcUrl": "https://polygon-mainnet.g.alchemy.com/v2/YOUR_KEY", "ChainId": 137, "BlockExplorer": "https://polygonscan.com" }
  }
}
```

Two config layers: static application configuration above (`appsettings.json`, PascalCase) selects
each network's RPC endpoint and chain ID; per-node-instance configuration (workflow JSON, camelCase)
selects `resource`/`operation`/`network` and operation-specific fields per step. Provider API keys
should be kept out of plain `RpcUrl` — use `RpcUrlCredentialId` (a vault `SERVICE_URL` credential)
for any endpoint that embeds a secret. See the
[Configuration guide](https://bizfirstai.github.io/EthereumAgents/guide/01-configuration.html) for
the full field reference.

## Registration

`EthereumDependency.RegisterDefaults(services)` registers the RPC connection provider, the NFT
metadata fetcher, one service per resource, the executor (scoped), and the `ExecutorRegistry` entry
(`ethereum`). Host applications should also add `new EthereumDependency().RegisterDefaults(services);`
to their node-plugin bootstrap so the assembly is force-loaded and discoverable at runtime — a
`ProjectReference` alone is not sufficient in this codebase's plugin-loading mechanism.

## Roadmap

- **account/history** needs an Etherscan-compatible indexer API integration — standard JSON-RPC has
  no way to list an address's past transactions.
- **contract/logs** is fully implemented but not yet wired into the operation-routing switch — a
  wiring fix, not new feature work.
- **Trigger nodes** (block/transaction/contract-event/interval listeners) are an open architectural
  question, out of scope for this action-node pass.

See the [Roadmap guide](https://bizfirstai.github.io/EthereumAgents/guide/16-roadmap.html) for full detail.

## About BizFirst.Ai

[BizFirst.Ai](https://bizfirstai.com) is a workflow automation platform for building AI-driven business
processes. This node is one of many community connectors that plug into its ProcessEngine — browse the
full node catalogue and developer guides at [docs.bizfirstai.com](https://docs.bizfirstai.com), or join
the discussion at [community.bizfirstai.com](https://community.bizfirstai.com).

## License

Community node maintained by the [BizFirst.Ai](https://bizfirstai.com) team.
