# Test script to verify transcription file saving logic
Write-Host "Testing transcription file saving logic..."

# Simulate the directory resolution logic from SaveTranscriptionFilesAsync
$documentsPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::MyDocuments)
Write-Host "Documents path: $documentsPath"

$speechAgentPath = Join-Path $documentsPath "SpeechAgent"
$recordingsPath = Join-Path $speechAgentPath "Recordings"

Write-Host "Expected SpeechAgent path: $speechAgentPath"
Write-Host "Expected Recordings path: $recordingsPath"

# Test directory creation
try {
    if (-not (Test-Path $speechAgentPath)) {
        Write-Host "Creating SpeechAgent directory..."
        New-Item -ItemType Directory -Path $speechAgentPath -Force | Out-Null
    }
    
    if (-not (Test-Path $recordingsPath)) {
        Write-Host "Creating Recordings directory..."
        New-Item -ItemType Directory -Path $recordingsPath -Force | Out-Null
    }
    
    # Test file creation with timestamp
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $rawFileName = "recording_${timestamp}_raw.txt"
    $correctedFileName = "recording_${timestamp}_corrected.txt"
    
    $rawFilePath = Join-Path $recordingsPath $rawFileName
    $correctedFilePath = Join-Path $recordingsPath $correctedFileName
    
    # Create test files
    $testRawContent = "This is a test raw transcription from Whisper."
    $testCorrectedContent = "This is a test corrected transcription from GPT."
    
    [System.IO.File]::WriteAllText($rawFilePath, $testRawContent, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($correctedFilePath, $testCorrectedContent, [System.Text.Encoding]::UTF8)
    
    Write-Host "SUCCESS: Test files created:"
    Write-Host "  Raw file: $rawFilePath"
    Write-Host "  Corrected file: $correctedFilePath"
    
    # Verify files exist and show content
    if ((Test-Path $rawFilePath) -and (Test-Path $correctedFilePath)) {
        Write-Host "`nFiles verified to exist. Contents:"
        Write-Host "Raw file content: $(Get-Content $rawFilePath)"
        Write-Host "Corrected file content: $(Get-Content $correctedFilePath)"
        
        # Show file details
        $rawFile = Get-Item $rawFilePath
        $correctedFile = Get-Item $correctedFilePath
        Write-Host "`nFile details:"
        Write-Host "Raw file: $($rawFile.Length) bytes, created $($rawFile.CreationTime)"
        Write-Host "Corrected file: $($correctedFile.Length) bytes, created $($correctedFile.CreationTime)"
    } else {
        Write-Host "ERROR: Files were not created successfully"
    }
    
} catch {
    Write-Host "ERROR during testing: $($_.Exception.Message)"
}

Write-Host "`nTest completed."
