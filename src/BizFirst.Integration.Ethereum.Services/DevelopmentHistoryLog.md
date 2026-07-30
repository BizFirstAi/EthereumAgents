# Ethereum Integration Services — Development History

## Initial build (2026-07-20)
- `EthereumConnectionProvider` — caches read-only Nethereum `Web3` clients per network; builds fresh signed clients per call (never cached, since they carry a private key)
- `EthereumRateLimitHandler` — 429 retry `DelegatingHandler` for RPC providers (Alchemy/Infura), mirrors Slack's `SlackRateLimitHandler`
- One service per resource: Account, Block, Transaction, Token, Contract, Ens, Gas, Utility, Nft
- `StandardAbis` — minimal ERC-20/ERC-721 ABI constants (avoids depending on the optional `Nethereum.Contracts.Standards` package)
