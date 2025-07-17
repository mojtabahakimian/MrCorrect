using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    public class CL_FileSecretHandler
    {
        /// <summary>
        /// Deletes a file in the temporary directory asynchronously and silently.
        /// </summary>
        /// <param name="filePath">The full path of the file to delete.</param>
        public static async Task DeleteFileSilentlyAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            await Task.Run(() =>
            {
                try
                {
                    // Ensure the file is not locked
                    while (IsFileLocked(filePath))
                    {
                        Thread.Sleep(500); // Wait for the file to be released
                    }

                    // Delete the file
                    File.Delete(filePath);
                }
                catch (Exception)
                {
                    // Suppress all exceptions silently
                }
            });
        }

        /// <summary>
        /// Checks if a file is locked by another process.
        /// </summary>
        /// <param name="filePath">The full path of the file to check.</param>
        /// <returns>True if the file is locked, false otherwise.</returns>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
                return false; // File is not locked
            }
            catch (IOException)
            {
                return true; // File is locked
            }
        }
    }
}
