# Speech-to-Text + Correction App

A comprehensive Windows desktop application that processes audio through live microphone recording or file uploads, transcribes speech using Azure OpenAI Whisper, corrects text using GPT-4, and provides advanced file management and logging capabilities.

## Features

### Core Audio Processing
- 🎤 **Live Audio Recording**: Capture high-quality audio from your microphone with toggle recording
- 📁 **File Upload Support**: Process existing audio/video files (WAV, MP3, MP4, MOV, AVI, M4A)
- ✂️ **Smart Audio Chunking**: Automatically split large files into 500-second segments using FFmpeg
- 🗣️ **Speech Transcription**: Convert speech to text using Azure OpenAI Whisper
- ✨ **Text Correction**: Improve grammar and clarity using GPT-4 with customizable prompts

### User Interface & Experience
- 📋 **Clipboard Integration**: Automatically copy corrected text to clipboard
- 📊 **Progress Tracking**: Real-time progress indicators for transcription and correction
- 📝 **Dual Text Display**: View both raw transcription and corrected text side-by-side
- 🎨 **Modern WPF UI**: Clean and intuitive interface with responsive design

### Advanced Features
- ⚙️ **Comprehensive Settings**: Secure settings management with encrypted local storage
- 📜 **Runtime Logging**: Full logging system with filtering, search, and export capabilities
- 💾 **File Management**: Automatic organization of recordings, transcripts, and processed files
- 🔒 **Security**: Encrypted storage for sensitive configuration data
- 🎯 **Customizable Prompts**: Personalize text correction behavior through settings

## Prerequisites

- Windows 10/11
- .NET 8.0 Runtime
- **FFmpeg**: Required for audio/video file processing and chunking
- Azure OpenAI Service with Whisper and GPT-4 deployments
- Microphone access

## Setup

### 1. Install FFmpeg

FFmpeg is required for processing audio and video files. Choose one of these installation methods:

**Option A: Using winget (Recommended)**
```powershell
winget install ffmpeg
```

**Option B: Using Chocolatey**
```powershell
choco install ffmpeg
```

**Option C: Manual Installation**
1. Download FFmpeg from [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)
2. Extract to a folder (e.g., `C:\ffmpeg`)
3. Add the `bin` folder to your system PATH environment variable

**Verify Installation**
```powershell
ffmpeg -version
```

### 2. Azure OpenAI Configuration

1. Create an Azure OpenAI resource in the Azure portal
2. Deploy the following models:
   - **Whisper**: Create a deployment named `whisper`
   - **GPT-4o**: Create a deployment named `gpt-4o`
3. Note down your endpoint URL and API key

### 3. Application Configuration

1. Clone this repository
2. Launch the application and click the Settings button
3. Configure your Azure OpenAI details in the Settings window:
   - **Endpoint**: Your Azure OpenAI resource endpoint
   - **API Key**: Your Azure OpenAI API key
   - **Whisper Deployment**: Name of your Whisper deployment
   - **GPT Deployment**: Name of your GPT-4 deployment
   - **Correction Prompt**: Customize the text correction behavior

*Note: Settings are encrypted and stored securely on your local machine*

### 4. Build and Run

```powershell
# Build the solution
dotnet build

# Run the application
dotnet run --project "src\SpeechAgent.UI\SpeechAgent.UI.csproj" --verbosity normal
```

## How It Works

### Recording Workflow
1. **Choose Input Method**: 
   - Toggle the microphone button to start/stop live recording
   - Or use the "Browse" button to upload an existing audio/video file

2. **File Processing**:
   - Large files are automatically chunked into manageable segments
   - Audio is processed through FFmpeg for optimal quality
   - Multiple file formats supported (WAV, MP3, MP4, MOV, AVI, M4A)

3. **Transcription**:
   - Audio is sent to Azure OpenAI Whisper for speech-to-text conversion
   - Progress is displayed in real-time

4. **Text Correction**:
   - Raw transcription is processed through GPT-4 for grammar and clarity
   - Customizable correction prompts via Settings
   - Both original and corrected text are displayed

5. **Output**:
   - Corrected text is automatically copied to clipboard
   - All files and transcripts are saved locally for future reference

### Settings Management
- Access comprehensive settings through the Settings window
- Configure Azure OpenAI credentials and endpoints
- Customize text correction prompts
- Manage file storage locations
- All sensitive data is encrypted and stored securely

### Logging & Monitoring
- Built-in runtime logging system tracks all operations
- Filter logs by level, timestamp, or content
- Export logs for troubleshooting or analysis
- Monitor application performance and errors

## Usage

### Live Recording
1. **Start Recording**: Click the "🎤" toggle button to begin capturing audio from your microphone
2. **Stop Recording**: Click the "🎤" button again to stop recording
3. **Process**: Audio is automatically transcribed and corrected
4. **View Results**: See both raw and corrected text in the interface
5. **Copy**: Corrected text is automatically copied to your clipboard

