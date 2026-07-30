# Ethereum ExecutionNode — Operation Reference

`NodeTypeName`: `ethereum` · Area: `Blockchain`

Every operation takes `resource`, `operation`, and an optional `network` (defaults to the
`Ethereum:DefaultNetwork` application setting) as config keys, plus the operation-specific fields
listed below. Write operations additionally require a vault `credentialID` pointing at a secret
holding the wallet's private key (a `CRYPTO_WALLET` credential).

10 resources, 64 defined operations (63 currently reachable through the node — see
[Known gaps](#known-gaps)).

## account

| operation | fields | notes |
|---|---|---|
| `balance` | `address`*, `format` (wei\|gwei\|ether), `block` | |
| `nonce` | `address`*, `block` | |
| `code` | `address`*, `block` | Empty bytecode ("0x") → wallet (EOA); non-empty → contract. |
| `history` | `address`*, `fromBlock`, `toBlock` | Requires an Etherscan-compatible indexer API — not wired up in this build; fails with `INDEXER_NOT_CONFIGURED`. |

## block

| operation | fields | notes |
|---|---|---|
| `get` | `blockNumberOrHash`, `includeTransactions` | |
| `number` | — | |
| `list` | `count`* (1-100, default 10), `fromBlock` | Composite: fans out to N `get` calls. |

## transaction

| operation | fields | notes |
|---|---|---|
| `get` | `hash`* | |
| `send` | `to`*, `value` (wei), `data`, `gasLimit`, `maxFeePerGas`, `maxPriorityFeePerGas`, `nonce` | Needs credential. Core write operation. |
| `receipt` | `hash`* | Real `eth_getTransactionReceipt` — success/revert status + logs + gas used. |
| `pending` | `addressFilter` | Provider-dependent; may not be supported by every RPC provider. |
| `decode` | `data`*, `abi` | Without `abi`, only the 4-byte function selector is reported. |
| `wait` | `hash`*, `confirmations`, `timeoutMs` | Polls until confirmed or timeout. |

## token (ERC-20)

| operation | fields | notes |
|---|---|---|
| `balance` | `tokenAddress`*, `ownerAddress`*, `formatDecimals` | |
| `transfer` | `tokenAddress`*, `to`*, `amount`* | Needs credential. Most popular write operation. |
| `approve` | `tokenAddress`*, `spender`*, `amount`* | Needs credential. |
| `transferFrom` | `tokenAddress`*, `from`*, `to`*, `amount`* | Needs credential. |
| `allowance` | `tokenAddress`*, `owner`*, `spender`*, `formatDecimals` | |
| `totalSupply` | `tokenAddress`*, `formatDecimals` | |
| `decimals` | `tokenAddress`* | |
| `name` | `tokenAddress`* | |
| `symbol` | `tokenAddress`* | |
| `mint` | `tokenAddress`*, `to`*, `amount`, `functionName` (default `mint`) | Needs credential. Not part of EIP-20 — assumes a common OpenZeppelin-style shape; reverts against USDC/USDT/DAI. |
| `burn` | `tokenAddress`*, `amount`, `from` | Needs credential. Matches OpenZeppelin's ERC20Burnable extension. |

## nft (ERC-721)

| operation | fields | notes |
|---|---|---|
| `balance` | `contractAddress`*, `ownerAddress`* | |
| `ownerOf` | `contractAddress`*, `tokenId`* | |
| `transferFrom` | `contractAddress`*, `from`*, `to`*, `tokenId`* | Needs credential. |
| `safeTransferFrom` | `contractAddress`*, `from`*, `to`*, `tokenId`* | Needs credential. Preferred over `transferFrom`. |
| `approve` | `contractAddress`*, `spender`*, `tokenId`* | Needs credential. |
| `setApprovalForAll` | `contractAddress`*, `operator`*, `approved`* | Needs credential. |
| `getApproved` | `contractAddress`*, `tokenId`* | |
| `isApprovedForAll` | `contractAddress`*, `owner`*, `operator`* | |
| `tokenUri` | `contractAddress`*, `tokenId`* | |
| `mint` | `contractAddress`*, `to`*, `tokenId`, `functionName` (default `safeMint`) | Needs credential. Not part of EIP-721. |
| `listOwnedTokens` | `contractAddress`*, `ownerAddress`*, `limit` (1-1000, default 100), `offset` | Gated on ERC-721 Enumerable (`0x780e9d63`); fails with `NOT_SUPPORTED` when absent. |
| `getMetadata` | `contractAddress`*, `tokenId`*, `timeoutSeconds` (1-60, default 10) | Calls `tokenURI()`, resolves `ipfs://`, HTTP-fetches and returns the metadata JSON. This node's only non-RPC outbound HTTP call. |

## contract

| operation | fields | notes |
|---|---|---|
| `read` | `contractAddress`*, `abi`*, `functionName`*, `functionArgs`, `block` | `view`/`pure` functions. |
| `write` | `contractAddress`*, `abi`*, `functionName`*, `functionArgs`, `value`, `gasLimit` | Needs credential. |
| `simulate` | `contractAddress`*, `abi`*, `functionName`*, `functionArgs`, `value`, `from` | Preview a state-changing call via `eth_call`, without sending. |
| `deploy` | `bytecode`*, `abi`*, `constructorArgs`, `value`, `gasLimit` | Needs credential. Returns only the deployment tx hash. |
| `multicall` | `calls`* (JSON array) | Batched reads; partial success per entry. |
| `detectStandards` | `contractAddress`* | Real ERC-165 probe + an ERC-20 heuristic. |
| `logs` | `contractAddress`, `eventAbi`*, `fromBlock`, `toBlock`, `topics` | **Implemented but not currently routed** — see [Known gaps](#known-gaps). |

## ens

All 5 read-only. Manual namehash + Registry/Resolver implementation; scoped to Ethereum mainnet
(and some testnets) — the ENS Registry generally isn't deployed on L2s.

| operation | fields |
|---|---|
| `resolve` | `name`* |
| `reverse` | `address`* |
| `avatar` | `name`* |
| `text` | `name`*, `key`* |
| `resolver` | `name`* |

## gas

| operation | fields | notes |
|---|---|---|
| `estimate` | `to`*, `value`, `data`, `from` | |
| `price` | — | Legacy `eth_gasPrice`. |
| `feeHistory` | `blockCount`*, `newestBlock`, `rewardPercentiles` | `eth_feeHistory`. |
| `priorityFee` | — | `eth_maxPriorityFeePerGas`. |
| `optimize` | `operations`, `strategy` (safe\|standard\|fast, default standard) | Composite heuristic built on the three primitives above — not empirically validated. |

## wallet

All 3 are pure local cryptography — no RPC call.

| operation | fields | notes |
|---|---|---|
| `signMessage` | `message`* | Needs credential. EIP-191 (`personal_sign`). |
| `signTypedData` | `typedDataJson`* (JSON) | Needs credential. EIP-712. |
| `verifySignature` | `message`*, `signature`* | No credential required — pure signature recovery. |

## utility

Mostly local; only `chainId` makes an RPC call.

| operation | fields |
|---|---|
| `validateAddress` | `address`* |
| `convertUnits` | `value`*, `fromUnit`*, `toUnit`* |
| `chainId` | — |
| `encodeFunctionData` | `abi`*, `functionName`*, `args` |
| `decodeFunctionData` | `abi`*, `data`* |
| `encodeEventTopics` | `eventAbi`*, `args` |
| `decodeEventLog` | `eventAbi`*, `topics`*, `data`* |
| `contractAddress` | `from`*, `nonce` (CREATE) or `bytecodeHash`+`salt` (CREATE2) |

`*` = required.

### Supported networks

Chain-agnostic by design — any EVM-compatible network works via a plain JSON-RPC URL and chain ID:

- `ethereum` — Ethereum mainnet (**default network**, chain ID 1)
- `polygon`, `arbitrum`, `optimism`, `base` — popular L2s/sidechains, same node code
- `sepolia` — Ethereum's public testnet, free faucet ETH
- any local dev chain (Anvil/Hardhat/Ganache) or private/enterprise EVM chain

Add more networks via the `Ethereum:Networks` appsettings section.

## Known gaps

- **account/history** requires an Etherscan-compatible indexer API that isn't wired up — standard
  JSON-RPC has no method to list an address's past transactions. Fails clearly with
  `INDEXER_NOT_CONFIGURED` rather than pretending to work.
- **contract/logs** is fully implemented (feature partial + service method both exist) but its case
  in the operation-routing switch is currently commented out, so it isn't reachable through the node
  today — a wiring gap, not a missing feature.
- **Triggers** (block/transaction/contract-event/interval listeners) are out of scope for this
  action-node build. Trigger-node architecture is an open question in the design docs.
- **ENS** resolution is a manual implementation (not the dedicated `Nethereum.ENS` package),
  cross-checked against the EIP-137 test vector but flagged for further verification beyond
  plain-ASCII `.eth` names on mainnet.
