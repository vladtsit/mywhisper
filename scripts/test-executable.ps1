# Test the standalone executable
param(
    [string]$Runtime = "win-x64"
)

$executablePath = ".\publish\$Runtime\SpeechAgent.UI.exe"

Write-Host "Testing standalone executable..." -ForegroundColor Green
Write-Host "Executable path: $executablePath" -ForegroundColor Yellow

if (Test-Path $executablePath) {
    Write-Host "✓ Executable exists" -ForegroundColor Green
    
    # Get file info
    $fileInfo = Get-Item $executablePath
    Write-Host "File size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Yellow
    Write-Host "Created: $($fileInfo.CreationTime)" -ForegroundColor Yellow
    
    # Check if it's a valid PE file (basic check)
    $bytes = [System.IO.File]::ReadAllBytes($executablePath)
    if ($bytes[0] -eq 0x4D -and $bytes[1] -eq 0x5A) {
        Write-Host "✓ Valid executable format" -ForegroundColor Green
    } else {
        Write-Host "✗ Invalid executable format" -ForegroundColor Red
    }
    
    Write-Host "`nTo run the application:" -ForegroundColor Green
    Write-Host "  $executablePath" -ForegroundColor Cyan
    
} else {
    Write-Host "✗ Executable not found at $executablePath" -ForegroundColor Red
    Write-Host "Run the build script first: .\scripts\build.ps1" -ForegroundColor Yellow
}