### File Upload
1. **Browse Files**: Click the "Browse" button to select an audio or video file
2. **Auto-Processing**: Large files are automatically chunked for processing
3. **Progress Tracking**: Monitor transcription and correction progress
4. **Results**: View transcribed and corrected text in the dual display
5. **Access Settings**: Use the Settings button to configure API keys and correction prompts
6. **View Logs**: Use the Runtime Logs button to monitor application activity

## Architecture

```
speech-agent/
├── src/
│   ├── SpeechAgent.UI/           # WPF desktop application
│   └── SpeechAgent.Core/         # Core services and interfaces
│       ├── Services/             # Business logic services
│       └── Models/               # Data models
├── tests/
│   └── SpeechAgent.Core.Tests/   # Unit tests
└── .github/workflows/            # CI/CD pipeline
```

### Key Components

- **AudioRecorder**: Captures microphone input using NAudio
- **FileUploadService**: Handles file selection and validation for multiple formats
- **AudioChunkingService**: Splits large audio files using FFmpeg for optimal processing
- **WhisperService**: Transcribes audio using Azure OpenAI Whisper
- **OpenAiCorrectionService**: Improves text using GPT-4 with customizable prompts
- **ClipboardService**: Manages clipboard operations
- **SettingsService**: Manages encrypted application configuration
- **RuntimeLoggingService**: Provides comprehensive logging and monitoring
- **FileStorageService**: Organizes and manages recorded files and transcripts

## Technologies Used

- **.NET 8.0**: Cross-platform framework
- **WPF**: Windows Presentation Foundation for UI with MVVM pattern
- **Azure OpenAI**: AI-powered speech and text processing (Whisper + GPT-4)
- **NAudio**: Audio capture and processing
- **FFmpeg**: Audio/video processing and chunking
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Data Protection**: Microsoft.AspNetCore.DataProtection for encrypted settings
- **NUnit & Moq**: Unit testing framework and mocking
- **Microsoft.Extensions.Logging**: Comprehensive logging infrastructure

## Development

### Running Tests

```bash
dotnet test
```

### Building for Release

```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Security

### Data Protection
- **Encrypted Settings**: All sensitive configuration data (API keys, endpoints) is encrypted using Microsoft Data Protection APIs
- **Local Storage**: Settings and logs are stored locally on your machine, never transmitted to third parties
- **Secure Communication**: All Azure OpenAI API calls use HTTPS encryption
- **No Data Retention**: Audio files and transcripts are processed and stored locally only

### Best Practices
- Never commit your Azure OpenAI API keys to version control
- Use environment variables or Azure Key Vault for production deployments
- Ensure your Azure OpenAI resource has appropriate access controls and networking restrictions
- Regularly rotate your API keys according to your organization's security policy
- Monitor usage through Azure portal to detect any unauthorized access

### Privacy Considerations
- Audio recordings are processed locally and via Azure OpenAI APIs only
- No audio data is stored by the application beyond your local machine
- Transcribed text remains on your local system unless explicitly shared
- Runtime logs contain operational data but no sensitive content

## Troubleshooting

### Common Issues

1. **Audio not recording**: 
   - Check microphone permissions in Windows Settings → Privacy & Security → Microphone
   - Ensure your microphone is set as the default recording device
   - Try running the application as administrator

2. **File upload not working**:
   - Verify file format is supported (WAV, MP3, MP4, MOV, AVI, M4A)
   - Check that FFmpeg is properly installed for file processing
   - Ensure file size is reasonable (large files will be chunked automatically)

3. **Transcription fails**: 
   - Verify Azure OpenAI endpoint and API key in Settings
   - Check that Whisper deployment name matches your Azure configuration
   - Monitor Runtime Logs for detailed error information

4. **Text correction not working**:
   - Confirm GPT-4 deployment name in Settings
   - Check API key permissions for GPT models
   - Verify custom correction prompt syntax

5. **Settings not saving**:
   - Ensure application has write permissions to user data folder
   - Check Windows event logs for data protection errors
   - Try running as administrator if encryption issues persist

6. **Build errors**: 
   - Ensure .NET 8.0 SDK is installed
   - Verify all NuGet packages are restored: `dotnet restore`
   - Check for any missing dependencies in project files

### Logging and Diagnostics

- Use the **Runtime Logs** window to monitor real-time application activity
- Filter logs by severity level (Error, Warning, Info, Debug)
- Export logs for detailed analysis or when reporting issues
- Check the `Logs` folder in the application directory for persistent log files

### Performance Optimization

- Large audio files are automatically chunked for better performance
- Transcription progress is displayed in real-time
- Settings are cached locally to minimize API calls
- Audio processing uses background threads to maintain UI responsiveness

### Support

For issues and questions, please open an issue on GitHub.
