using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Packaging;
using MimeDetective;

namespace Functions
{
    public static class ExtensionTypeManage
    {
        public static string GetExtensionFileType(byte[] ByteyFile)
        {
            string? FormatTypeSuffix = null;

            var Inspector = new ContentInspectorBuilder() { Definitions = MimeDetective.Definitions.Default.All() }.Build(); //Initialize MimeDetective

            var RawData = Inspector.Inspect(ByteyFile); //Try to detect type normally
            if (RawData.Length > 0)
            {
                string BYEX = RawData.ByFileExtension().FirstOrDefault().Extension.ToLower();
                switch (BYEX)
                {
                    case "eml": FormatTypeSuffix = ".txt"; break;

                    default:
                        FormatTypeSuffix = "." + RawData.ByFileExtension().FirstOrDefault().Extension;
                        break;
                }
            }
            else //Suppose is OLE object MS Access
            {
                var ExtractedBytey = MimeTypeManagement.GetNormalizeBytes(ByteyFile); // Get Main Valid Data From OLE Data

                //Try To See What file Really is: -----------------------------------------------------------
                if (IsValidPdf(ExtractedBytey) || IsValidPdf(ByteyFile))   //PDF: //Install-Package itext7
                {
                    FormatTypeSuffix = ".pdf";
                }
                else if (IsValidPhoto(ExtractedBytey) || IsValidPhoto(ByteyFile)) //Pictures
                {
                    string fileExtension = PictureHelper.TryGetExtension(ByteyFile);
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        FormatTypeSuffix = "." + fileExtension;
                    }
                    else
                    {
                        FormatTypeSuffix = "." + PictureHelper.TryGetExtension(ExtractedBytey);
                    }
                }
                else if (IsValidDoc(ExtractedBytey) || IsValidDoc(ByteyFile)) //Install-Package OpenMcdf //Documents e.g : xls , docx ,zip
                {
                    var DTC = DocumentTypeDetector.GetDocumentType(ExtractedBytey);
                    if (!string.IsNullOrEmpty(DTC))
                    {
                        FormatTypeSuffix = "." + DocumentTypeDetector.GetDocumentType(ExtractedBytey);
                    }
                }

                //Still Did not detect type so let's Find out again
                if (string.IsNullOrEmpty(FormatTypeSuffix) || string.IsNullOrWhiteSpace(FormatTypeSuffix) || FormatTypeSuffix == ".")
                {
                    RawData = Inspector.Inspect(ExtractedBytey);
                    if (RawData.Length > 0)
                    {
                        FormatTypeSuffix = "." + RawData.ByFileExtension().FirstOrDefault().Extension;
                    }
                    else
                    {
                        // Final Last Try to get type
                        FormatTypeSuffix = MimeTypeManagement.GetFileExtension(ExtractedBytey);
                    }
                }

                //-------------------------------------------------------------------------------------------
            }

            return FormatTypeSuffix;
        }

