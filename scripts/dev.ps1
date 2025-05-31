# GitHub Codespaces Development Script
# This script sets up and runs the Speech Agent in development mode

param(
    [switch]$Install,
    [switch]$Build,
    [switch]$Test,
    [switch]$Run,
    [switch]$Clean,
    [string]$Configuration = "Debug"
)

Write-Host "Speech Agent Development Script for GitHub Codespaces" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Function to check if we're in Codespaces
function Test-IsCodespaces {
    return $env:CODESPACES -eq "true" -or $env:GITHUB_CODESPACES_TOKEN -ne $null
}

# Clean previous builds
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "./bin") { Remove-Item -Path "./bin" -Recurse -Force }
    if (Test-Path "./obj") { Remove-Item -Path "./obj" -Recurse -Force }
    if (Test-Path "./publish") { Remove-Item -Path "./publish" -Recurse -Force }
    
    # Clean all project bin/obj folders
    Get-ChildItem -Path . -Recurse -Directory -Name "bin", "obj" | ForEach-Object {
        Remove-Item -Path $_ -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "Clean completed!" -ForegroundColor Green
}

# Install/Restore dependencies
if ($Install -or $Build -or $Test -or $Run) {
    Write-Host "Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore speech-agent.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to restore dependencies"
        exit 1
    }
}

# Build the application
if ($Build -or $Test -or $Run) {
    Write-Host "Building application..." -ForegroundColor Yellow
    dotnet build speech-agent.sln --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build application"
        exit 1
    }
}

# Run tests
if ($Test) {
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test speech-agent.sln --configuration $Configuration --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed"
        exit 1
    }
}

# Run the application (only works on Windows with GUI)
if ($Run) {
    if (Test-IsCodespaces) {
        Write-Warning "Cannot run WPF application in GitHub Codespaces (no GUI support)"
        Write-Host "Consider using:" -ForegroundColor Yellow
        Write-Host "  - GitHub Desktop for local development" -ForegroundColor Cyan
        Write-Host "  - VS Code with Remote-Containers extension" -ForegroundColor Cyan
        Write-Host "  - Building a console version for testing" -ForegroundColor Cyan
    } elseif ($IsLinux -or $IsMacOS) {
        Write-Warning "WPF applications only run on Windows"
        Write-Host "Building console test version instead..." -ForegroundColor Yellow
        # Could add console runner here for basic functionality testing
    } else {
        Write-Host "Starting Speech Agent UI..." -ForegroundColor Yellow
        dotnet run --project "src\SpeechAgent.UI\SpeechAgent.UI.csproj" --configuration $Configuration
    }
}

# Show help if no parameters provided
if (-not ($Install -or $Build -or $Test -or $Run -or $Clean)) {
    Write-Host ""
    Write-Host "Usage Examples:" -ForegroundColor Cyan
    Write-Host "  .\scripts\dev.ps1 -Install          # Restore dependencies" -ForegroundColor White
    Write-Host "  .\scripts\dev.ps1 -Build            # Build the solution" -ForegroundColor White
    Write-Host "  .\scripts\dev.ps1 -Test             # Run tests" -ForegroundColor White
    Write-Host "  .\scripts\dev.ps1 -Run              # Run the application" -ForegroundColor White
    Write-Host "  .\scripts\dev.ps1 -Clean            # Clean build artifacts" -ForegroundColor White
    Write-Host "  .\scripts\dev.ps1 -Build -Test      # Build and test" -ForegroundColor White
    Write-Host ""
    if (Test-IsCodespaces) {
        Write-Host "GitHub Codespaces Detected!" -ForegroundColor Green
        Write-Host "Recommended workflow:" -ForegroundColor Yellow
        Write-Host "  1. .\scripts\dev.ps1 -Install -Build -Test" -ForegroundColor Cyan
        Write-Host "  2. Use VS Code for development" -ForegroundColor Cyan
        Write-Host "  3. Push changes to trigger CI/CD" -ForegroundColor Cyan
    }
}

Write-Host "Development script completed!" -ForegroundColor Green
