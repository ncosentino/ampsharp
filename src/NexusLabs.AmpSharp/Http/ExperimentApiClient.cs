using System.Text;
using System.Text.Json;
using NexusLabs.AmpSharp.Models;
using Microsoft.Extensions.Logging;

namespace NexusLabs.AmpSharp.Http;

/// <summary>
/// HTTP client for making requests to the Amplitude Experiment API.
/// </summary>
internal sealed class ExperimentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _deploymentKey;
    private readonly RemoteEvaluationConfig _config;
    private readonly ILogger _logger;

    public ExperimentApiClient(
        HttpClient httpClient,
        string deploymentKey,
        RemoteEvaluationConfig config,
        ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _deploymentKey = deploymentKey ?? throw new ArgumentNullException(nameof(deploymentKey));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Set base address and default headers
        _httpClient.BaseAddress = new Uri(_config.GetServerUrl());
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Api-Key {_deploymentKey}");
    }

    /// <summary>
    /// Fetches variants for a user from the Amplitude Experiment API.
    /// </summary>
    /// <param name="user">The user context for evaluation.</param>
    /// <param name="options">Optional fetch options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary of flag keys to variants.</returns>
    public async Task<Dictionary<string, Variant>> FetchVariantsAsync(
        ExperimentUser user,
        FetchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Build query string
        var queryParams = BuildQueryString(user, options);
        var requestUri = $"/v1/vardata?{queryParams}";

        _logger.LogDebug("Fetching variants from {Uri}", requestUri);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_config.FetchTimeoutMillis);

        try
        {
            return await RetryHelper.ExecuteWithRetryAsync(
                async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                    // Add tracking header if needed
                    if (options?.TracksExposure == false && options?.TracksAssignment == false)
                    {
                        request.Headers.Add("X-Amp-Exp-Track", "no-track");
                    }

                    using var response = await _httpClient.SendAsync(request, cts.Token);

                    // Throw HttpRequestException with status code for retry logic
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                        _logger.LogWarning("Request failed with status {StatusCode}: {ErrorContent}",
                            response.StatusCode, errorContent);

                        throw new HttpRequestException(
                            $"Request failed with status {response.StatusCode}",
                            null,
                            response.StatusCode);
                    }

                    var content = await response.Content.ReadAsStringAsync(cts.Token);
                    _logger.LogDebug("Received response: {Content}", content);

                    var variants = JsonSerializer.Deserialize<Dictionary<string, Variant>>(content);
                    return variants ?? new Dictionary<string, Variant>();
                },
                _config.FetchRetries,
                _config.FetchRetryBackoffMinMillis,
                _config.FetchRetryBackoffMaxMillis,
                _config.FetchRetryBackoffScalar,
                _logger,
                cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Request timed out after {Timeout}ms", _config.FetchTimeoutMillis);
            throw new TimeoutException($"Request timed out after {_config.FetchTimeoutMillis}ms");
        }
    }

    /// <summary>
    /// Builds the query string for the API request.
    /// </summary>
    private string BuildQueryString(ExperimentUser user, FetchOptions? options)
    {
        var queryParams = new List<string>();

        // Add user_id and device_id if present
        if (!string.IsNullOrEmpty(user.UserId))
        {
            queryParams.Add($"user_id={Uri.EscapeDataString(user.UserId)}");
        }

        if (!string.IsNullOrEmpty(user.DeviceId))
        {
            queryParams.Add($"device_id={Uri.EscapeDataString(user.DeviceId)}");
        }

        // Add flag_keys if specified
        if (options?.FlagKeys != null && options.FlagKeys.Count > 0)
        {
            var flagKeys = string.Join(",", options.FlagKeys);
            queryParams.Add($"flag_keys={Uri.EscapeDataString(flagKeys)}");
        }

        // Serialize user context as JSON
        var contextJson = JsonSerializer.Serialize(user, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        queryParams.Add($"context={Uri.EscapeDataString(contextJson)}");

        return string.Join("&", queryParams);
    }
}
