param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = "./publish",
    [switch]$SelfContained,
    [switch]$SingleFile,
    [switch]$Test,
    [switch]$SkipCodespaceWarning
)

# Set default values for switches
if (-not $PSBoundParameters.ContainsKey('SelfContained')) { $SelfContained = $true }
if (-not $PSBoundParameters.ContainsKey('SingleFile')) { $SingleFile = $true }
if (-not $PSBoundParameters.ContainsKey('Test')) { $Test = $true }

# Detect if running in Codespaces
function Test-IsCodespaces {
    return ($env:CODESPACES -eq "true") -or ($null -ne $env:GITHUB_CODESPACE_TOKEN)
}

Write-Host "Building Speech Agent for Windows..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Runtime: $Runtime" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow

if (Test-IsCodespaces -and -not $SkipCodespaceWarning) {
    Write-Host "GitHub Codespaces detected!" -ForegroundColor Cyan
    Write-Host "Optimizing build for Windows Codespaces environment..." -ForegroundColor Yellow
}

# Clean previous builds
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore speech-agent.sln
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to restore dependencies"
    exit 1
}

# Run tests if requested
if ($Test) {
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test speech-agent.sln --configuration $Configuration --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed"
        exit 1
    }
}

# Build the application
Write-Host "Building application..." -ForegroundColor Yellow
$publishArgs = @(
    "publish"
    "src/SpeechAgent.UI/SpeechAgent.UI.csproj"
    "--configuration", $Configuration
    "--runtime", $Runtime
    "--output", "$OutputPath/$Runtime"
)

if ($SelfContained) {
    $publishArgs += "--self-contained", "true"
}

if ($SingleFile) {
    $publishArgs += "/p:PublishSingleFile=true"
    $publishArgs += "/p:PublishReadyToRun=true"
    $publishArgs += "/p:IncludeNativeLibrariesForSelfExtract=true"
    $publishArgs += "/p:EnableCompressionInSingleFile=true"
    $publishArgs += "/p:DebugType=embedded"
}

dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build application"
    exit 1
}

Write-Host "Windows build completed successfully!" -ForegroundColor Green
Write-Host "Output location: $OutputPath/$Runtime" -ForegroundColor Green

if (Test-IsCodespaces) {
    Write-Host "Build optimized for Windows Codespaces environment" -ForegroundColor Cyan
}

# List the generated files
if (Test-Path "$OutputPath/$Runtime") {
    Write-Host "Generated files:" -ForegroundColor Yellow
    Get-ChildItem -Path "$OutputPath/$Runtime" | Format-Table Name, Length, LastWriteTime
    
    if (Test-IsCodespaces) {
        Write-Host "Note: WPF applications built in Codespaces are ready for Windows deployment" -ForegroundColor Cyan
    }
}
