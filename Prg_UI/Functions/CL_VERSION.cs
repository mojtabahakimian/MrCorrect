using Dapper;
using Microsoft.Data.SqlClient;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Functions
{
    public static class CL_VERSION
    {
        //public static string MrCorrectFullVersion { get; } = "Version 1.0.0.42 Date : 1403/08/21"; //46
        public static string MrCorrectFullVersion { get; } = "Version 1.0.0.256 Date : 1404/07/30";

        /// <summary>
        /// Checks if the current application version is valid and greater than or equal to the version stored in the database.
        /// Updates the database version if the current version is newer.
        /// </summary>
        /// <returns>True if the current version is valid and up-to-date or newer; False if the stored version is newer.</returns>
        public static bool IsValidGreaterVersion()
        {
            string fullVersionFromDB = string.Empty;

            using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();

                // Ensure the version table exists (attempt to create if not)
                try
                {
                    db.Execute(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'NOISREV')
                        BEGIN
                            CREATE TABLE dbo.NOISREV (
                                ID INT PRIMARY KEY IDENTITY(1,1),
                                VSR NVARCHAR(100) NOT NULL, -- Increased size for safety
                                UpdatedOn DATETIME2 DEFAULT (GETDATE())
                            );
                        END");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not ensure NOISREV table exists. {ex.Message}");
                }

                // Get the stored version string from the database
                // Using QueryFirstOrDefault for safety in case the table is empty
                fullVersionFromDB = db.QueryFirstOrDefault<string?>("SELECT TOP 1 VSR FROM dbo.NOISREV") ?? string.Empty;

                // If no version is stored, insert the current version and assume it's valid
                if (string.IsNullOrEmpty(fullVersionFromDB))
                {
                    try
                    {
                        // Use parameterized query to prevent SQL injection
                        db.Execute("INSERT INTO dbo.NOISREV (VSR) VALUES (@Version)", new { Version = MrCorrectFullVersion });
                        fullVersionFromDB = MrCorrectFullVersion;
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("C:\\CORRECT\\VersionCheckLog.txt",
                            $"{DateTime.Now}: DB Insert failed: {ex.Message} \n{ex.StackTrace}\n");
                        return true; // Example: Allow app to run even if DB insert fails first time
                    }
                }

                // Compare the current application version with the stored database version
                try
                {
                    // Only perform comparison if versions differ
                    if (fullVersionFromDB != MrCorrectFullVersion)
                    {
                        // Extract version and date components from both strings
                        var currentAppData = ExtractVersionData(MrCorrectFullVersion);
                        var storedDbData = ExtractVersionData(fullVersionFromDB);

                        // Perform a proper version comparison
                        if (currentAppData.Version >= storedDbData.Version) // Uses System.Version comparison
                        {
                            // If versions are equal, check the date (though typically version includes date implicitly)
                            if (currentAppData.Version == storedDbData.Version && currentAppData.Date < storedDbData.Date)
                            {
                                // Stored version is same number but newer date - unusual case, treat as newer
                                return false;
                            }

                            // Current version is newer or equal (with same/newer date), update DB
                            // Use parameterized query
                            db.Execute("UPDATE dbo.NOISREV SET VSR = @Version, UpdatedOn = GETDATE()", new { Version = MrCorrectFullVersion });
                        }
                        else
                        {
                            // Stored version is strictly newer than current app version
                            return false;
                        }
                    }

                    // Versions match, or current version is newer and DB was updated
                    return true;
                }
                catch (Exception ex)
                {
                    File.AppendAllText("C:\\CORRECT\\VersionCheckLog.txt",
                        $"{DateTime.Now}: Version check/update failed: {ex.Message} \n{ex.StackTrace}\n");
                    return true;
                }
            } // using SqlConnection ensures disposal
        }

        /// <summary>
        /// Extracts the Version object and DateTime from the formatted version string.
        /// Handles potential Persian dates.
        /// </summary>
        /// <param name="versionString">The string like "Version 1.0.0.660 Date : 1404/02/30"</param>
        /// <returns>A tuple containing the System.Version and DateTime.</returns>
        /// <exception cref="FormatException">Throws if the string format is invalid.</exception>
        private static (Version Version, DateTime Date) ExtractVersionData(string versionString)
        {
            if (string.IsNullOrWhiteSpace(versionString))
            {
                throw new FormatException("Version string cannot be empty.");
            }

            // Regex to capture version (4 parts) and date (Persian or Gregorian format)
            var match = Regex.Match(versionString, @"Version\s+(\d+\.\d+\.\d+\.\d+)\s+Date\s+:\s+(\d{4}/\d{2}/\d{2})");

            if (!match.Success || match.Groups.Count != 3)
            {
                throw new FormatException($"Version string '{versionString}' is not in the expected format 'Version #.#.#.# Date : YYYY/MM/DD'.");
            }

            string versionPart = match.Groups[1].Value;
            string datePart = match.Groups[2].Value;

            // Parse the version part
            if (!System.Version.TryParse(versionPart, out Version version))
            {
                throw new FormatException($"Could not parse version part '{versionPart}' from string '{versionString}'.");
            }

            // --- Fix for Persian Date ---
            // Use PersianCalendar and fa-IR culture to parse the date
            DateTime date;
            try
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                persianCulture.DateTimeFormat.Calendar = new PersianCalendar();
                date = DateTime.ParseExact(datePart, "yyyy/MM/dd", persianCulture);
            }
            catch (FormatException ex)
            {
                // Rethrow with more context if parsing fails
                throw new FormatException($"Could not parse date part '{datePart}' from string '{versionString}' as a Persian date.", ex);
            }
            // --- End Fix ---

            return (version, date);
        }
    }
}
