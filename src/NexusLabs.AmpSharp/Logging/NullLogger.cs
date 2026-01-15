using Microsoft.Extensions.Logging;

namespace NexusLabs.AmpSharp.Logging;

/// <summary>
/// A no-op logger implementation that does not log anything.
/// This is used as the default logger when no custom logger is provided.
/// </summary>
internal sealed class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new();

    private NullLogger()
    {
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // No-op
    }
}

/// <summary>
/// A generic no-op logger implementation.
/// </summary>
/// <typeparam name="T">The type associated with the logger.</typeparam>
internal sealed class NullLogger<T> : ILogger<T>
{
    public static readonly NullLogger<T> Instance = new();

    private NullLogger()
    {
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // No-op
    }
}
