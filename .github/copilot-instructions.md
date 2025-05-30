# GitHub Copilot Instructions for .NET C# WPF App

- Prefer MVVM (Model-View-ViewModel) pattern for structuring code.
- Use ObservableCollection for data binding in lists.
- Implement INotifyPropertyChanged in ViewModels for property change notifications.
- Use async/await for long-running or I/O-bound operations.
- Prefer ICommand implementations (like RelayCommand) for button and menu actions.
- Use Dependency Injection for services and data access.
- Follow .NET naming conventions: PascalCase for classes/properties, camelCase for fields/parameters.
- Keep code-behind files minimal; place logic in ViewModels.
- Use XAML for UI layout and styling; avoid hardcoding UI in code-behind.
- Add XML documentation comments to public classes and methods.
- Write unit tests for ViewModels and business logic.
- use powershell in console to run commands like `dotnet build`, `dotnet test`, etc.
- use dotnet run --project "src\SpeechAgent.UI\SpeechAgent.UI.csproj" --verbosity normal to build and run the application.
- use cd dotnet build to build the project.