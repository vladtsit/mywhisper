param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = "./publish",
    [switch]$SelfContained = $true,
    [switch]$SingleFile = $true,
    [switch]$Test = $true
)

Write-Host "Building Speech Agent..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Runtime: $Runtime" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow

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

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Output location: $OutputPath/$Runtime" -ForegroundColor Green

# List the generated files
if (Test-Path "$OutputPath/$Runtime") {
    Write-Host "Generated files:" -ForegroundColor Yellow
    Get-ChildItem -Path "$OutputPath/$Runtime" | Format-Table Name, Length, LastWriteTime
}