        public static bool IsValidPdf(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    // Attempt to open the PDF document from the byte array
                    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(ms)))
                    {
                        // If no exceptions were thrown, it's likely a valid PDF
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // If an error occurs, assume it's not a valid PDF
                return false;
            }
        }
        public static bool IsValidPhoto(byte[] byteArray)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();

                    return true;
                }
            }
            catch (Exception)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Copy data from the original byte array, skipping the OLE header.
                        ms.Write(byteArray, 78, byteArray.Length - 78);

                        // Create a BitmapImage.
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // Image is cached on load
                        ms.Seek(0, SeekOrigin.Begin); // Rewind the stream.
                        image.StreamSource = ms; // Use the MemoryStream as the source
                        image.EndInit(); // Initializes the image
                        image.Freeze(); // Make the BitmapImage thread-safe

                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public static bool IsValidDoc(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    // Attempt to open the Wordprocessing document from the byte array
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(ms, false))
                    {
                        // If no exceptions were thrown, it's likely a valid .docx
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // If an error occurs, assume it's not a valid .docx
                return false;
            }
        }
        public class DocumentTypeDetector
        {
            public static string GetDocumentType(byte[] bytes)
            {
                // Check for null or empty array
                if (bytes == null || bytes.Length == 0) return "";

                // Convert the first few bytes to a string for comparison
                string header = Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 8));

                // PDF signature
                if (header.StartsWith("%PDF-")) return "PDF";

                // CFBF signature for .xls files and other older binary formats
                if (bytes.Length >= 8 && bytes[0] == 0xD0 && bytes[1] == 0xCF && bytes[2] == 0x11 && bytes[3] == 0xE0 &&
                    bytes[4] == 0xA1 && bytes[5] == 0xB1 && bytes[6] == 0x1A && bytes[7] == 0xE1)
                {
                    return "xls";
                }

                // Office Open XML formats (DOCX, XLSX, PPTX) have a ZIP container
                if (bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04)
                {
                    return DetermineOfficeOpenXMLType(bytes);
                }

                // Add more formats here as needed

                return "";
            }

            private static string DetermineOfficeOpenXMLType(byte[] bytes)
            {
                using (var ms = new MemoryStream(bytes))
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    bool hasWordDir = archive.Entries.Any(entry => entry.FullName.StartsWith("word/"));
                    bool hasXlDir = archive.Entries.Any(entry => entry.FullName.StartsWith("xl/"));
                    bool hasPptDir = archive.Entries.Any(entry => entry.FullName.StartsWith("ppt/"));

                    if (hasWordDir) return "docx";
                    if (hasXlDir) return "xlsx";
                    if (hasPptDir) return "pptx";

                    var header = bytes.Take(8).ToArray();
                    if (IsZip(header)) return "zip";
                    if (IsRar(header)) return "rar";
                }

                // If none of the specific directories are found, but it's a ZIP container,
                // it's likely an Office Open XML format, but we can't determine which one.
                return "";
            }

            private static bool IsZip(byte[] header)
            {
                // Common ZIP file signature
                return header.Length >= 4 && header[0] == 0x50 && header[1] == 0x4B &&
                       (header[2] == 0x03 && header[3] == 0x04 ||
                        header[2] == 0x05 && header[3] == 0x06 ||
                        header[2] == 0x07 && header[3] == 0x08);
            }

            private static bool IsRar(byte[] header)
            {
                // RAR 1.5 to 4.x signature
                var rarOld = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 };
                // RAR 5.0+ signature
                var rarNew = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 };

                return header.Length >= 7 && (header.Take(7).SequenceEqual(rarOld) ||
                                              header.Take(8).SequenceEqual(rarNew));
            }
        }
        public static class MimeTypeManagement
        {
            private static readonly Lazy<Dictionary<byte[], string>> fileSignatures = new Lazy<Dictionary<byte[], string>>(() =>
            {
                return new Dictionary<byte[], string>(new ByteArrayComparer())
                {

                    // Images
                    { new byte[] {  137,  80,  78,  71,  13,  10,  26,  10 }, ".png" }, // PNG
                    { new byte[] {  255,  216,  255 }, ".jpg" }, // JPEG
                    { new byte[] {  71,  73,  70,  56 }, ".gif" }, // GIF
                    { new byte[] {  73,  73,  42,  0 }, ".tiff" }, // TIFF
                    { new byte[] {  66,  77 }, ".bmp" }, // BMP
                    { new byte[] {  80,  75,  3,  4 }, ".zip" }, // ZIP and DOCX (ZIP-based formats)
                    { new byte[] {  0,  0,  1,  0 }, ".ico" }, // ICO
                                 
                    // Documents
                    { new byte[] { 208, 207, 17, 224, 161, 177, 26, 225 }, ".doc" }, // DOC
                    { new byte[] { 37, 80, 68, 70, 45 }, ".pdf" }, // PDF
                    { new byte[] { 80, 75, 3, 4, 14, 0, 6, 0 }, ".xlsx" }, 
                                 
                    // Audio and Video
                    { new byte[] { 73, 68, 51 }, ".mp3" }, // MP3
                    { new byte[] { 255, 251, 48 }, ".mp3" }, // MP3 without ID3
                    { new byte[] { 79, 103, 103, 83 }, ".ogg" }, // OGG
                    { new byte[] { 82, 73, 70, 70 }, ".wav" }, // WAV
                    { new byte[] { 0, 0, 1, 0x76 }, ".mp4" }, // MP4
                                 
                    // Archives
                    { new byte[] { 82, 97, 114, 33, 26, 7, 0 }, ".rar" }, // RAR
                    { new byte[] { 82, 97, 114, 33, 26, 7, 0, 0 }, ".rar" }, // RAR 5.x
                    { new byte[] { 82, 97, 114, 33, 26, 7, 1, 0 }, ".rar" }, // RAR 5.x
                    { new byte[] { 31, 139, 8 }, ".gz" }, // GZIP
                    { new byte[] { 80, (byte)'K', 3, 4, 20 }, ".7z" }, // 7Z
                                 
                    // Others
                    { new byte[] { 37, 33, 80, 83 }, ".ps" }, // PostScript
                    { new byte[] { 255, 254 }, ".txt" }, // TXT (UTF-16 LE)
                    { new byte[] { 254, 255 }, ".txt" }, // TXT (UTF-16 BE)
                    { new byte[] { 239, 187, 191 }, ".txt" }, // TXT with BOM (UTF-8)
                    { new byte[] { 0, 1, 0, 0 }, ".ttf" }, // TTF
                    { new byte[] { 77, 90 }, ".exe" }, // Executables (EXE, DLL)
                };
            });
            public static byte[] GetNormalizeBytes(byte[] theFile)
            {
                if (theFile == null || !theFile.Any()) return Array.Empty<byte>();

                foreach (var signature in fileSignatures.Value.Keys)
                {
                    int startIndex = FindSignatureIndex(theFile, signature);
                    if (startIndex != -1)
                    {
                        return theFile.Skip(startIndex).ToArray();
                    }
                }
                return theFile;
            }
            public static string GetFileExtension(byte[] fileData)
            {
                if (fileData == null || !fileData.Any()) return null;

                string _SV_ = ".txt";

                foreach (var signature in fileSignatures.Value)
                {
                    if (fileData.Length >= signature.Key.Length && MatchesSignature(fileData, signature.Key))
                    {
                        _SV_ = signature.Value;
                    }
                }

                return _SV_;
            }
            private static bool MatchesSignature(byte[] fileData, byte[] signature)
            {
                for (int i = 0; i < signature.Length; i++)
                {
                    if (fileData[i] != signature[i])
                        return false;
                }
                return true;
            }
            private static int FindSignatureIndex(byte[] data, byte[] signature)
            {
                int limit = data.Length - signature.Length;
                for (int i = 0; i <= limit; i++)
                {
                    bool isMatch = true;
                    for (int j = 0; j < signature.Length; j++)
                    {
                        if (data[i + j] != signature[j])
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch) return i;
                }
                return -1; // Signature not found
            }
            private class ByteArrayComparer : IEqualityComparer<byte[]>
            {
                public bool Equals(byte[] x, byte[] y)
                {
                    if (x == null || y == null) return false;
                    return x.SequenceEqual(y);
                }

                public int GetHashCode(byte[] obj)
                {
                    unchecked // Overflow is fine, just wrap
                    {
                        int hash = 17;
                        foreach (byte element in obj)
                        {
                            hash = hash * 31 + element;
                        }
                        return hash;
                    }
                }
            }
        }
        public static class PictureHelper
        {
            // some magic bytes for the most important image formats, see Wikipedia for more
            static readonly List<byte> jpg = new List<byte> { 0xFF, 0xD8 };
            static readonly List<byte> bmp = new List<byte> { 0x42, 0x4D };
            static readonly List<byte> gif = new List<byte> { 0x47, 0x49, 0x46 };
            static readonly List<byte> png = new List<byte> { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            static readonly List<byte> svg_xml_small = new List<byte> { 0x3C, 0x3F, 0x78, 0x6D, 0x6C }; // "<?xml"
            static readonly List<byte> svg_xml_capital = new List<byte> { 0x3C, 0x3F, 0x58, 0x4D, 0x4C }; // "<?XML"
            static readonly List<byte> svg_small = new List<byte> { 0x3C, 0x73, 0x76, 0x67 }; // "<svg"
            static readonly List<byte> svg_capital = new List<byte> { 0x3C, 0x53, 0x56, 0x47 }; // "<SVG"
            static readonly List<byte> intel_tiff = new List<byte> { 0x49, 0x49, 0x2A, 0x00 };
            static readonly List<byte> motorola_tiff = new List<byte> { 0x4D, 0x4D, 0x00, 0x2A };

            static readonly List<(List<byte> magic, string extension)> imageFormats = new List<(List<byte> magic, string extension)>()
            {
                (jpg, "jpg"),
                (bmp, "bmp"),
                (gif, "gif"),
                (png, "png"),
                (svg_small, "svg"),
                (svg_capital, "svg"),
                (intel_tiff,"tif"),
                (motorola_tiff, "tif"),
                (svg_xml_small, "svg"),
                (svg_xml_capital, "svg")
            };

            public static string TryGetExtension(Byte[] array)
            {
                // check for simple formats first
                foreach (var imageFormat in imageFormats)
                {
                    if (IsImage(array, imageFormat.magic))
                    {
                        if (imageFormat.magic != svg_xml_small && imageFormat.magic != svg_xml_capital)
                            return imageFormat.extension;

                        // special handling for SVGs starting with XML tag
                        int readCount = imageFormat.magic.Count; // skip XML tag
                        int maxReadCount = 1024;

                        do
                        {
                            if (IsImage(array, svg_small, readCount) || IsImage(array, svg_capital, readCount))
                            {
                                return imageFormat.extension;
                            }
                            readCount++;
                        }
                        while (readCount < maxReadCount && readCount < array.Length - 1);

                        return null;
                    }
                }
                return null;
            }

            private static bool IsImage(Byte[] array, List<byte> comparer, int offset = 0)
            {
                int arrayIndex = offset;
                foreach (byte c in comparer)
                {
                    if (arrayIndex > array.Length - 1 || array[arrayIndex] != c)
                        return false;
                    ++arrayIndex;
                }
                return true;
            }
        }
    }

}
