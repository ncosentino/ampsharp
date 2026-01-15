# AmpSharp - Amplitude Experiment C# SDK

**An unofficial C# SDK for [Amplitude Experiment](https://amplitude.com/docs/experiment) by Nexus Software Labs**

AmpSharp provides remote evaluation of feature flags and experiments for server-side .NET applications, enabling you to leverage Amplitude's powerful experimentation platform in your C# codebase.

> **Note:** This is an unofficial, community-maintained SDK. For official Amplitude SDKs, visit [Amplitude's documentation](https://amplitude.com/docs).

## Features

- 🚀 Remote evaluation of feature flags and experiments
- 🔄 Exponential backoff retry logic for resilient API calls
- 🔌 Built-in support for ASP.NET Core dependency injection
- 🛡️ Nullable reference types enabled for better null safety
- ✅ Comprehensive test coverage (28+ tests)
- 📦 .NET 10 compatible

## Installation

```bash
dotnet add package NexusLabs.AmpSharp
```

## Quick Start

### Basic Usage

```csharp
using NexusLabs.AmpSharp;
using NexusLabs.AmpSharp.Models;

// Initialize the client with your Amplitude deployment key
var experiment = Experiment.InitializeRemote("<DEPLOYMENT_KEY>");

// Create a user context
var user = new ExperimentUser
{
    UserId = "user@company.com",
    DeviceId = "abcdefg",
    UserProperties = new Dictionary<string, object>
    {
        ["premium"] = true,
        ["age"] = 25
    }
};

// Fetch variants
var variants = await experiment.FetchV2Async(user);

// Check a flag value
var variant = variants["YOUR-FLAG-KEY"];
if (variant?.Value == "on")
{
    // Flag is on - enable feature
}
```

### With Configuration

```csharp
var config = new RemoteEvaluationConfig
{
    ServerZone = ServerZone.EU,  // Use EU data center
    FetchTimeoutMillis = 5000,
    FetchRetries = 3
};

var experiment = Experiment.InitializeRemote("<DEPLOYMENT_KEY>", config);
```

### Fetch Specific Flags

```csharp
var options = new FetchOptions
{
    FlagKeys = new List<string> { "flag-1", "flag-2" },
    TracksExposure = true,
    TracksAssignment = false
};

var variants = await experiment.FetchV2Async(user, options);
```

## ASP.NET Core Integration

Register AmpSharp in your `Program.cs`:

```csharp
using NexusLabs.AmpSharp.Extensions;

// Add to service collection
builder.Services.AddAmplitudeExperiment("<DEPLOYMENT_KEY>", new RemoteEvaluationConfig
{
    ServerZone = ServerZone.US,
    FetchTimeoutMillis = 10000
});

// Or with configuration action
builder.Services.AddAmplitudeExperiment("<DEPLOYMENT_KEY>", config =>
{
    config.ServerZone = ServerZone.EU;
    config.FetchRetries = 5;
});
```

Then inject the client into your services:

```csharp
public class FeatureService
{
    private readonly IRemoteEvaluationClient _experiment;

    public FeatureService(IRemoteEvaluationClient experiment)
    {
        _experiment = experiment;
    }

    public async Task<bool> IsFeatureEnabled(string userId, string featureKey)
    {
        var user = new ExperimentUser { UserId = userId };
        var variants = await _experiment.FetchV2Async(user);

        return variants.TryGetValue(featureKey, out var variant)
            && variant.Value == "on";
    }
}
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `ServerZone` | Data center location (US or EU) | `ServerZone.US` |
| `ServerUrl` | Custom server URL (overrides ServerZone) | `null` |
| `FetchTimeoutMillis` | Request timeout in milliseconds | `10000` |
| `FetchRetries` | Number of retry attempts on failure | `8` |
| `FetchRetryBackoffMinMillis` | Minimum backoff delay | `500` |
| `FetchRetryBackoffMaxMillis` | Maximum backoff delay | `10000` |
| `FetchRetryBackoffScalar` | Exponential backoff scalar | `1.5` |
| `LogLevel` | Logging level | `LogLevel.Error` |
| `Logger` | Custom ILogger instance | `null` (uses NullLogger) |

## User Context

The `ExperimentUser` class supports all Amplitude user properties for sophisticated targeting:

```csharp
var user = new ExperimentUser
{
    // Identity
    UserId = "user-123",
    DeviceId = "device-456",

    // Location
    Country = "US",
    City = "San Francisco",
    Region = "CA",
    Language = "en",

    // Device
    Platform = "Web",
    OS = "macOS",
    Version = "1.0.0",
    DeviceModel = "MacBook Pro",

    // Custom properties
    UserProperties = new Dictionary<string, object>
    {
        ["subscription"] = "premium",
        ["age"] = 30,
        ["features"] = new[] { "beta", "alpha" }
    },

    // Groups
    Groups = new Dictionary<string, string[]>
    {
        ["company"] = new[] { "nexus-labs" },
        ["team"] = new[] { "engineering" }
    },

    // Cohorts
    CohortIds = new List<string> { "cohort-1", "cohort-2" }
};
```

## Error Handling and Retries

AmpSharp automatically retries failed requests with exponential backoff for:
- ✅ Network errors
- ✅ 5xx server errors
- ✅ 429 (Too Many Requests) rate limit errors

It does **NOT** retry:
- ❌ 4xx client errors (except 429)
- ❌ Invalid authentication (401)
- ❌ Malformed requests (400)

Example error handling:

```csharp
try
{
    var variants = await experiment.FetchV2Async(user);
}
catch (HttpRequestException ex)
{
    // Handle network or server errors
    _logger.LogError(ex, "Failed to fetch experiments");
    // Fallback to default behavior
}
catch (TimeoutException ex)
{
    // Handle timeout
    _logger.LogError(ex, "Request timed out");
}
```

## Development

### Prerequisites

- .NET 10.0 SDK or later
- C# 13 with nullable reference types

### Build

```bash
dotnet build ampsharp.slnx
```

### Test

```bash
dotnet test ampsharp.slnx
```

### Project Structure

```
ampsharp/
├── src/
│   └── NexusLabs.AmpSharp/         # Main library
│       ├── Client/                 # Client implementations
│       ├── Models/                 # Data models
│       ├── Http/                   # HTTP and retry logic
│       ├── Logging/                # Logging utilities
│       └── Extensions/             # DI extensions
├── tests/
│   └── NexusLabs.AmpSharp.Tests/   # Unit and integration tests
├── Directory.Packages.props         # Central package management
└── ampsharp.slnx                    # Solution file
```

## About Nexus Software Labs

AmpSharp is developed and maintained by **Nexus Software Labs**, a software consultancy focused on building high-quality developer tools and integrations.

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes with tests
4. Ensure all tests pass
5. Submit a pull request

## Support

For issues and questions:
- [GitHub Issues](https://github.com/nexus-labs/ampsharp/issues)
- [Amplitude Documentation](https://amplitude.com/docs/experiment)

## Acknowledgments

This SDK interfaces with Amplitude's Experiment platform. Amplitude and Amplitude Experiment are trademarks of Amplitude, Inc. This is an independent, unofficial implementation by Nexus Software Labs.
