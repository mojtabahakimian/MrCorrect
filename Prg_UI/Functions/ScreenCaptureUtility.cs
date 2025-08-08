using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
using Prg_Proccessy.MODELS;

namespace Functions
{
    public class ScreenCaptureUtility
    {
        private static readonly string ScreenshotPath = Path.Combine("C:", "Correct", "ErrorScreenshots");


        public static string CaptureScreenOnError(Exception ex)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(ScreenshotPath);

                // Generate unique filename
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string errorType = ex.GetType().Name;
                string filename = $"Error_{errorType}_{timestamp}.png";
                string fullPath = Path.Combine(ScreenshotPath, filename);

                using (Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        try
                        {
                            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

                            // Add error information overlay
                            using (Font font = new Font("Arial", 10))
                            using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Red))
                            {
                                string errorInfo = $"Error: {ex.Message}\nTimestamp: {timestamp}\nUsername: {Baseknow.UUSER}";
                                graphics.DrawString(errorInfo, font, brush, new PointF(10, 10));
                            }

                            // Save with compression
                            using var encoderParameters = new EncoderParameters(1);
                            using var qualityParameter = new EncoderParameter(Encoder.Quality, 85L);
                            encoderParameters.Param[0] = qualityParameter;
                            var codec = GetEncoderInfo("image/jpeg");

                            bitmap.Save(fullPath, codec, encoderParameters);
                        }
                        catch (Exception captureEx)
                        {
                        }
                    }
                }

                return fullPath;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<string> CaptureScreenOnErrorAsync(Exception ex)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(ScreenshotPath);

                // Generate unique filename
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                string errorType = ex.GetType().Name;
                string filename = $"Error_{errorType}_{timestamp}.png";
                string fullPath = Path.Combine(ScreenshotPath, filename);

                // Capture primary screen
                await Task.Run(() =>
                {
                    using (Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            try
                            {
                                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

                                // Add error information overlay
                                using (Font font = new Font("Arial", 10))
                                using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Red))
                                {
                                    string errorInfo = $"Error: {ex.Message}\nTimestamp: {timestamp}";
                                    graphics.DrawString(errorInfo, font, brush, new PointF(10, 10));
                                }

                                // Save with compression
                                using var encoderParameters = new EncoderParameters(1);
                                using var qualityParameter = new EncoderParameter(Encoder.Quality, 85L);
                                encoderParameters.Param[0] = qualityParameter;
                                var codec = GetEncoderInfo("image/jpeg");

                                bitmap.Save(fullPath, codec, encoderParameters);
                            }
                            catch (Exception captureEx)
                            {
                            }
                        }
                    }
                });

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return Array.Find(codecs, codec => codec.MimeType == mimeType);
        }

        // Clean up old screenshots (keep last 50)
        public static void CleanupOldScreenshots()
        {
            try
            {
                var directory = new DirectoryInfo(ScreenshotPath);
                if (!directory.Exists) return;

                var files = directory.GetFiles("Error_*.png")
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(50);

                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { } // Ignore deletion errors
                }
            }
            catch { } // Ignore cleanup errors
        }
    }
}
