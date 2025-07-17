using HeyRed.Mime;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Functions
{
    public class FileTypeDetector
    {
        private static readonly Dictionary<byte[], string> FileSignatures = new Dictionary<byte[], string>
        {
            { new byte[] { 0xFF, 0xD8, 0xFF }, ".jpg" }, // JPEG
            { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png" }, // PNG
            { new byte[] { 0x47, 0x49, 0x46, 0x38 }, ".gif" }, // GIF
            { new byte[] { 0x42, 0x4D }, ".bmp" }, // BMP
            { new byte[] { 0x25, 0x50, 0x44, 0x46 }, ".pdf" }, // PDF
            { new byte[] { 0x25, 0x21, 0x50, 0x53 }, ".ps" }, // PostScript
            { new byte[] { 0x4D, 0x5A }, ".exe" }, // Executable
            { new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 }, ".docx" }, // DOCX
            { new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x08, 0x00 }, ".xlsx" }, // XLSX
            
            //{ new byte[] { 0x50, 0x4B, 0x03, 0x04 }, ".zip" }, // ZIP (Basic)
            //{ new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, ".rar" }, // RAR v1.5 and v4.x
            //{ new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 }, ".rar" } // RAR v5.x
        };


        public static string GetFileExtension(byte[] fileData)
        {
            // First check using known file signatures
            string extension = CheckFileSignature(fileData);
            if (extension != null)
            {
                return extension;
            }

            // Handle `application/zip` type files (e.g., .xlsx, .docx)
            if (fileData.Length > 4 && fileData[0] == 0x50 && fileData[1] == 0x4B)
            {
                extension = GetOfficeFileExtension(fileData);
                if (!string.IsNullOrEmpty(extension))
                    return extension;
            }

            // In case of no match use MIME types
            return GetMimeType(fileData);
        }

        private static string CheckFileSignature(byte[] fileData)
        {
            foreach (var signature in FileSignatures)
            {
                if (fileData.Take(signature.Key.Length).SequenceEqual(signature.Key))
                {
                    return signature.Value;
                }
            }
            return null;
        }

        private static string GetOfficeFileExtension(byte[] fileData)
        {
            // Write the byte array to a temporary file
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, fileData);

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(tempFilePath))
                {
                    // Check for specific files in the archive indicative of certain formats
                    if (archive.Entries.Any(e => e.FullName == "[Content_Types].xml"))
                    {
                        if (archive.Entries.Any(e => e.FullName.StartsWith("xl/") && e.FullName.EndsWith(".xml")))
                            return ".xlsx";
                        if (archive.Entries.Any(e => e.FullName.StartsWith("word/") && e.FullName.EndsWith(".xml")))
                            return ".docx";
                        if (archive.Entries.Any(e => e.FullName.StartsWith("ppt/") && e.FullName.EndsWith(".xml")))
                            return ".pptx";
                    }
                }
            }
            catch
            {
                // Handle exceptions if needed
            }
            finally
            {
                // Ensure the temporary file is deleted
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            return ".zip"; // Default to .zip if no specific Office format found
        }

        public static string GetMimeType(byte[] fileData)
        {
            string tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, fileData);

            try
            {
                string mimeType = MimeTypesMap.GetMimeType(tempFile);
                File.Delete(tempFile);  // Clean up the temporary file

                string extension = MimeTypesMap.GetExtension(mimeType);
                return extension ?? ".unknown";
            }
            catch
            {
                return ".unknown";
            }
        }
    }
}