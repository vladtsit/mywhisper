# CI/CD Pipeline and GitHub Codespaces

This document explains how to use the CI/CD pipeline and GitHub Codespaces for Speech Agent development.

## CI/CD Pipeline Overview

The CI/CD pipeline automatically:
- **Tests** the code on both Ubuntu and Windows
- **Builds** standalone executables for Windows
- **Performs** code quality checks
- **Creates** releases when code is pushed to the main branch

### Pipeline Triggers

- **Push** to `main` or `develop` branches
- **Pull requests** to the `main` branch

### Jobs

1. **Test Job**: Runs on Ubuntu and Windows
   - Restores dependencies
   - Builds in Debug configuration
   - Runs all unit tests
   - Uploads test results as artifacts

2. **Build Standalone Job**: Creates production executables
   - Builds Windows x64 standalone executable (WPF only runs on Windows)
   - Single-file deployment with compression
   - Self-contained (no .NET runtime required)

3. **Code Quality Job**: Ensures code standards
   - Runs `dotnet format` validation
   - Scans for vulnerable packages

4. **Create Release Job**: Publishes releases (main branch only)
   - Creates GitHub release with version tag
   - Uploads standalone executable as release asset

## GitHub Codespaces Support

The project includes a `.devcontainer` configuration that:
- Provides .NET 8 development environment
- Installs necessary VS Code extensions
- Configures C# development tools
- Supports testing and debugging

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

1. Open the repository in GitHub Codespaces
2. The environment will automatically configure
3. Run tests: `dotnet test`
4. Build application: `dotnet build`
5. Run application: `dotnet run --project src/SpeechAgent.UI/SpeechAgent.UI.csproj`

## Platform-Specific Notes

### Windows
- Builds as `.exe` executable
- Includes all WPF dependencies
- Uses win-x64 runtime

### Linux
- Requires X11 for GUI (limited support for WPF)
- May need additional runtime libraries
- Consider using Windows Subsystem for Linux (WSL)

### macOS
- WPF has limited support on macOS
- Consider alternative UI frameworks for cross-platform support

## Troubleshooting

### Build Failures
- Check .NET version compatibility
- Verify all NuGet packages restore correctly
- Review test output for failures

### Codespaces Issues
- Ensure devcontainer rebuilds if configuration changes
- Check port forwarding for web applications
- Verify extensions are properly installed
