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
            WinBase,
            ProformaInvoice,
            Msgwin,
            Splash,
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

            WriteRuntimeDiagnostics(diagPath, exePath);

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

                // Handle Splash screen (WinSplashy)
                activeWindow = HandleSplashScreenIfPresent(activeWindow, ref windowType, startupMsgPath, treeDumpPath, step1Screenshot);

                // Handle Msgwin dialogs
                if (activeWindow != null)
                {
                    activeWindow = HandleMsgwinDialogsIfPresent(activeWindow, ref windowType, startupMsgPath, treeDumpPath, step1Screenshot);
                }

                if (activeWindow == null)
                {
                    string procInfo = _app != null ? $"ProcessID: {_app.ProcessId}, HasExited: {_app.HasExited}" : "App process is null";
                    throw new InvalidOperationException($"Active top-level window became null after startup window transitions. ({procInfo})");
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
                        ExecuteUserLoginScenario(activeWindow, step2Screenshot, startupMsgPath, treeDumpPath);
                        break;

                    case WindowType.WinConnectionChoose:
                        ExecuteConnectionChooseScenario(activeWindow, step2Screenshot, startupMsgPath, treeDumpPath);
                        break;

                    case WindowType.WinBase:
                        File.AppendAllText(startupMsgPath, $"[{DateTime.UtcNow:o}] WINBASE DASHBOARD ALREADY ACTIVE\n", Encoding.UTF8);
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

                // 7. Verify post-interaction active window is operational
                AutomationElement? finalWindow = GetActiveWindow();
                Assert.NotNull(finalWindow);
            }
            catch (Exception ex)
            {
                HandleTestFailure(artifactDir, ex);
                throw;
            }
            finally
            {
                CleanupAppProcess();
            }
        }

        [Fact]
        public void Test_Wpf_Application_OpenProformaInvoiceScreen()
        {
            string artifactDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestArtifacts_Proforma");
            Directory.CreateDirectory(artifactDir);

            string treeDumpPath = Path.Combine(artifactDir, "proforma_ui_tree_dump.txt");
            string logPath = Path.Combine(artifactDir, "proforma_navigation_log.txt");
            string diagPath = Path.Combine(artifactDir, "proforma_runtime_diagnostics.txt");
            string step1Screenshot = Path.Combine(artifactDir, "1_proforma_start.png");
            string step2Screenshot = Path.Combine(artifactDir, "2_proforma_menu_opened.png");
            string step3Screenshot = Path.Combine(artifactDir, "3_proforma_screen_opened.png");

            string exePath = FindWpfExecutable();
            Assert.True(File.Exists(exePath), $"WPF executable not found at path: {exePath}");

            WriteRuntimeDiagnostics(diagPath, exePath);

            try
            {
                // 1. Launch real MrCorrect.exe process
                var psi = new System.Diagnostics.ProcessStartInfo(exePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(exePath)
                };
                _app = Application.Launch(psi);

                // 2. Attach UIA3 Automation & Get Initial Active Window
                AutomationElement? activeWindow = GetActiveWindow();
                Assert.NotNull(activeWindow);

                WindowType windowType = ClassifyWindow(activeWindow);
                activeWindow = HandleSplashScreenIfPresent(activeWindow, ref windowType, logPath, treeDumpPath, step1Screenshot);
                if (activeWindow != null)
                {
                    activeWindow = HandleMsgwinDialogsIfPresent(activeWindow, ref windowType, logPath, treeDumpPath, step1Screenshot);
                }
                Assert.NotNull(activeWindow);

                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Initial Active Window Classified: {windowType}\n", Encoding.UTF8);

                // 3. Handle Authentication if USER_LOGIN or WinConnectionChoose is active
                if (windowType == WindowType.USER_LOGIN)
                {
                    File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Authenticating through USER_LOGIN...\n", Encoding.UTF8);
                    ExecuteUserLoginScenario(activeWindow, step1Screenshot, logPath, treeDumpPath);

                    activeWindow = GetActiveWindow();
                    Assert.NotNull(activeWindow);
                    windowType = ClassifyWindow(activeWindow);
                    activeWindow = HandleMsgwinDialogsIfPresent(activeWindow, ref windowType, logPath, treeDumpPath, step1Screenshot);
                    Assert.NotNull(activeWindow);
                }
                else if (windowType == WindowType.WinConnectionChoose)
                {
                    File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Testing database connection in WinConnectionChoose...\n", Encoding.UTF8);
                    ExecuteConnectionChooseScenario(activeWindow, step1Screenshot, logPath, treeDumpPath);

                    activeWindow = GetActiveWindow();
                    Assert.NotNull(activeWindow);
                    windowType = ClassifyWindow(activeWindow);
                }

                // 4. Perform Dynamic Navigation to Proforma Invoice Screen ("پیش فاکتور")
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Navigating to Proforma Invoice Screen...\n", Encoding.UTF8);
                AutomationElement? proformaWindow = NavigateToProformaInvoice(activeWindow, logPath, treeDumpPath, step2Screenshot);

                Assert.NotNull(proformaWindow);
                WindowType finalWindowType = ClassifyWindow(proformaWindow);

                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Target Window Reached. Type: {finalWindowType}, Name: '{proformaWindow.Name}', ClassName: '{proformaWindow.ClassName}'\n", Encoding.UTF8);

                using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                {
                    writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR PROFORMA INVOICE SCREEN ===");
                    DumpAutomationTree(proformaWindow, writer, 0);
                }

                CaptureScreen(step3Screenshot);

                // 5. Assert that Proforma Invoice screen actually opened
                bool isProformaOpened = finalWindowType == WindowType.ProformaInvoice ||
                                        (proformaWindow.Name != null && proformaWindow.Name.Contains("پیش فاکتور")) ||
                                        proformaWindow.ClassName == "HEAD_LST_PISHFROOSH2";

                Assert.True(isProformaOpened, $"Failed to open Proforma Invoice screen. Active window Name: '{proformaWindow.Name}', ClassName: '{proformaWindow.ClassName}'");
            }
            catch (Exception ex)
            {
                HandleTestFailure(artifactDir, ex);
                throw;
            }
            finally
            {
                CleanupAppProcess();
            }
        }

        private AutomationElement? NavigateToProformaInvoice(AutomationElement activeWindow, string logPath, string treeDumpPath, string screenshotPath)
        {
            // 1. Try direct trigger if button 'Pishcafor' or 'Btn_PreInvoce' is already accessible on active window
            var directPishcaforBtn = FindButton(activeWindow, "Pishcafor", "پیش فاکتور") ??
                                     FindButton(activeWindow, "Btn_PreInvoce", "پیش فاکتور");

            if (directPishcaforBtn != null && directPishcaforBtn.IsEnabled)
            {
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Direct 'پیش فاکتور' button found and enabled. Clicking...\n", Encoding.UTF8);
                directPishcaforBtn.Click();
                Thread.Sleep(2500);

                var targetWindow = GetActiveWindow();
                if (targetWindow != null && (ClassifyWindow(targetWindow) == WindowType.ProformaInvoice || (targetWindow.Name != null && targetWindow.Name.Contains("پیش فاکتور"))))
                {
                    return targetWindow;
                }
            }

            // 2. Open Sidebar Navigation Panel ('OpenerPnl')
            var openerBtn = FindButton(activeWindow, "OpenerPnl", "منو") ??
                            activeWindow.FindFirstDescendant(cf => cf.ByAutomationId("OpenerPnl"))?.AsButton();

            if (openerBtn != null && openerBtn.IsEnabled)
            {
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Clicking sidebar OpenerPnl button...\n", Encoding.UTF8);
                openerBtn.Click();
                Thread.Sleep(1000);
                CaptureScreen(screenshotPath);
            }

            // 3. Locate Sales Expander ('KharidFurush_Expan' / 'خرید و فروش')
            var salesExpander = activeWindow.FindFirstDescendant(cf => cf.ByAutomationId("KharidFurush_Expan")) ??
                                activeWindow.FindFirstDescendant(cf => cf.ByName("خرید و فروش"));

            if (salesExpander != null)
            {
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Sales expander ('خرید و فروش') found. Clicking to expand...\n", Encoding.UTF8);
                salesExpander.Click();
                Thread.Sleep(800);
            }

            // 4. Click 'Pishcafor' ("پیش فاکتور") button inside sidebar or window
            var pishcaforSidebarBtn = activeWindow.FindFirstDescendant(cf => cf.ByAutomationId("Pishcafor"))?.AsButton() ??
                                      FindButton(activeWindow, "Pishcafor", "پیش فاکتور") ??
                                      FindButton(activeWindow, "Btn_PreInvoce", "پیش فاکتور");

            if (pishcaforSidebarBtn != null && pishcaforSidebarBtn.IsEnabled)
            {
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Clicking 'Pishcafor' ('پیش فاکتور') sidebar button...\n", Encoding.UTF8);
                pishcaforSidebarBtn.Click();
                Thread.Sleep(3000);
            }
            else
            {
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] 'Pishcafor' button not found directly. Searching all buttons for 'پیش فاکتور'...\n", Encoding.UTF8);
                var allButtons = activeWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
                foreach (var btn in allButtons)
                {
                    if (btn.Name != null && btn.Name.Contains("پیش فاکتور") && btn.IsEnabled)
                    {
                        File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] Found button with text '{btn.Name}'. Clicking...\n", Encoding.UTF8);
                        btn.AsButton().Click();
                        Thread.Sleep(3000);
                        break;
                    }
                }
            }

            // 5. Detect newly opened top-level Proforma Invoice window
            AutomationElement? newlyOpenedWindow = Retry.WhileNull(
                () =>
                {
                    if (_app == null || _app.HasExited) return null;
                    var windows = _app.GetAllTopLevelWindows(_automation);
                    return windows.FirstOrDefault(w => w.IsAvailable && (ClassifyWindow(w) == WindowType.ProformaInvoice || (w.Name != null && w.Name.Contains("پیش فاکتور"))));
                },
                TimeSpan.FromSeconds(15),
                TimeSpan.FromMilliseconds(500)
            ).Result ?? GetActiveWindow();

            return newlyOpenedWindow;
        }

        private AutomationElement? HandleSplashScreenIfPresent(AutomationElement activeWindow, ref WindowType windowType, string logPath, string treeDumpPath, string screenshotPath)
        {
            if (windowType == WindowType.Splash)
            {
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] SPLASH SCREEN DETECTED (WinSplashy)\n", Encoding.UTF8);

                using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                {
                    writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR SPLASH SCREEN (WinSplashy) ===");
                    DumpAutomationTree(activeWindow, writer, 0);
                }

                CaptureScreen(screenshotPath);

                var nextWindow = Retry.WhileNull(
                    () =>
                    {
                        if (_app == null || _app.HasExited) return null;
                        var windows = _app.GetAllTopLevelWindows(_automation);
                        return windows.FirstOrDefault(w => w.IsAvailable && ClassifyWindow(w) != WindowType.Splash);
                    },
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromMilliseconds(500)
                ).Result ?? GetActiveWindow();

                if (nextWindow != null)
                {
                    windowType = ClassifyWindow(nextWindow);
                    return nextWindow;
                }
            }
            return activeWindow;
        }

        private AutomationElement? HandleMsgwinDialogsIfPresent(AutomationElement activeWindow, ref WindowType windowType, string logPath, string treeDumpPath, string screenshotPath)
        {
            while (windowType == WindowType.Msgwin && activeWindow != null)
            {
                string msgNote = GetMsgwinNote(activeWindow);
                File.AppendAllText(logPath, $"[{DateTime.UtcNow:o}] MSGWIN DETECTED: {msgNote}\n", Encoding.UTF8);

                using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                {
                    writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR MSGWIN ({msgNote}) ===");
                    DumpAutomationTree(activeWindow, writer, 0);
                }

                CaptureScreen(screenshotPath);

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

            return activeWindow;
        }

        private AutomationElement? GetActiveWindow()
        {
            return Retry.WhileNull(
                () =>
                {
                    if (_app == null || _app.HasExited) return null;

                    var windows = _app.GetAllTopLevelWindows(_automation);
                    var active = windows.FirstOrDefault(w => w.IsAvailable);
                    if (active != null) return active;

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

            // Check WinSplashy controls
            if (window.FindFirstDescendant(cf => cf.ByAutomationId("RDP_LABEL")) != null ||
                window.ClassName == "WinSplashy" ||
                window.Name == "WinSplashy")
            {
                return WindowType.Splash;
            }

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

            // Check Proforma Invoice (HEAD_LST_PISHFROOSH2)
            if (window.ClassName == "HEAD_LST_PISHFROOSH2" ||
                window.Name == "HEAD_LST_PISHFROOSH2" ||
                (window.Name != null && window.Name.Contains("پیش فاکتور")) ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("NUMBER_LBL")) != null)
            {
                return WindowType.ProformaInvoice;
            }

            // Check WinBase (Main Dashboard)
            if (window.ClassName == "WinBase" ||
                window.Name == "مستر کارکت" ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("OpenerPnl")) != null ||
                window.FindFirstDescendant(cf => cf.ByAutomationId("Pishcafor")) != null)
            {
                return WindowType.WinBase;
            }

            return WindowType.Unknown;
        }

        private static string GetMsgwinNote(AutomationElement msgWindow)
        {
            var noteElement = msgWindow.FindFirstDescendant(cf => cf.ByAutomationId("MsgTextNote"))?.AsTextBox();
            return noteElement?.Text ?? noteElement?.Name ?? "[No text available]";
        }

        private void ExecuteUserLoginScenario(AutomationElement window, string screenshotPath, string startupMsgPath, string treeDumpPath)
        {
            var dispassCheckBox = FindCheckBox(window, "dispass", "نمایش رمز عبور");
            if (dispassCheckBox != null)
            {
                dispassCheckBox.Click();
                Thread.Sleep(500);
                CaptureScreen(screenshotPath);
            }

            var passwordInput = FindTextBox(window, "SecoRmzo", "Rmzo");
            if (passwordInput != null && passwordInput.IsEnabled)
            {
                passwordInput.Text = "123456";
                Thread.Sleep(300);
            }

            var loginBtn = FindButton(window, "Greet", "ورود");
            Assert.NotNull(loginBtn);
            Assert.True(loginBtn.IsEnabled, "Login button ('Greet') is disabled.");

            loginBtn.Click();
            Thread.Sleep(2000);

            AutomationElement? postLoginWindow = GetActiveWindow();
            Assert.NotNull(postLoginWindow);

            if (ClassifyWindow(postLoginWindow) == WindowType.Msgwin)
            {
                string loginMsgNote = GetMsgwinNote(postLoginWindow);
                File.AppendAllText(startupMsgPath, $"[{DateTime.UtcNow:o}] LOGIN RESULT MSGWIN: {loginMsgNote}\n", Encoding.UTF8);

                using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                {
                    writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR LOGIN RESULT MSGWIN ({loginMsgNote}) ===");
                    DumpAutomationTree(postLoginWindow, writer, 0);
                }

                Assert.False(string.IsNullOrWhiteSpace(loginMsgNote), "Login result Msgwin note was empty.");

                var seeOkBtn = FindButton(postLoginWindow, "Btn_SeeOK", "تایید") ?? FindButton(postLoginWindow, "Btn_yes", "بله");
                if (seeOkBtn != null && seeOkBtn.IsEnabled)
                {
                    seeOkBtn.Click();
                    Thread.Sleep(1000);
                }
            }
        }

        private void ExecuteConnectionChooseScenario(AutomationElement window, string screenshotPath, string startupMsgPath, string treeDumpPath)
        {
            var testConnBtn = FindButton(window, "Btn_TestConnection", "تست اتصال");
            Assert.NotNull(testConnBtn);
            Assert.True(testConnBtn.IsEnabled, "Btn_TestConnection button is disabled.");

            testConnBtn.Click();
            Thread.Sleep(2000);
            CaptureScreen(screenshotPath);

            AutomationElement? resultMsgWin = GetActiveWindow();
            if (resultMsgWin != null && ClassifyWindow(resultMsgWin) == WindowType.Msgwin)
            {
                string resultNote = GetMsgwinNote(resultMsgWin);
                File.AppendAllText(startupMsgPath, $"[{DateTime.UtcNow:o}] CONNECTION TEST RESULT MSGWIN: {resultNote}\n", Encoding.UTF8);

                using (var writer = new StreamWriter(treeDumpPath, true, Encoding.UTF8))
                {
                    writer.WriteLine($"\n=== UIA3 AUTOMATION TREE DUMP FOR CONNECTION TEST MSGWIN ({resultNote}) ===");
                    DumpAutomationTree(resultMsgWin, writer, 0);
                }

                Assert.False(string.IsNullOrWhiteSpace(resultNote), "Connection test Msgwin note was empty.");

                var seeOkBtn = FindButton(resultMsgWin, "Btn_SeeOK", "تایید") ?? FindButton(resultMsgWin, "Btn_yes", "بله");
                if (seeOkBtn != null && seeOkBtn.IsEnabled)
                {
                    seeOkBtn.Click();
                    Thread.Sleep(1000);
                }
            }

            var activeWindow = GetActiveWindow();
            Assert.NotNull(activeWindow);
            var saveBtn = FindButton(activeWindow, "Btn_SaveConnection", "تایید");
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

        private static void WriteRuntimeDiagnostics(string diagPath, string exePath)
        {
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
        }

        private static string FindWpfExecutable()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

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

        private void HandleTestFailure(string artifactDir, Exception ex)
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
        }

        private void CleanupAppProcess()
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

        public void Dispose()
        {
            _automation?.Dispose();
        }
    }
}
