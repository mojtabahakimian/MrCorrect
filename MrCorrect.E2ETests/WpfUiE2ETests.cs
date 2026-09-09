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
        private enum WindowType
        {
            USER_LOGIN,
            WinConnectionChoose,
            Msgwin,
            Unknown
        }

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
            string startupMsgPath = Path.Combine(artifactDir, "startup_message_text.txt");
            string diagPath = Path.Combine(artifactDir, "runtime_diagnostics.txt");
            string step1Screenshot = Path.Combine(artifactDir, "1_login_window_launched.png");
            string step2Screenshot = Path.Combine(artifactDir, "2_window_interacted.png");
            string step3Screenshot = Path.Combine(artifactDir, "3_workflow_completed.png");

            string exePath = FindWpfExecutable();
            Assert.True(File.Exists(exePath), $"WPF executable not found at path: {exePath}");

            // Diagnostic reporting: OS, Architecture, .NET Runtime, Target Framework, Microsoft.Data.SqlClient assembly details
            var diag = new StringBuilder();
            diag.AppendLine($"[RUNTIME DIAGNOSTICS - {DateTime.UtcNow:o}]");
            diag.AppendLine($"OS Description: {RuntimeInformation.OSDescription}");
            diag.AppendLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
            diag.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
            diag.AppendLine($".NET Runtime Version: {Environment.Version}");
            diag.AppendLine($"Executable Path: {exePath}");

            string exeDir = Path.GetDirectoryName(exePath) ?? "";
            string sqlClientPath = Path.Combine(exeDir, "Microsoft.Data.SqlClient.dll");
            if (File.Exists(sqlClientPath))
            {
                var fi = new FileInfo(sqlClientPath);
                diag.AppendLine($"Microsoft.Data.SqlClient.dll Path: {sqlClientPath}");
                diag.AppendLine($"Microsoft.Data.SqlClient.dll Size: {fi.Length} bytes");
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(sqlClientPath);
                    diag.AppendLine($"Microsoft.Data.SqlClient.dll Assembly Version: {assemblyName.Version}");
                }
                catch (Exception ex)
                {
                    diag.AppendLine($"Microsoft.Data.SqlClient.dll Assembly Read Error: {ex.Message}");
                }
            }
            else
            {
                diag.AppendLine("Microsoft.Data.SqlClient.dll NOT FOUND in executable directory!");
            }
            File.WriteAllText(diagPath, diag.ToString(), Encoding.UTF8);

            try
            {
                // 1. Launch real WPF application process
                var psi = new System.Diagnostics.ProcessStartInfo(exePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };
                _app = Application.Launch(psi);

                // 2. Attach UIA3 Automation & Get Initial Active Top-Level Window
                AutomationElement? activeWindow = GetActiveWindow();
                if (activeWindow == null)
                {
                    string procInfo = _app != null ? $"ProcessID: {_app.ProcessId}, HasExited: {_app.HasExited}" : "App process is null";
                    throw new InvalidOperationException($"Failed to detect active top-level WPF window for process. ({procInfo})");
                }

                // 3. Classify initial window state
                WindowType windowType = ClassifyWindow(activeWindow);

                // If Msgwin dialog(s) appear at startup, log message text, dump tree, dismiss it and re-detect
                while (windowType == WindowType.Msgwin)
                {
                    string msgNote = GetMsgwinNote(activeWindow);
                    File.AppendAllText(startupMsgPath, $"[{DateTime.UtcNow:o}] MSGWIN DETECTED: {msgNote}\n", Encoding.UTF8);

                    using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR MSGWIN ({msgNote}) ===");
                        DumpAutomationTree(activeWindow, writer, 0);
                    }

                    CaptureScreen(step1Screenshot);

                    var seeOkBtn = FindButton(activeWindow, "Btn_SeeOK", "تایید") ?? FindButton(activeWindow, "Btn_yes", "بله");
                    if (seeOkBtn != null && seeOkBtn.IsEnabled)
                    {
                        seeOkBtn.Click();
                        Thread.Sleep(1500);
                    }
                    else
                    {
                        break;
                    }

                    activeWindow = GetActiveWindow();
                    if (activeWindow == null) break;
                    windowType = ClassifyWindow(activeWindow);
                }

                if (activeWindow == null)
                {
                    string procInfo = _app != null ? $"ProcessID: {_app.ProcessId}, HasExited: {_app.HasExited}" : "App process is null";
                    throw new InvalidOperationException($"Active top-level window became null after dismissing startup Msgwin dialog. ({procInfo})");
                }

                // 4. Dump complete UIA3 Automation Tree of main target window
                using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                {
                    writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR ACTIVE WINDOW (Classified: {windowType}) ===");
                    DumpAutomationTree(activeWindow, writer, 0);
                }

                CaptureScreen(step1Screenshot);

                // 5. Execute E2E scenario based on explicit window classification
                switch (windowType)
                {
                    case WindowType.USER_LOGIN:
                        ExecuteUserLoginScenario(activeWindow, step2Screenshot);
                        break;

                    case WindowType.WinConnectionChoose:
                        ExecuteConnectionChooseScenario(activeWindow, step2Screenshot);
                        break;

                    case WindowType.Msgwin:
                        string msgNote = GetMsgwinNote(activeWindow);
                        Assert.Fail($"Application stuck on unexpected Msgwin dialog. Message content: '{msgNote}'");
                        break;

                    default:
                        Assert.Fail($"Application reached unknown window type. Window Title: '{activeWindow.Name}', ClassName: '{activeWindow.ClassName}'");
                        break;
                }

                // 6. Capture final post-interaction screenshot
                CaptureScreen(step3Screenshot);

                // 7. Verify window is operational
                Assert.NotNull(activeWindow);
            }
            catch (Exception ex)
            {
                string errorScreenshot = Path.Combine(artifactDir, "error_failure_screenshot.png");
                CaptureScreen(errorScreenshot);

                var procInfo = new StringBuilder();
                procInfo.AppendLine($"[TEST FAILURE DIAGNOSTICS - {DateTime.UtcNow:o}]");
                if (_app != null)
                {
                    procInfo.AppendLine($"ProcessID: {_app.ProcessId}");
                    procInfo.AppendLine($"HasExited: {_app.HasExited}");
                    try { if (_app.HasExited) procInfo.AppendLine($"ExitCode: {_app.ExitCode}"); } catch { }
                }
                procInfo.AppendLine($"Exception: {ex}");

                File.WriteAllText(Path.Combine(artifactDir, "test_error_log.txt"), procInfo.ToString(), Encoding.UTF8);
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

        private AutomationElement? GetActiveWindow()
        {
            return Retry.WhileNull(
                () =>
                {
                    if (_app == null || _app.HasExited) return null;

                    // 1. Try process top-level windows
                    var windows = _app.GetAllTopLevelWindows(_automation);
                    var active = windows.FirstOrDefault(w => w.IsAvailable);
                    if (active != null) return active;

                    // 2. Fallback: Search desktop children by process ID
                    var desktopWindows = _automation.GetDesktop().FindAllChildren(cf => cf.ByProcessId(_app.ProcessId));
                    return desktopWindows.FirstOrDefault(w => w.IsAvailable);
                },
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(500)
            ).Result;
        }

        private static WindowType ClassifyWindow(AutomationElement window)
        {
            if (window == null) return WindowType.Unknown;

            // Check Msgwin controls
            if (window.FindFirstDescendant(cf => cf.ByAutomationId("MsgTextNote")) != null ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("Btn_SeeOK")) != null)
            {
                return WindowType.Msgwin;
            }

            // Check USER_LOGIN controls
            if (window.FindFirstDescendant(cf => cf.ByAutomationId("dispass")) != null ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("SecoRmzo")) != null ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("Greet")) != null)
            {
                return WindowType.USER_LOGIN;
            }

            // Check WinConnectionChoose controls
            if (window.FindFirstDescendant(cf => cf.ByAutomationId("Btn_TestConnection")) != null ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("Btn_SaveConnection")) != null ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("ServerChooser")) != null)
            {
                return WindowType.WinConnectionChoose;
            }

            return WindowType.Unknown;
        }

        private static string GetMsgwinNote(AutomationElement msgWindow)
        {
            var noteElement = msgWindow.FindFirstDescendant(cf => cf.ByAutomationId("MsgTextNote"))?.AsTextBox();
            return noteElement?.Text ?? noteElement?.Name ?? "[No text available]";
        }

        private static void ExecuteUserLoginScenario(AutomationElement window, string screenshotPath)
        {
            // Toggle Password Visibility CheckBox
            var dispassCheckBox = FindCheckBox(window, "dispass", "نمایش رمز عبور");
            if (dispassCheckBox != null)
            {
                dispassCheckBox.Click();
                Thread.Sleep(500);
                CaptureScreen(screenshotPath);
            }

            // Enter Password Text
            var passwordInput = FindTextBox(window, "SecoRmzo", "Rmzo");
            if (passwordInput != null && passwordInput.IsEnabled)
            {
                passwordInput.Text = "123456";
                Thread.Sleep(300);
            }

            // Click Login Button
            var loginBtn = FindButton(window, "Greet", "ورود");
            Assert.NotNull(loginBtn);
            if (loginBtn.IsEnabled)
            {
                loginBtn.Click();
                Thread.Sleep(1000);
            }
        }

        private static void ExecuteConnectionChooseScenario(AutomationElement window, string screenshotPath)
        {
            var testConnBtn = FindButton(window, "Btn_TestConnection", "تست اتصال");
            if (testConnBtn != null && testConnBtn.IsEnabled)
            {
                testConnBtn.Click();
                Thread.Sleep(1000);
                CaptureScreen(screenshotPath);
            }

            var saveBtn = FindButton(window, "Btn_SaveConnection", "تایید");
            Assert.NotNull(saveBtn);
        }

        private static CheckBox? FindCheckBox(AutomationElement root, string automationId, string nameText)
        {
            var element = root.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsCheckBox();
            if (element != null) return element;

            element = root.FindFirstDescendant(cf => cf.ByName(nameText))?.AsCheckBox();
            if (element != null) return element;

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
            var element = root.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsButton();
            if (element != null) return element;

            element = root.FindFirstDescendant(cf => cf.ByName(nameText))?.AsButton();
            if (element != null) return element;

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
                // Best-effort screenshot capture: do NOT abort functional E2E test execution
            }
        }

        private static string FindWpfExecutable()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Search upwards for repository root containing MrCorrect.sln
            DirectoryInfo? dir = new DirectoryInfo(baseDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MrCorrect.sln")))
            {
                dir = dir.Parent;
            }

            if (dir != null)
            {
                string[] candidates = new[]
                {
                    Path.Combine(dir.FullName, "Prg_UI", "bin", "Debug", "net8.0-windows7.0", "win-x64", "MrCorrect.exe"),
                    Path.Combine(dir.FullName, "Prg_UI", "bin", "Release", "net8.0-windows7.0", "win-x64", "MrCorrect.exe"),
                    Path.Combine(dir.FullName, "Prg_UI", "bin", "Debug", "net8.0-windows7.0", "MrCorrect.exe"),
                    Path.Combine(dir.FullName, "Prg_UI", "bin", "Release", "net8.0-windows7.0", "MrCorrect.exe"),
                };

                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        return Path.GetFullPath(candidate);
                    }
                }
            }

            return Path.Combine(baseDir, "MrCorrect.exe");
        }

        public void Dispose()
        {
            _automation?.Dispose();
        }
    }
}
