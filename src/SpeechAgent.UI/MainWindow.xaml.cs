using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SpeechAgent.Core.Services;
using SpeechAgent.UI.Helpers;
using SpeechAgent.UI.Views;
using System.Collections.Generic;

namespace SpeechAgent.UI
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IAudioRecorder _audioRecorder;
        private readonly ITranscriptionService _transcriptionService;
        private readonly ICorrectionService _correctionService;
        private readonly IClipboardService _clipboardService;
        private readonly ISettingsService _settingsService;
        private readonly ILogService _logService; private readonly IServiceProvider _serviceProvider;
        private MemoryStream? _recordedAudio;
        private string _currentText = string.Empty;
        private string _lastRecordingTimestamp = string.Empty; private string _audioSourceName = string.Empty;
        public MainWindow(
            IAudioRecorder audioRecorder,
            ITranscriptionService transcriptionService,
            ICorrectionService correctionService,
            IClipboardService clipboardService,
            ISettingsService settingsService,
            ILogService logService,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _audioRecorder = audioRecorder;
            _transcriptionService = transcriptionService;
            _correctionService = correctionService;
            _clipboardService = clipboardService;
            _settingsService = settingsService;
            _logService = logService;
            _serviceProvider = serviceProvider;

            _audioRecorder.RecordingStateChanged += OnRecordingStateChanged;

            _logService.LogInfo("MainWindow initialized", "MainWindow");
        }
        private void OnRecordingStateChanged(object? sender, bool isRecording)
        {
            Dispatcher.Invoke(() =>
            {
                RecordToggleButton.IsChecked = isRecording;
                CopyButton.IsEnabled = false;
                StatusText.Text = isRecording
                    ? "Recording... Click the microphone to stop."
                    : "Recording stopped. Processing...";
            });
        }        private async void RecordToggleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RecordToggleButton.IsChecked == true)
                {
                    // Validate settings before starting recording
                    if (!ValidateSettingsForOperation())
                    {
                        RecordToggleButton.IsChecked = false;
                        return;
                    }

                    // Start recording
                    await _audioRecorder.StartRecordingAsync();
                }
                else
                {
                    // Stop recording
                    _logService.LogDebug("Stop recording started");

                    _recordedAudio = await _audioRecorder.StopRecordingAsync();

                    _logService.LogDebug($"StopRecordingAsync returned. Stream is null: {_recordedAudio == null}");
                    if (_recordedAudio != null)
                    {
                        _logService.LogInfo($"Recording stopped successfully - Stream length: {_recordedAudio.Length} bytes");
                        _logService.LogDebug($"Stream details - Position: {_recordedAudio.Position}, CanRead: {_recordedAudio.CanRead}, CanSeek: {_recordedAudio.CanSeek}, CanWrite: {_recordedAudio.CanWrite}");

                        // Check if stream is disposed
                        try
                        {
                            var testPosition = _recordedAudio.Position;
                            _logService.LogDebug($"Stream access test successful, position: {testPosition}");
                        }
                        catch (Exception streamEx)
                        {
                            _logService.LogError($"Stream access test failed: {streamEx.Message}");
                        }
                        // Reset position to beginning for future reading
                        if (_recordedAudio.CanSeek)
                        {
                            try
                            {
                                _recordedAudio.Position = 0;
                                _logService.LogDebug("Reset stream position to 0");
                            }
                            catch (Exception seekEx)
                            {
                                _logService.LogError($"Error resetting stream position: {seekEx.Message}");
                            }
                        }

                        // Save recording to file
                        _logService.LogDebug("About to call SaveRecordingToFileAsync...");
                        await SaveRecordingToFileAsync(_recordedAudio);
                        _logService.LogDebug("SaveRecordingToFileAsync completed");

                        // Update UI state now that we have recorded audio
                        Dispatcher.Invoke(() =>
                        {
                            StatusText.Text = "Recording saved. Processing...";
                            _logService.LogDebug("UI updated - starting processing");
                        });

                        // Automatically process the recorded audio
                        await ProcessRecordedAudioAsync();

                        _logService.LogDebug("Stop recording completed");
                    }
                    else
                    {
                        _logService.LogWarning("Recording returned null stream");
                        Dispatcher.Invoke(() =>
                        {
                            StatusText.Text = "Recording failed. Please try again.";
                        });
                    }

                    _logService.LogDebug("Stop recording completed");
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error during recording operation: {ex.Message}", "MainWindow", ex);

                MessageBox.Show($"Failed to handle recording: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);

                // Reset button state on error
                RecordToggleButton.IsChecked = false;
            }
        }

        /// <summary>
        /// Saves raw and corrected transcription text files next to the audio recording
        /// </summary>
        private async Task SaveTranscriptionFilesAsync(string rawTranscription, string correctedTranscription, string timestamp)
        {
            try
            {
                _logService.LogDebug("Save transcription files started");
                if (string.IsNullOrWhiteSpace(rawTranscription) && string.IsNullOrWhiteSpace(correctedTranscription))
                {
                    _logService.LogWarning("No transcription to save");
                    return;
                }

                var recordingsDir = FileStorageHelper.EnsureRecordingsDirectory();

                var rawFilename = $"transcript_{timestamp}.txt";
                await FileStorageHelper.SaveTextToFileAsync(recordingsDir, rawFilename, rawTranscription ?? string.Empty);
                _logService.LogInfo($"Raw transcription saved to: {rawFilename}");

                if (!string.IsNullOrWhiteSpace(correctedTranscription))
                {
                    var corrFilename = $"transcript_corrected_{timestamp}.txt";
                    await FileStorageHelper.SaveTextToFileAsync(recordingsDir, corrFilename, correctedTranscription);
                    _logService.LogInfo($"Corrected transcription saved to: {corrFilename}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error saving transcription files: {ex.Message}", "MainWindow", ex);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settingsService)
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
        }

        private void ViewLogsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logsWindow = _serviceProvider.GetRequiredService<RuntimeLogsWindow>();
                logsWindow.Owner = this;
                logsWindow.Show();
                _logService.LogInfo("Runtime logs window opened", "MainWindow");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to open runtime logs window: {ex.Message}", "MainWindow", ex);
                MessageBox.Show($"Failed to open logs window: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _audioRecorder.RecordingStateChanged -= OnRecordingStateChanged;
            if (_audioRecorder is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _recordedAudio?.Dispose();
            base.OnClosed(e);
        }        /// <summary>
        /// Transcribes large audio by splitting into 500-second chunks and combining results.
        /// </summary>
        /// <returns>A tuple containing the raw combined transcript and the corrected combined transcript</returns>
        private async Task<(string RawTranscript, string CorrectedTranscript)> TranscribeWithChunksAsync(Stream audioStream)
        {
            // recordings folder for final outputs
            var baseDir = FileStorageHelper.EnsureRecordingsDirectory();
            var tempDir = Path.Combine(Path.GetTempPath(), "SpeechAgentChunks_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            var sourceName = !string.IsNullOrWhiteSpace(_audioSourceName)
                ? _audioSourceName
                : DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // write input wav to temp folder
            var inputFile = Path.Combine(tempDir, $"input_{sourceName}.wav");
            await using (var fs = new FileStream(inputFile, FileMode.Create, FileAccess.Write))
            {
                if (audioStream.CanSeek) audioStream.Position = 0;
                await audioStream.CopyToAsync(fs);
            }
            // also save full extracted audio to recordings
            var extractedName = $"{sourceName}_extracted.wav";
            var extractedBytes = await File.ReadAllBytesAsync(inputFile);
            await FileStorageHelper.SaveBytesToFileAsync(baseDir, extractedName, extractedBytes);
            _logService.LogInfo($"Extracted audio saved to recordings: {extractedName}");
            // split into chunks
            Dispatcher.Invoke(() => StatusText.Text = "Splitting audio into chunks...");
            var pattern = Path.Combine(tempDir, $"{sourceName}_%03d.wav");
            var psi = new ProcessStartInfo("ffmpeg",
                $"-i \"{inputFile}\" -f segment -segment_time 500 -c copy -start_number 1 \"{pattern}\" -y")
            { RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            using var proc = Process.Start(psi);
            var err = await proc!.StandardError.ReadToEndAsync(); proc.WaitForExit();
            if (proc.ExitCode != 0) throw new Exception("Audio split failed: " + err);
            // transcribe each segment (fallback to full file if none)
            var segmentFiles = Directory.GetFiles(tempDir, $"{sourceName}_*.wav");
            if (segmentFiles.Length == 0)
            {
                segmentFiles = new[] { inputFile };
            }

            var rawChunks = new List<string>();
            var correctedChunks = new List<string>();
            foreach (var segFile in segmentFiles)
            {
                var fileInfo = new FileInfo(segFile);
                if (fileInfo.Length < 3200)
                {
                    _logService.LogDebug($"Skipping tiny segment '{fileInfo.Name}' ({fileInfo.Length} bytes)");
                    continue;
                }
                var fileNameOnly = Path.GetFileNameWithoutExtension(segFile);
                Dispatcher.Invoke(() => StatusText.Text = $"Transcribing chunk {fileNameOnly}...");
                var bytes = await File.ReadAllBytesAsync(segFile);
                await FileStorageHelper.SaveBytesToFileAsync(baseDir, fileInfo.Name, bytes);
                using var msSeg = new MemoryStream(bytes);
                var chunkText = await _transcriptionService.TranscribeAsync(msSeg);
                await FileStorageHelper.SaveTextToFileAsync(baseDir, $"{fileNameOnly}.txt", chunkText);
                var correctedChunk = await _correctionService.CorrectAsync(chunkText);
                await FileStorageHelper.SaveTextToFileAsync(baseDir, $"{fileNameOnly}_corrected.txt", correctedChunk);
                rawChunks.Add(chunkText);
                correctedChunks.Add(correctedChunk);
            }

            Dispatcher.Invoke(() => StatusText.Text = "Combining transcripts...");
            // cleanup
            Directory.Delete(tempDir, true);            var combinedRaw = string.Join(Environment.NewLine, rawChunks.Where(t => !string.IsNullOrWhiteSpace(t)));
            var combinedCorrected = string.Join(Environment.NewLine, correctedChunks.Where(t => !string.IsNullOrWhiteSpace(t)));
            await FileStorageHelper.SaveTextToFileAsync(baseDir, $"{sourceName}_combined.txt", combinedRaw);
            await FileStorageHelper.SaveTextToFileAsync(baseDir, $"{sourceName}_combined_corrected.txt", combinedCorrected);

            return (combinedRaw, combinedCorrected);
        }        /// <summary>
        /// Automatically transcribes and corrects recorded audio.
        /// </summary>
        private async Task ProcessRecordedAudioAsync()
        {
            if (_recordedAudio == null) return;

            // Skip very short audio (<0.1s at 16kHz, ~3200 bytes)
            if (_recordedAudio.Length < 3200)
            {
                MessageBox.Show("Audio too short to transcribe. Please record or upload a longer clip.",
                                "Audio Too Short", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ShowProgress("Transcribing audio...");
                if (_recordedAudio.CanSeek) _recordedAudio.Position = 0;
                
                // TranscribeWithChunksAsync now returns a tuple with both raw and corrected text
                var (transcript, corrected) = await TranscribeWithChunksAsync(_recordedAudio);
                if (string.IsNullOrWhiteSpace(transcript))
                {
                    StatusText.Text = "No speech detected in the recording.";
                    HideProgress();
                    return;
                }

                RawTextBox.Text = transcript;
                _currentText = corrected;
                CorrectedTextBox.Text = corrected;
                CopyButton.IsEnabled = true;

                if (!string.IsNullOrWhiteSpace(_lastRecordingTimestamp))
                    await SaveTranscriptionFilesAsync(transcript, corrected, _lastRecordingTimestamp);

                StatusText.Text = "Processing complete! Text is ready to copy.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to process audio: {ex.Message}", "Processing Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Processing failed. Please try again.";
            }
            finally
            {
                HideProgress();
            }
        }

        private void ShowProgress(string message)
        {
            StatusText.Text = message;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
        }

        private void HideProgress()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
        }
        private async Task SaveRecordingToFileAsync(MemoryStream audioStream)
        {
            try
            {
                _logService.LogDebug("Save recording started");
                if (audioStream == null || audioStream.Length == 0)
                {
                    _logService.LogError(audioStream == null ? "audioStream is null" : "audioStream is empty (0 bytes)");
                    return;
                }

                var recordingsDir = FileStorageHelper.EnsureRecordingsDirectory();
                _logService.LogDebug($"Recordings directory set to: {recordingsDir}");

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"recording_{timestamp}.wav";
                _lastRecordingTimestamp = timestamp;
                _audioSourceName = $"recording_{timestamp}";

                if (audioStream.CanSeek)
                    audioStream.Position = 0;
                var data = audioStream.ToArray();
                await FileStorageHelper.SaveBytesToFileAsync(recordingsDir, filename, data);
                _logService.LogInfo($"Recording saved to: {filename} ({data.Length} bytes)");

                if (audioStream.CanSeek)
                    audioStream.Position = 0;
                Dispatcher.Invoke(() =>
                    StatusText.Text = $"Recording saved to: {filename}."
                );
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error saving recording: {ex.Message}", "MainWindow", ex);
                MessageBox.Show($"Warning: Failed to save recording: {ex.Message}\nYou can still transcribe the audio.",
                               "Save Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Re-add Copy button click handler for CopyButton
        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentText))
            {
                MessageBox.Show("No text to copy. Please process audio first.", "No Text",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await _clipboardService.CopyToClipboardAsync(_currentText);
                StatusText.Text = "Text copied to clipboard!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Copy Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add file upload handler
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Media Files (*.wav;*.mp3;*.mp4;*.mov;*.avi;*.m4a)|*.wav;*.mp3;*.mp4;*.mov;*.avi;*.m4a|All Files (*.*)|*.*",
                Title = "Select media file"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // Backup original media file
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var recordingsDir = FileStorageHelper.EnsureRecordingsDirectory();
                    var originalName = Path.GetFileName(dlg.FileName);
                    var baseName = Path.GetFileNameWithoutExtension(originalName);
                    _audioSourceName = baseName;
                    var originalBytes = await File.ReadAllBytesAsync(dlg.FileName);
                    var backupName = $"uploaded_{timestamp}_{originalName}";
                    await FileStorageHelper.SaveBytesToFileAsync(recordingsDir, backupName, originalBytes);
                    _logService.LogInfo($"Original media backed up: {backupName}");
                    _lastRecordingTimestamp = timestamp;

                    // Prepare audio stream for Whisper
                    MemoryStream audioStream;
                    var ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
                    if (ext == ".wav" || ext == ".mp3")
                    {
                        audioStream = new MemoryStream(originalBytes);
                    }
                    else
                    {
                        // Extract audio using FFmpeg
                        var tempWav = Path.Combine(Path.GetTempPath(), $"extracted_{timestamp}.wav");
                        var ffInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = $"-i \"{dlg.FileName}\" -ar 16000 -ac 1 -f wav \"{tempWav}\" -y",
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using var proc = Process.Start(ffInfo);
                        var error = await proc!.StandardError.ReadToEndAsync();
                        proc.WaitForExit();
                        if (proc.ExitCode != 0)
                            throw new Exception("Audio extraction failed: " + error);
                        var wavBytes = await File.ReadAllBytesAsync(tempWav);
                        audioStream = new MemoryStream(wavBytes);
                        try { File.Delete(tempWav); } catch { }
                    }

                    // Process the audio
                    _recordedAudio = audioStream;
                    Dispatcher.Invoke(() => StatusText.Text = $"Loaded file: {originalName}. Processing...");
                    await ProcessRecordedAudioAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load media: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Validates that settings are properly configured for operations
        /// </summary>
        private bool ValidateSettingsForOperation()
        {
            if (!_settingsService.AreSettingsValid())
            {
                var result = MessageBox.Show(
                    "Azure OpenAI settings are not properly configured.\n\n" +
                    "Would you like to open the settings window to configure them now?",
                    "Settings Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var settingsWindow = new SettingsWindow(_settingsService)
                    {
                        Owner = this
                    };
                    settingsWindow.ShowDialog();
                    
                    // Re-check settings after dialog
                    return _settingsService.AreSettingsValid();
                }
                
                return false;
            }
            
            return true;
        }
    }
} // namespace SpeechAgent.UI