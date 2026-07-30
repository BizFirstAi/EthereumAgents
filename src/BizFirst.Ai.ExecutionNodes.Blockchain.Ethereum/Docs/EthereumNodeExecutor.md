# Ethereum ExecutionNode — Operation Reference

`NodeTypeName`: `ethereum` · Area: `Blockchain`

Every operation takes `resource`, `operation`, and an optional `network` (defaults to the
`Ethereum:DefaultNetwork` application setting) as config keys, plus the operation-specific fields
listed below. Write operations additionally require a vault `credentialID` pointing at a secret
holding the wallet's private key.

## account

| operation | fields | notes |
|---|---|---|
| `balance` | `address`*, `format` (wei\|gwei\|ether), `block` | |
| `nonce` | `address`*, `block` | |
| `history` | `address`*, `fromBlock`, `toBlock` | Requires an indexer API — not wired up in this build; fails with `INDEXER_NOT_CONFIGURED`. |
| `code` | `address`*, `block` | Empty bytecode → wallet (EOA); non-empty → contract. |

## block

| operation | fields | notes |
|---|---|---|
| `get` | `blockNumberOrHash`, `includeTransactions` | |
| `number` | — | |
| `list` | `count`* (1-100), `fromBlock` | Composite: fans out to N `get` calls. |

## transaction

| operation | fields | notes |
|---|---|---|
| `get` | `hash`* | |
| `send` | `to`*, `value`* (wei), `data`, `gasLimit`, `maxFeePerGas`, `maxPriorityFeePerGas`, `nonce` | Needs credential. Core operation. |
| `receipt` | `hash`* | Real success/revert status + logs + gas used. |
| `pending` | `addressFilter` | Provider-dependent; may return `NOT_SUPPORTED`. |
| `decode` | `data`*, `abi` | Without `abi`, only the function selector is reported. |
| `wait` | `hash`*, `confirmations`, `timeoutMs` | Polls until confirmed or timeout. |

## token (ERC-20)

| operation | fields | notes |
|---|---|---|
| `balance` | `tokenAddress`*, `ownerAddress`*, `formatDecimals` | |
| `transfer` | `tokenAddress`*, `to`*, `amount`* | Needs credential. Most popular operation. |
| `approve` | `tokenAddress`*, `spender`*, `amount`* | Needs credential. |
| `transferFrom` | `tokenAddress`*, `from`*, `to`*, `amount`* | Needs credential. |
| `allowance` | `tokenAddress`*, `owner`*, `spender`*, `formatDecimals` | |
| `totalSupply` | `tokenAddress`*, `formatDecimals` | |
| `decimals` | `tokenAddress`* | |
| `name` | `tokenAddress`* | |
| `symbol` | `tokenAddress`* | |

No mint/burn — not part of the ERC-20 standard, and would revert against USDC/USDT/DAI.

## contract

| operation | fields | notes |
|---|---|---|
| `read` | `contractAddress`*, `abi`*, `functionName`*, `functionArgs`, `block` | `view`/`pure` functions. |
| `write` | `contractAddress`*, `abi`*, `functionName`*, `functionArgs`, `value`, `gasLimit` | Needs credential. |
| `simulate` | `contractAddress`*, `abi`*, `functionName`*, `functionArgs`, `value`, `from` | Preview a state-changing call without sending. |
| `deploy` | `bytecode`*, `abi`*, `constructorArgs`, `value`, `gasLimit` | Needs credential. |
| `multicall` | `calls`* (JSON array) | Batched reads. |
| `logs` | `contractAddress`, `eventAbi`*, `fromBlock`, `toBlock`, `topics` | Historical query — pair with `utility/decodeEventLog`. |

## ens

| operation | fields |
|---|---|
| `resolve` | `name`* |
| `reverse` | `address`* |
| `avatar` | `name`* |
| `text` | `name`*, `key`* |
| `resolver` | `name`* |

## gas

| operation | fields |
|---|---|
| `estimate` | `to`*, `value`, `data`, `from` |
| `price` | — |
| `feeHistory` | `blockCount`*, `newestBlock`, `rewardPercentiles` |
| `priorityFee` | — |
| `optimize` | `operations`, `strategy` (safe\|standard\|fast) |

## utility

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

## nft (ERC-721)

| operation | fields |
|---|---|
| `balance` | `contractAddress`*, `ownerAddress`* |
| `ownerOf` | `contractAddress`*, `tokenId`* |
| `transferFrom` | `contractAddress`*, `from`*, `to`*, `tokenId`* — needs credential |
| `safeTransferFrom` | `contractAddress`*, `from`*, `to`*, `tokenId`* — needs credential |
| `approve` | `contractAddress`*, `spender`*, `tokenId`* — needs credential |
| `setApprovalForAll` | `contractAddress`*, `operator`*, `approved`* — needs credential |
| `getApproved` | `contractAddress`*, `tokenId`* |
| `isApprovedForAll` | `contractAddress`*, `owner`*, `operator`* |
| `tokenUri` | `contractAddress`*, `tokenId`* |

`*` = required.
