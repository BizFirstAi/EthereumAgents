namespace BizFirst.Integration.Ethereum.Services;

/// <summary>
/// <see cref="DelegatingHandler"/> that retries on HTTP 429 (Too Many Requests) — Alchemy/Infura and
/// most RPC providers enforce compute-unit/rate budgets and return 429 under load. Mirrors the
/// Slack integration's SlackRateLimitHandler pattern (Guideline 04, Flavour B).
/// </summary>
public sealed class EthereumRateLimitHandler : DelegatingHandler
{
    private const int MaxRetries = 3;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = null!;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0 && request.Content is not null)
                request.Content = await CloneContentAsync(request.Content, cancellationToken);

            response = await base.SendAsync(request, cancellationToken);

            if ((int)response.StatusCode != 429)
                return response;

            if (attempt == MaxRetries)
                break;

            var delay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(2 * (attempt + 1));
            await Task.Delay(delay, cancellationToken);
        }

        return response;
    }

    private static async Task<HttpContent> CloneContentAsync(HttpContent original, CancellationToken cancellationToken)
    {
        var bytes = await original.ReadAsByteArrayAsync(cancellationToken);
        var clone = new ByteArrayContent(bytes);
        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return clone;
    }
}
