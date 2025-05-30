using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SpeechAgent.Core.Services;

namespace SpeechAgent.UI.Views;

/// <summary>
/// Interaction logic for RuntimeLogsWindow.xaml
/// </summary>
public partial class RuntimeLogsWindow : Window, INotifyPropertyChanged
{
    private readonly ILogService _logService;
    private readonly IClipboardService _clipboardService;
    private ObservableCollection<LogEntry> _filteredLogEntries;
    private string _selectedLogLevel = "All";
    private bool _autoScrollEnabled = true;

    public RuntimeLogsWindow(ILogService logService, IClipboardService clipboardService)
    {
        InitializeComponent();
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        
        _filteredLogEntries = new ObservableCollection<LogEntry>();
        
        DataContext = this;
        
        // Set default selection
        LogLevelFilter.SelectedIndex = 0; // "All"
        
        // Subscribe to log service events
        _logService.LogAdded += OnLogAdded;
        _logService.PropertyChanged += OnLogServicePropertyChanged;
        
        // Subscribe to list box events for auto-scroll and copy functionality
        LogsListBox.MouseDoubleClick += LogsListBox_MouseDoubleClick;
        LogLevelFilter.SelectionChanged += LogLevelFilter_SelectionChanged;
        
        // Load existing logs
        RefreshFilteredLogs();
        
        Loaded += RuntimeLogsWindow_Loaded;
    }

    public ObservableCollection<LogEntry> FilteredLogEntries
    {
        get => _filteredLogEntries;
        private set
        {
            _filteredLogEntries = value;
            OnPropertyChanged(nameof(FilteredLogEntries));
        }
    }

    public string SelectedLogLevel
    {
        get => _selectedLogLevel;
        set
        {
            if (_selectedLogLevel != value)
            {
                _selectedLogLevel = value;
                OnPropertyChanged(nameof(SelectedLogLevel));
                RefreshFilteredLogs();
            }
        }
    }

    public bool AutoScrollEnabled
    {
        get => _autoScrollEnabled;
        set
        {
            if (_autoScrollEnabled != value)
            {
                _autoScrollEnabled = value;
                OnPropertyChanged(nameof(AutoScrollEnabled));
            }
        }
    }

    public int LogCount => FilteredLogEntries?.Count ?? 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RuntimeLogsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Scroll to bottom if auto-scroll is enabled and we have logs
        if (AutoScrollEnabled && LogsListBox.Items.Count > 0)
        {
            LogsListBox.ScrollIntoView(LogsListBox.Items[LogsListBox.Items.Count - 1]);
        }
    }

    private void OnLogAdded(object? sender, LogEntry logEntry)
    {
        // This event comes from a background thread, so we need to invoke on UI thread
        Dispatcher.BeginInvoke(() =>
        {
            if (ShouldIncludeLogEntry(logEntry))
            {
                FilteredLogEntries.Add(logEntry);
                OnPropertyChanged(nameof(LogCount));
                
                // Auto-scroll to bottom if enabled
                if (AutoScrollEnabled)
                {
                    LogsListBox.ScrollIntoView(logEntry);
                }
            }
        });
    }

    private void OnLogServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ILogService.LogEntries))
        {
            Dispatcher.BeginInvoke(() =>
            {
                RefreshFilteredLogs();
            });
        }
    }

    private void RefreshFilteredLogs()
    {
        var allLogs = _logService.LogEntries;
        var filteredLogs = allLogs.Where(ShouldIncludeLogEntry).ToList();
        
        FilteredLogEntries.Clear();
        foreach (var log in filteredLogs)
        {
            FilteredLogEntries.Add(log);
        }
        
        OnPropertyChanged(nameof(LogCount));
        
        // Auto-scroll to bottom if enabled
        if (AutoScrollEnabled && FilteredLogEntries.Count > 0)
        {
            Dispatcher.BeginInvoke(() =>
            {
                LogsListBox.ScrollIntoView(FilteredLogEntries.Last());
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private bool ShouldIncludeLogEntry(LogEntry logEntry)
    {
        if (SelectedLogLevel == "All")
            return true;
            
        return logEntry.Level.ToString() == SelectedLogLevel;
    }

    private void LogLevelFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LogLevelFilter.SelectedItem is ComboBoxItem selectedItem)
        {
            SelectedLogLevel = selectedItem.Tag?.ToString() ?? "All";
        }
    }
    
    private async void LogsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LogsListBox.SelectedItem is LogEntry selectedLog)
        {
            try
            {
                await _clipboardService.CopyToClipboardAsync(selectedLog.FormattedMessage);
                // Show a brief status message
                MessageBox.Show("Log entry copied to clipboard!", "Copied", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to clear all logs?", "Clear Logs", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            _logService.ClearLogs();
        }
    }
    
    private async void CopyAllButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FilteredLogEntries.Count == 0)
            {
                MessageBox.Show("No log entries to copy.", "Information", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Build a string containing all log entries
            var allLogs = string.Join(Environment.NewLine, 
                FilteredLogEntries.Select(log => log.FormattedMessage));

            await _clipboardService.CopyToClipboardAsync(allLogs);
            
            // Show a brief status message
            MessageBox.Show($"All {FilteredLogEntries.Count} log entries copied to clipboard!", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to copy logs to clipboard: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Unsubscribe from events
        _logService.LogAdded -= OnLogAdded;
        _logService.PropertyChanged -= OnLogServicePropertyChanged;
        
        base.OnClosing(e);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
