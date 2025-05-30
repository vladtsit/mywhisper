using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SpeechAgent.Core.Services;

/// <summary>
/// Service for logging messages and providing access to runtime logs
/// </summary>
public interface ILogService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets all log entries
    /// </summary>
    IReadOnlyList<LogEntry> LogEntries { get; }

    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="component">The component name (e.g., "AudioRecorder", "MainWindow")</param>
    void LogDebug(string message, string component = "App");

    /// <summary>
    /// Logs an information message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="component">The component name</param>
    void LogInfo(string message, string component = "App");

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="component">The component name</param>
    void LogWarning(string message, string component = "App");

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="component">The component name</param>
    /// <param name="exception">Optional exception details</param>
    void LogError(string message, string component = "App", Exception? exception = null);

    /// <summary>
    /// Clears all log entries
    /// </summary>
    void ClearLogs();

    /// <summary>
    /// Event fired when a new log entry is added
    /// </summary>
    event EventHandler<LogEntry>? LogAdded;
}

/// <summary>
/// Represents a single log entry
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public string Component { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Exception? Exception { get; init; }

    public string FormattedMessage => 
        $"[{Timestamp:HH:mm:ss.fff}] [{Level}] [{Component}] {Message}" +
        (Exception != null ? $"\nException: {Exception}" : "");
}

/// <summary>
/// Log level enumeration
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
