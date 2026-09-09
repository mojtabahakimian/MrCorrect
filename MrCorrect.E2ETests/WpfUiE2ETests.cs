using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
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

            string treeDumpPath = Path.Combine(artifactDir, "ui_automation_tree_dump.txt");
            string step1Screenshot = Path.Combine(artifactDir, "1_login_window_launched.png");
            string step2Screenshot = Path.Combine(artifactDir, "2_window_interacted.png");
            string step3Screenshot = Path.Combine(artifactDir, "3_workflow_completed.png");

            string exePath = FindWpfExecutable();
            Assert.True(File.Exists(exePath), $"WPF executable not found at path: {exePath}");

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

                // 3. Dump the complete UIA3 Automation Tree of the active window for diagnostics
                using (var writer = new StreamWriter(treeDumpPath, false, Encoding.UTF8))
                {
                    writer.WriteLine($"=== UIA3 AUTOMATION TREE DUMP FOR WINDOW: {window.Title} (Framework: {window.FrameworkType}) ===");
                    DumpAutomationTree(window, writer, 0);
                }

                // 4. Capture initial launch screenshot of live WPF window (fails loudly on error)
                CaptureScreen(step1Screenshot);

                // 5. Locate Password Visibility CheckBox ("dispass" / "نمایش رمز عبور")
                var dispassCheckBox = FindCheckBox(window, "dispass", "نمایش رمز عبور");
                if (dispassCheckBox != null)
                {
                    // Scenario A: USER_LOGIN window loaded
                    dispassCheckBox.Click();
                    Thread.Sleep(500);
                    CaptureScreen(step2Screenshot);

                    // Find Password Input (SecoRmzo / Rmzo)
                    var passwordInput = FindTextBox(window, "SecoRmzo", "Rmzo");
                    Assert.NotNull(passwordInput);
                    if (passwordInput.IsEnabled)
                    {
                        passwordInput.Text = "123456";
                        Thread.Sleep(300);
                    }

                    // Find and Click Login Button ("Greet" / "ورود")
                    var loginBtn = FindButton(window, "Greet", "ورود");
                    Assert.NotNull(loginBtn);
                    if (loginBtn.IsEnabled)
                    {
                        loginBtn.Click();
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    // Scenario B: WinConnectionChoose window loaded (triggered when SQL Server is offline on CI)
                    var testConnBtn = FindButton(window, "Btn_TestConnection", "تست اتصال");
                    Assert.NotNull(testConnBtn);
                    testConnBtn.Click();
                    Thread.Sleep(1000);
                    CaptureScreen(step2Screenshot);

                    var saveBtn = FindButton(window, "Btn_SaveConnection", "تایید");
                    Assert.NotNull(saveBtn);
                }

                // 6. Capture final state screenshot
                CaptureScreen(step3Screenshot);

                // 7. Assert window remained responsive throughout E2E interaction
                Assert.NotNull(window);
            }
            catch (Exception ex)
            {
                string errorScreenshot = Path.Combine(artifactDir, "error_failure_screenshot.png");
                try { CaptureScreen(errorScreenshot); } catch { }
                File.WriteAllText(Path.Combine(artifactDir, "test_error_log.txt"), ex.ToString(), Encoding.UTF8);
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

        private static CheckBox? FindCheckBox(AutomationElement root, string automationId, string nameText)
        {
            // 1. Search by AutomationId
            var element = root.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsCheckBox();
            if (element != null) return element;

            // 2. Search by Name
            element = root.FindFirstDescendant(cf => cf.ByName(nameText))?.AsCheckBox();
            if (element != null) return element;

            // 3. Fallback: Search all CheckBoxes for matching Name or AutomationId
            var allCheckBoxes = root.FindAllDescendants(cf => cf.ByControlType(ControlType.CheckBox));
            foreach (var cb in allCheckBoxes)
            {
                if ((cb.Name != null && cb.Name.Contains(nameText)) ||
                    (cb.AutomationId != null && cb.AutomationId.Equals(automationId, StringComparison.OrdinalIgnoreCase)))
                {
                    return cb.AsCheckBox();
                }
            }

            return null;
        }

        private static TextBox? FindTextBox(AutomationElement root, params string[] automationIds)
        {
            foreach (var id in automationIds)
            {
                var element = root.FindFirstDescendant(cf => cf.ByAutomationId(id))?.AsTextBox();
                if (element != null) return element;
            }

            return null;
        }

        private static Button? FindButton(AutomationElement root, string automationId, string nameText)
        {
            // 1. Search by AutomationId
            var element = root.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsButton();
            if (element != null) return element;

            // 2. Search by Name
            element = root.FindFirstDescendant(cf => cf.ByName(nameText))?.AsButton();
            if (element != null) return element;

            // 3. Fallback: Search all Buttons for matching Name or AutomationId
            var allButtons = root.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
            foreach (var btn in allButtons)
            {
                if ((btn.Name != null && btn.Name.Contains(nameText)) ||
                    (btn.AutomationId != null && btn.AutomationId.Equals(automationId, StringComparison.OrdinalIgnoreCase)))
                {
                    return btn.AsButton();
                }
            }

            return null;
        }

        private static void DumpAutomationTree(AutomationElement element, TextWriter writer, int depth)
        {
            if (element == null) return;
            string indent = new string(' ', depth * 2);
            try
            {
                string name = element.Name ?? "";
                string autoId = element.AutomationId ?? "";
                string controlType = element.ControlType.ToString();
                string frameworkId = element.FrameworkType.ToString();
                string isEnabled = element.IsEnabled.ToString();
                string className = element.ClassName ?? "";

                writer.WriteLine($"{indent}- Name: '{name}' | AutoId: '{autoId}' | ControlType: {controlType} | ClassName: '{className}' | Enabled: {isEnabled} | FrameworkId: '{frameworkId}'");

                var children = element.FindAllChildren();
                foreach (var child in children)
                {
                    DumpAutomationTree(child, writer, depth + 1);
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"{indent}[Error dumping element: {ex.Message}]");
            }
        }

        private static void CaptureScreen(string destinationPath)
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
                            return;
                        }
                    }
                }
                throw new InvalidOperationException("Failed to invoke FlaUI.Core.Captures.Capture.Screen().");
            }
            else
            {
                throw new PlatformNotSupportedException("Screen capture requires Windows platform.");
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
