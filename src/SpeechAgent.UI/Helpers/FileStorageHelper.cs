using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.UI.Helpers
{
    /// <summary>
    /// Provides utility methods for saving files to the SpeechAgent recordings directory.
    /// </summary>
    internal static class FileStorageHelper
    {
        /// <summary>
        /// Ensures the recordings directory exists and returns its path.
        /// </summary>
        public static string EnsureRecordingsDirectory()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var recordingsDir = Path.Combine(documentsPath, "SpeechAgent", "Recordings");
            Directory.CreateDirectory(recordingsDir);
            return recordingsDir;
        }

        /// <summary>
        /// Saves binary data to a file within the specified directory.
        /// </summary>
        public static async Task SaveBytesToFileAsync(string directory, string filename, byte[] data)
        {
            var filePath = Path.Combine(directory, filename);
            await File.WriteAllBytesAsync(filePath, data);
        }

        /// <summary>
        /// Saves text content to a file within the specified directory.
        /// </summary>
        public static async Task SaveTextToFileAsync(string directory, string filename, string content)
        {
            var filePath = Path.Combine(directory, filename);
            await File.WriteAllTextAsync(filePath, content);
        }
    }
}
