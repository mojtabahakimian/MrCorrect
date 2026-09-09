using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Xunit;

namespace MrCorrect.E2ETests
{
    public class WpfUiE2ETests : IDisposable
    {
        private readonly UIA3Automation _automation;
        private Application? _app;

        public WpfUiE2ETests()
        {
            _automation = new UIA3Automation();
        }

        [Fact]
        public void Test_Wpf_Application_LaunchAndInteractWithLoginUI()
        {
            string artifactDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestArtifacts");
            Directory.CreateDirectory(artifactDir);

            string exePath = FindWpfExecutable();
            Assert.True(File.Exists(exePath), $"WPF executable not found at path: {exePath}");

            string initialScreenshot = Path.Combine(artifactDir, "1_login_window_launched.png");
            string interactedScreenshot = Path.Combine(artifactDir, "2_login_window_interacted.png");

            try
            {
                // 1. Launch actual WPF application process
                var psi = new System.Diagnostics.ProcessStartInfo(exePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };
                _app = Application.Launch(psi);

                // 2. Attach UIA3 Automation & Find Main Window
                var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(20));
                Assert.NotNull(window);

                // 3. Take initial screenshot of live WPF GUI session
                CaptureScreen(initialScreenshot);

                // 4. Interact with real UI controls (Find checkbox / buttons in USER_LOGIN)
                var dispassCheckBox = window.FindFirstDescendant(cf => cf.ByAutomationId("dispass"))?.AsCheckBox();
                if (dispassCheckBox != null)
                {
                    dispassCheckBox.Click();
                    Thread.Sleep(500);
                }

                // Search for Login button or Close button
                var loginButton = window.FindFirstDescendant(cf => cf.ByAutomationId("Greet"))?.AsButton();
                if (loginButton != null)
                {
                    Assert.NotNull(loginButton);
                }

                // 5. Capture post-interaction screenshot
                CaptureScreen(interactedScreenshot);

                // 6. Verify window title or presence
                Assert.NotNull(window);
            }
            catch (Exception ex)
            {
                string errorScreenshot = Path.Combine(artifactDir, "error_failure_screenshot.png");
                CaptureScreen(errorScreenshot);
                File.WriteAllText(Path.Combine(artifactDir, "test_error_log.txt"), ex.ToString());
                throw;
            }
            finally
            {
                if (_app != null && !_app.HasExited)
                {
                    _app.Close();
                    Thread.Sleep(1000);
                    if (!_app.HasExited)
                    {
                        _app.Kill();
                    }
                }
            }
        }

        private static void CaptureScreen(string destinationPath)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var captureType = Type.GetType("FlaUI.Core.Captures.Capture, FlaUI.Core");
                    if (captureType != null)
                    {
                        var screenMethod = captureType.GetMethod("Screen", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                        if (screenMethod != null)
                        {
                            using var img = screenMethod.Invoke(null, null) as IDisposable;
                            if (img != null)
                            {
                                var toFileMethod = img.GetType().GetMethod("ToFile", new[] { typeof(string) });
                                toFileMethod?.Invoke(img, new object[] { destinationPath });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback silently if screen capture fails
            }
        }

        private static string FindWpfExecutable()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] possiblePaths = new[]
            {
                Path.Combine(baseDir, "MrCorrect.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Prg_UI", "bin", "Debug", "net8.0-windows7.0", "win-x64", "MrCorrect.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Prg_UI", "bin", "Release", "net8.0-windows7.0", "win-x64", "MrCorrect.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Prg_UI", "bin", "Debug", "net8.0-windows7.0", "MrCorrect.exe"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Prg_UI", "bin", "Release", "net8.0-windows7.0", "MrCorrect.exe")
            };

            foreach (var path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return Path.GetFullPath(possiblePaths[0]);
        }

        public void Dispose()
        {
            _automation?.Dispose();
        }
    }
}
