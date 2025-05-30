using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace SpeechAgent.Core.Services;

/// <summary>
/// Implementation of the logging service that maintains in-memory logs
/// and integrates with Microsoft.Extensions.Logging
/// </summary>
public class LogService : ILogService
{
    private readonly ILogger<LogService> _logger;
    private readonly ObservableCollection<LogEntry> _logEntries;
    private readonly object _lock = new();

    public LogService(ILogger<LogService> logger)
    {
        _logger = logger;
        _logEntries = new ObservableCollection<LogEntry>();
    }

    public IReadOnlyList<LogEntry> LogEntries 
    { 
        get
        {
            lock (_lock)
            {
                return new List<LogEntry>(_logEntries);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<LogEntry>? LogAdded;

    public void LogDebug(string message, string component = "App")
    {
        AddLogEntry(LogLevel.Debug, message, component);
        _logger.LogDebug("[{Component}] {Message}", component, message);
    }

    public void LogInfo(string message, string component = "App")
    {
        AddLogEntry(LogLevel.Info, message, component);
        _logger.LogInformation("[{Component}] {Message}", component, message);
    }

    public void LogWarning(string message, string component = "App")
    {
        AddLogEntry(LogLevel.Warning, message, component);
        _logger.LogWarning("[{Component}] {Message}", component, message);
    }

    public void LogError(string message, string component = "App", Exception? exception = null)
    {
        AddLogEntry(LogLevel.Error, message, component, exception);
        if (exception != null)
        {
            _logger.LogError(exception, "[{Component}] {Message}", component, message);
        }
        else
        {
            _logger.LogError("[{Component}] {Message}", component, message);
        }
    }

    public void ClearLogs()
    {
        lock (_lock)
        {
            _logEntries.Clear();
        }
        OnPropertyChanged(nameof(LogEntries));
    }

    private void AddLogEntry(LogLevel level, string message, string component, Exception? exception = null)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Component = component,
            Message = message,
            Exception = exception
        };

        lock (_lock)
        {
            _logEntries.Add(logEntry);
            
            // Keep only the last 1000 entries to prevent memory issues
            if (_logEntries.Count > 1000)
            {
                _logEntries.RemoveAt(0);
            }
        }

        LogAdded?.Invoke(this, logEntry);
        OnPropertyChanged(nameof(LogEntries));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
