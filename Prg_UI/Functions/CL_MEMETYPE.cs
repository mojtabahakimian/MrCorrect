using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class FileSignatureDetector
    {
        private static readonly Dictionary<byte[], FileSignatureInfo> FileSignatures = new Dictionary<byte[], FileSignatureInfo>
        {
            // Images
            { new byte[]{0xFF, 0xD8, 0xFF}, new FileSignatureInfo(".jpg", "image/jpeg", SignatureType.Simple) },
            { new byte[]{0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}, new FileSignatureInfo(".png", "image/png", SignatureType.Simple) },
            { new byte[]{0x47, 0x49, 0x46, 0x38}, new FileSignatureInfo(".gif", "image/gif", SignatureType.Simple) },
            { new byte[]{0x42, 0x4D}, new FileSignatureInfo(".bmp", "image/bmp", SignatureType.Simple) },
            
            // Documents
            { new byte[]{0x25, 0x50, 0x44, 0x46}, new FileSignatureInfo(".pdf", "application/pdf", SignatureType.Simple) },
            { new byte[]{0x25, 0x21, 0x50, 0x53}, new FileSignatureInfo(".ps", "application/postscript", SignatureType.Simple) },
            { new byte[]{0x4D, 0x5A}, new FileSignatureInfo(".exe", "application/x-msdownload", SignatureType.Simple) },

            // RAR Archives
            { new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 },
                new FileSignatureInfo(".rar", "application/x-rar-compressed", SignatureType.Rar4) },
            { new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 },
                new FileSignatureInfo(".rar", "application/x-rar-compressed", SignatureType.Rar5) },

            // ZIP and Office Files (all share same signature but different internal structure)
            { new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                new FileSignatureInfo(".zip", "application/zip", SignatureType.ZipBased) },
        };

        // Known Office file content types
        private static readonly Dictionary<string, FileSignatureInfo> OfficeContentTypes = new Dictionary<string, FileSignatureInfo>
        {
            { "word/document.xml", new FileSignatureInfo(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", SignatureType.Office) },
            { "xl/workbook.xml", new FileSignatureInfo(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", SignatureType.Office) },
            { "ppt/presentation.xml", new FileSignatureInfo(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", SignatureType.Office) }
        };

        public static async Task<FileTypeResult> DetectFileTypeAsync(byte[] fileBytes, string fileName = null)
        {
            try
            {
                // Basic signature check
                foreach (var signature in FileSignatures)
                {
                    if (fileBytes.Take(signature.Key.Length).SequenceEqual(signature.Key))
                    {
                        var info = signature.Value;

                        // Special handling for ZIP-based formats
                        if (info.Type == SignatureType.ZipBased)
                        {
                            return await DetectZipBasedFormat(fileBytes, fileName);
                        }

                        return new FileTypeResult
                        {
                            Extension = info.Extension,
                            MimeType = info.MimeType,
                            IsValid = true
                        };
                    }
                }

                return new FileTypeResult { IsValid = false };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error detecting file type: {ex.Message}");
                return new FileTypeResult { IsValid = false, Error = ex.Message };
            }
        }

        private static async Task<FileTypeResult> DetectZipBasedFormat(byte[] fileBytes, string fileName)
        {
            try
            {
                using (var memoryStream = new MemoryStream(fileBytes))
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    // Check for Office file formats
                    foreach (var entry in archive.Entries)
                    {
                        foreach (var officeType in OfficeContentTypes)
                        {
                            if (entry.FullName.Equals(officeType.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                return new FileTypeResult
                                {
                                    Extension = officeType.Value.Extension,
                                    MimeType = officeType.Value.MimeType,
                                    IsValid = true
                                };
                            }
                        }
                    }

                    // If no office format detected, it's a regular ZIP
                    return new FileTypeResult
                    {
                        Extension = ".zip",
                        MimeType = "application/zip",
                        IsValid = true
                    };
                }
            }
            catch (InvalidDataException)
            {
                // If we can't read it as a ZIP file, try to use the original filename
                if (!string.IsNullOrEmpty(fileName))
                {
                    string ext = Path.GetExtension(fileName).ToLowerInvariant();
                    if (OfficeContentTypes.Values.Any(v => v.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                    {
                        var officeType = OfficeContentTypes.Values.First(v => v.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
                        return new FileTypeResult
                        {
                            Extension = officeType.Extension,
                            MimeType = officeType.MimeType,
                            IsValid = true,
                            Warning = "File structure could not be verified"
                        };
                    }
                }

                return new FileTypeResult { IsValid = false, Error = "Invalid ZIP file structure" };
            }
        }
    }

    public class FileSignatureInfo
    {
        public string Extension { get; set; }
        public string MimeType { get; set; }
        public SignatureType Type { get; set; }

        public FileSignatureInfo(string extension, string mimeType, SignatureType type)
        {
            Extension = extension;
            MimeType = mimeType;
            Type = type;
        }
    }

    public enum SignatureType
    {
        Simple,
        ZipBased,
        Rar4,
        Rar5,
        Office
    }

    public class FileTypeResult
    {
        public string Extension { get; set; }
        public string MimeType { get; set; }
        public bool IsValid { get; set; }
        public string Error { get; set; }
        public string Warning { get; set; }
    }

}
