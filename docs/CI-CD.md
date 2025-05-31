# CI/CD Pipeline and GitHub Codespaces

This document explains how to use the CI/CD pipeline and GitHub Codespaces for Speech Agent development.

## CI/CD Pipeline Overview

The CI/CD pipeline is optimized for Windows development and automatically:
- **Tests** the code on Windows runners only (WPF applications require Windows)
- **Builds** standalone executables for Windows x64
- **Performs** code quality checks using Windows-compatible tools
- **Creates** releases when code is pushed to the main branch

### Pipeline Triggers

- **Push** to `main` or `develop` branches
- **Pull requests** to the `main` branch

### Jobs

1. **Test Job**: Runs on Windows only
   - Restores dependencies
   - Builds in Debug configuration
   - Runs all unit tests
   - Uploads test results as artifacts

2. **Build Standalone Job**: Creates production executables
   - Builds Windows x64 standalone executable
   - Single-file deployment with compression
   - Self-contained (no .NET runtime required)

3. **Code Quality Job**: Ensures code standards (Windows)
   - Runs `dotnet format` validation
   - Scans for vulnerable packages

4. **Create Release Job**: Publishes releases (main branch only)
   - Creates GitHub release with version tag
   - Uploads standalone executable as release asset

## GitHub Codespaces Support

The project includes a `.devcontainer` configuration optimized for Windows development:
- Provides .NET 8 development environment
- Installs necessary VS Code extensions for C# and WPF development
- Configures PowerShell as the default terminal
- Supports testing and debugging WPF applications
- Optimized for Windows Codespaces with WPF support

### Windows Codespaces Benefits
- **Native WPF Support**: Run and debug WPF applications directly
- **PowerShell Integration**: Full PowerShell support for build scripts
- **Windows-specific Tooling**: Access to Windows-only development tools
- **Performance**: Better performance for Windows-specific workloads

## Local Build Scripts

Use the PowerShell build script for local development:

```powershell
# Build for Windows (default)
.\scripts\build.ps1

# Build for specific platform
.\scripts\build.ps1 -Runtime linux-x64

# Build without tests
.\scripts\build.ps1 -Test:$false

# Debug build
.\scripts\build.ps1 -Configuration Debug
```

## Artifacts

- **Test Results**: Available for 30 days
- **Standalone Executables**: Available for 30 days
- **Releases**: Permanent storage on main branch

## Running in Codespaces

1. Open the repository in GitHub Codespaces (Windows)
2. The environment will automatically configure with Windows support
3. Run tests: `dotnet test`
4. Build application: `dotnet build`
5. Run build script: `.\scripts\build.ps1`
6. Run development script: `.\scripts\dev.ps1 -Build -Test`

### Codespaces Commands
```powershell
# Full development workflow
.\scripts\dev.ps1 -Install -Build -Test

# Build standalone executable
.\scripts\build.ps1

# Run tests only
dotnet test speech-agent.sln --verbosity normal
```

## Platform-Specific Notes

### Windows (Primary Platform)
- Builds as `.exe` executable
- Includes all WPF dependencies
- Uses win-x64 runtime
- Full WPF application support
- Native Windows development experience

### Cross-Platform Considerations
- **WPF Limitation**: WPF applications only run on Windows
- **Alternative Frameworks**: Consider Avalonia or .NET MAUI for cross-platform UI
- **Core Logic**: Business logic in `SpeechAgent.Core` is cross-platform compatible
- **Testing**: Unit tests can run on any platform supporting .NET 8

## Troubleshooting

### Build Failures
- Check .NET version compatibility
- Verify all NuGet packages restore correctly
- Review test output for failures

### Codespaces Issues
- Ensure devcontainer rebuilds if configuration changes
- Verify Windows Codespace allocation for WPF support
- Check that PowerShell is set as default terminal
- Verify extensions are properly installed for Windows development
