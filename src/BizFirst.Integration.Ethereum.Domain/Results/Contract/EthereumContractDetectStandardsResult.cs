namespace BizFirst.Integration.Ethereum.Domain;

/// <summary>
/// Result of SC07 — contract/detectStandards. Two independent probes: ERC-165
/// (<see cref="Erc165Supported"/> + <see cref="SupportedStandards"/>, both real, verified
/// interface-ID lookups) and an ERC-20 heuristic (<see cref="Erc20Heuristic"/> — ERC-20 predates
/// ERC-165 and cannot be detected via supportsInterface at all).
/// </summary>
public sealed record EthereumContractDetectStandardsResult(
    bool Success,
    bool Erc165Supported,
    IReadOnlyList<string> SupportedStandards,
    bool Erc20Heuristic,
    string ErrorCode,
    string ErrorMessage)
{
    public static EthereumContractDetectStandardsResult Ok(bool erc165Supported, IReadOnlyList<string> supportedStandards, bool erc20Heuristic) =>
        new(true, erc165Supported, supportedStandards, erc20Heuristic, string.Empty, string.Empty);

    public static EthereumContractDetectStandardsResult Fail(string errorCode, string errorMessage) =>
        new(false, false, Array.Empty<string>(), false, errorCode, errorMessage);
}
