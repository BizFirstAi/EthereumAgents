namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>Result of AC04 — account/code. Empty bytecode ("0x") means the address is a wallet (EOA), not a contract.</summary>
public sealed record EthereumAccountCodeResult(bool Success, string Bytecode, bool IsContract, string ErrorCode, string ErrorMessage)
{
    public static EthereumAccountCodeResult Ok(string bytecode) =>
        new(true, bytecode, IsContract: !string.IsNullOrEmpty(bytecode) && bytecode != "0x", string.Empty, string.Empty);

    public static EthereumAccountCodeResult Fail(string errorCode, string errorMessage) =>
        new(false, "0x", false, errorCode, errorMessage);
}
