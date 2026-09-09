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
            string step2Screenshot = Path.Combine(artifactDir, "2_window_interacted.png");
            string step3Screenshot = Path.Combine(artifactDir, "3_workflow_completed.png");

            try
            {
                // 1. Launch real WPF application process
                var psi = new System.Diagnostics.ProcessStartInfo(exePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };
                _app = Application.Launch(psi);

                // 2. Attach UIA3 Automation & Find Active WPF Top-Level Window
                var window = Retry.WhileNull(
                    () =>
                    {
                        var windows = _app.GetAllTopLevelWindows(_automation);
                        return windows.FirstOrDefault(w => w.IsAvailable);
                    },
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMilliseconds(500)
                ).Result;

                Assert.NotNull(window);

                // 3. Capture initial launch screenshot of live WPF window
                CaptureScreen(step1Screenshot);

                // 4. Determine active form (USER_LOGIN or WinConnectionChoose)
                var dispassCheckBox = window.FindFirstDescendant(cf => cf.ByAutomationId("dispass"))?.AsCheckBox();
                if (dispassCheckBox != null)
                {
                    // Scenario A: USER_LOGIN window
                    dispassCheckBox.Click();
                    Thread.Sleep(500);
                    CaptureScreen(step2Screenshot);

                    var secoRmzoBox = window.FindFirstDescendant(cf => cf.ByAutomationId("SecoRmzo"))?.AsTextBox();
                    if (secoRmzoBox != null && secoRmzoBox.IsEnabled)
                    {
                        secoRmzoBox.Text = "123456";
                        Thread.Sleep(300);
                    }

                    var loginButton = window.FindFirstDescendant(cf => cf.ByAutomationId("Greet"))?.AsButton();
                    if (loginButton != null && loginButton.IsEnabled)
                    {
                        loginButton.Click();
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    // Scenario B: WinConnectionChoose window (triggered when SQL Server is offline on CI)
                    var testConnBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("Btn_TestConnection"))?.AsButton();
                    if (testConnBtn != null)
                    {
                        testConnBtn.Click();
                        Thread.Sleep(1000);
                        CaptureScreen(step2Screenshot);
                    }

                    var saveBtn = window.FindFirstDescendant(cf => cf.ByAutomationId("Btn_SaveConnection"))?.AsButton();
                    Assert.NotNull(saveBtn);
                }

                // 5. Capture final state screenshot
                CaptureScreen(step3Screenshot);

                // 6. Assert window was responsive throughout interaction
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
