using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SpeechAgent.UI.ViewModels;
using SpeechAgent.Core.Services;

namespace SpeechAgent.UI.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;    /// <summary>
    /// Event raised when settings are successfully saved
    /// </summary>
    public event EventHandler? SettingsSaved;

    public SettingsWindow(ISettingsService settingsService)
    {
        InitializeComponent();
          _viewModel = new SettingsViewModel(settingsService);
        DataContext = _viewModel;
        
        // Subscribe to view model events
        _viewModel.CloseRequested += (s, e) => Close();
        _viewModel.SettingsSaved += (s, e) => 
        {
            // Settings were saved successfully - notify any external listeners
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        };

        // Handle password box binding (WPF doesn't support direct binding for security reasons)
        ApiKeyPasswordBox.Password = _viewModel.Key;
        ApiKeyPasswordBox.PasswordChanged += (s, e) => _viewModel.Key = ApiKeyPasswordBox.Password;
        
        // Update password box when view model key changes
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.Key) && ApiKeyPasswordBox.Password != _viewModel.Key)
            {
                ApiKeyPasswordBox.Password = _viewModel.Key;
            }
        };
    }
}

/// <summary>
/// Converter to invert boolean values for binding
/// </summary>
public class BooleanInverseConverter : IValueConverter
{
    public static BooleanInverseConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : true;
    }
}
