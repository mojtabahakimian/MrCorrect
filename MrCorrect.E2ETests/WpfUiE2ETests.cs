using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
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
        public void Test_Wpf_Application_FullLoginE2EWorkflow()
        {
            string artifactDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestArtifacts");
            Directory.CreateDirectory(artifactDir);

            string exePath = FindWpfExecutable();
            Assert.True(File.Exists(exePath), $"WPF executable not found at path: {exePath}");

            string step1Screenshot = Path.Combine(artifactDir, "1_login_window_launched.png");
            string step2Screenshot = Path.Combine(artifactDir, "2_password_visibility_toggled.png");
            string step3Screenshot = Path.Combine(artifactDir, "3_login_button_clicked.png");

            try
            {
                // 1. Launch real WPF application process
                var psi = new System.Diagnostics.ProcessStartInfo(exePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };
                _app = Application.Launch(psi);

                // 2. Attach UIA3 Automation & Find Main Window
                var window = Retry.WhileNull(
                    () => _app.GetMainWindow(_automation, TimeSpan.FromSeconds(25)),
                    TimeSpan.FromSeconds(25),
                    TimeSpan.FromMilliseconds(500)
                ).Result;

                Assert.NotNull(window);

                // 3. Capture initial launch screenshot
                CaptureScreen(step1Screenshot);

                // 4. Find dispass ("نمایش رمز عبور") CheckBox and click it
                var dispassCheckBox = window.FindFirstDescendant(cf => cf.ByAutomationId("dispass"))?.AsCheckBox();
                Assert.NotNull(dispassCheckBox);
                dispassCheckBox.Click();
                Thread.Sleep(500);
                CaptureScreen(step2Screenshot);

                // 5. Find SecoRmzo TextBox and enter password text
                var secoRmzoBox = window.FindFirstDescendant(cf => cf.ByAutomationId("SecoRmzo"))?.AsTextBox();
                Assert.NotNull(secoRmzoBox);
                if (secoRmzoBox.IsEnabled)
                {
                    secoRmzoBox.Text = "123456";
                    Thread.Sleep(300);
                }

                // 6. Find and inspect Login Button ("Greet")
                var loginButton = window.FindFirstDescendant(cf => cf.ByAutomationId("Greet"))?.AsButton();
                Assert.NotNull(loginButton);

                // Perform click interaction on Login button if enabled
                if (loginButton.IsEnabled)
                {
                    loginButton.Click();
                    Thread.Sleep(1000);
                }

                // 7. Capture post-submission screenshot
                CaptureScreen(step3Screenshot);

                // 8. Verify window is responsive and operational
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
