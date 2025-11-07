using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;
using Prg_UI.SplashScreen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using static Prg_UI.Functions.CL_LMethods;
using System.Collections;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.HelperWins;
using Wins.WinOther;
using Prg_UI.Functions;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Functions;
using System.Reflection;
using System.Globalization;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Stimulsoft.Base;
using Microsoft.Xaml.Behaviors;
using System.Threading.Tasks;
using Prg_UI.Wins.WinSetting;

namespace Prg_UI
{
    public partial class App : Application
    {
        private static bool _isHandlingSqlConnectionFailure;


        #region NEW_ADDED_FOR_FIX
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
        }
        private static System.Reflection.Assembly? ResolveAssemblies(object? sender, ResolveEventArgs args)
        {
            var requested = new AssemblyName(args.Name);
            if (requested.Name == "Microsoft.Xaml.Behaviors")
            {
                return typeof(Behavior).Assembly;
            }
            try
            {
                if (requested.Name.StartsWith("Stimulsoft.", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string searchRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DLLS");
                        if (System.IO.Directory.Exists(searchRoot))
                        {
                            string[] matches = System.IO.Directory.GetFiles(searchRoot, requested.Name + ".dll", System.IO.SearchOption.AllDirectories);
                            if (matches.Length > 0 && System.IO.File.Exists(matches[0]))
                            {
                                return System.Reflection.Assembly.LoadFrom(matches[0]);
                            }
                        }
                    }
                    catch { }
                }
                var _ = typeof(Stimulsoft.Report.StiReport).Assembly.FullName; // Just in case
            }
            catch { }

            return null;
        }
        #endregion
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Perform pre-exit cleanup
            CL_LMethods.CleanupBeforeExiting();

            bool shouldAttemptConnectionRecovery = false;
            bool isRecoveryAlreadyInProgress = false;

            try
            {
                string userMessage = string.Empty;

                // Handle SQL Exceptions
                if (e.Exception is SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 2:
                        case 232: // Server not available
                            userMessage = "سرور در دسترس نیست!.";
                            break;

                        case 4060: // Database not found
                            userMessage = "دیتابیس در دسترس نیست!.";
                            break;

                        case 233: // Connection issue
                        case 10061: //Connection issue
                            userMessage = "وضعیت پایگاه داده تغییر یافته , احتمالا سرویس پایگاه داده متوقف یا اتصال به شبکه قطع شده است !.";
                            break;

                        case 18456: // Login failed
                            userMessage = "احراز هویت پایگاه داده به دلیل خطا انجام نشده , بنا بر این نمیتوان به سرور متصل شد!";
                            break;

                        case -2: // Timeout
                        case 53: // Network error
                        case 26: // Instance error
                            userMessage = "اتصال به سرور پایگاه داده ممکن نیست. لطفاً اتصال شبکه و در دسترس بودن سرور خود را بررسی کنید , همچنین سرویس رو هم بررسی کنید که فعال باشد..";
                            break;

                        case 208: // Invalid object name
                            userMessage = "بعضی از موجودیت های پایگاه داده وجود ندارد , با پشتیبانی در ارتباط باشید";
                            break;


                        default: userMessage = "ارتباط با پایگاه داده دچار مشکل شده است"; break;
                    }

                    bool isConnectionIssue = CL_CCNNMANAGER.IsConnectionRelated(sqlEx);

                    if (isConnectionIssue)
                    {
                        CL_CCNNMANAGER.ConnectedToSQLDB = false;
                    }


                    // Show the message to the user
                    new Msgwin(false, userMessage).ShowDialog();

                    //e.Handled = true; //Prevent application crash // By that error won't stay shown !
                    if (isConnectionIssue)
                    {
                        if (_isHandlingSqlConnectionFailure)
                        {
                            e.Handled = true;
                            isRecoveryAlreadyInProgress = true;
                        }
                        else
                        {
                            _isHandlingSqlConnectionFailure = true;
                            e.Handled = true;
                            shouldAttemptConnectionRecovery = true;
                        }
                    }
                }
                else
                {
                    // Handle non-SQL exceptions
                    userMessage = "“متأسفیم، به نظر می‌رسد که مشکل کوچکی پیش آمده است. برنامه به صورت خودکار بسته خواهد شد. ما از صبر و درک شما سپاسگزاریم.”";
                    try
                    {
                        new Msgwin(false, userMessage).ShowDialog();
                    }
                    catch { }
                    //e.Handled = true;
                }

                //LogException(e.Exception);
            }
            catch (Exception ex)
            {
                // Log any failure within the exception handler itself
                LogException(ex, "Critical error in Unhandled Exception Handler.");
            }

            LogException(e.Exception);

            if (isRecoveryAlreadyInProgress)
            {
                return;
            }

            if (shouldAttemptConnectionRecovery)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        var owner = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
                        var connectionWindow = new WinConnectionChoose();
                        //if (owner != null && !ReferenceEquals(owner, connectionWindow))
                        //{
                        //    connectionWindow.Owner = owner;
                        //}

                        connectionWindow.ShowDialog();
                    }
                    finally
                    {
                        _isHandlingSqlConnectionFailure = false;

                        if (!CL_CCNNMANAGER.ConnectedToSQLDB)
                        {
                            CL_LMethods.CleanupBeforeExiting();
                            CL_LMethods.GoExitTheApplication();
                        }
                    }
                }));

                return;
            }

            CL_LMethods.GoExitTheApplication();

        }

        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);
        private const int GR_GDIOBJECTS = 0;
        private const int GR_USEROBJECTS = 1;


        private static readonly DateTime AppStartTime = DateTime.Now;
        private void LogException(Exception ex, string context = null)
        {
            string filePath = @"C:\Correct\ErrorApplicationMine.txt";

            try
            {
                // Ensure thread-safe writes
                lock (typeof(Application))
                {
                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    var logBuilder = new StringBuilder();
                    string divider = new string('=', 80);
                    string subDivider = new string('-', 80);
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    var process = Process.GetCurrentProcess();
                    //int gdiCount = GetGuiResources(process.Handle, GR_GDIOBJECTS);
                    //int userCount = GetGuiResources(process.Handle, GR_USEROBJECTS);

                    // Header
                    logBuilder.AppendLine($"\n\n\n");
                    logBuilder.AppendLine("------------------------------Error Logging----------------------------------");
                    logBuilder.AppendLine($"Timestamp: {timestamp}");
                    logBuilder.AppendLine(divider);
                    logBuilder.AppendLine($"MrCorrect Full Version: {CL_VERSION.MrCorrectFullVersion}");
                    logBuilder.AppendLine(divider);
                    // Exception Details
                    logBuilder.AppendLine("EXCEPTION DETAILS");
                    logBuilder.AppendLine($"  Type:                 {ex.GetType().FullName}");
                    logBuilder.AppendLine($"  Message:              {ex.Message}");
                    logBuilder.AppendLine($"  Source:               {ex.Source}");
                    logBuilder.AppendLine($"  Target Method:        {ex.TargetSite?.Name}");
                    logBuilder.AppendLine($"  Target FullName:      {ex.TargetSite?.DeclaringType?.FullName}.{ex.TargetSite?.Name}");
                    logBuilder.AppendLine($"  HResult:              {ex.HResult}");
                    logBuilder.AppendLine($"  Inner Exception:      {(ex.InnerException != null ? ex.InnerException.Message : "None")}");
                    logBuilder.AppendLine($"  Help Link:            {(string.IsNullOrEmpty(ex.HelpLink) ? "None" : ex.HelpLink)}");
                    logBuilder.AppendLine("  Stack Trace:");
                    logBuilder.AppendLine(ex.StackTrace);
                    logBuilder.AppendLine(subDivider);

                    // Detailed Exception Data (if any)
                    if (ex.Data != null && ex.Data.Count > 0)
                    {
                        logBuilder.AppendLine("EXCEPTION DATA:");
                        foreach (DictionaryEntry entry in ex.Data)
                        {
                            logBuilder.AppendLine($"  {entry.Key}: {entry.Value}");
                        }
                        logBuilder.AppendLine(subDivider);
                    }
                    // Error Context (if available)
                    if (!string.IsNullOrEmpty(context))
                    {
                        logBuilder.AppendLine("ERROR CONTEXT:");
                        logBuilder.AppendLine($"  {context}");
                        logBuilder.AppendLine(subDivider);
                    }
                    // Additional Details
                    logBuilder.AppendLine("ADDITIONAL DETAILS");
                    logBuilder.AppendLine($"  Method Source:        {MethodBase.GetCurrentMethod()?.Name}");
                    logBuilder.AppendLine($"  Base Exception:       {ex.GetBaseException().Message}");
                    logBuilder.AppendLine($"  Connection String:    {CL_CCNNMANAGER.CONNECTION_STR}");
                    logBuilder.AppendLine(divider);
                    logBuilder.AppendLine();  // Blank line for spacing
                    // Application Information
                    logBuilder.AppendLine("APPLICATION INFORMATION");
                    logBuilder.AppendLine($"  Username:             {Baseknow.UUSER}");
                    logBuilder.AppendLine($"  Active Window:        {Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.GetType().Name}");
                    logBuilder.AppendLine($"  Title of Active Win:  {Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Title}");
                    logBuilder.AppendLine($"  Focused Control:      {Keyboard.FocusedElement}");
                    try { logBuilder.AppendLine($"  Focused Control Name: {Keyboard.FocusedElement.GetType()?.Name}"); } catch { }
                    logBuilder.AppendLine($"  Machine:              {Environment.MachineName}");
                    logBuilder.AppendLine($"  OSVersion:            {Environment.OSVersion}");
                    logBuilder.AppendLine($"  Working Set Memory:   {Environment.WorkingSet / 1024 / 1024} MB");
                    logBuilder.AppendLine($"  Private Memory:       {process.PrivateMemorySize64 / (1024 * 1024)} MB");
                    logBuilder.AppendLine($"  Current User:         {Environment.UserName}");
                    logBuilder.AppendLine($"  App Startup Path:     {AppDomain.CurrentDomain.BaseDirectory}");
                    logBuilder.AppendLine($"  App Domain:           {AppDomain.CurrentDomain.FriendlyName}");
                    logBuilder.AppendLine($"  Dot NET Version:      {Environment.Version}");
                    logBuilder.AppendLine($"  TickCount64:          {TimeSpan.FromMilliseconds(Environment.TickCount64)}");
                    logBuilder.AppendLine($"  App Started At:       {AppStartTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                    logBuilder.AppendLine($"  Processor Count:      {Environment.ProcessorCount}");
                    logBuilder.AppendLine($"  Windows Count:        {Application.Current.Windows.Count}");
                    //logBuilder.AppendLine($"  GDI Handles:          {gdiCount}");
                    //logBuilder.AppendLine($"  USER Handles:         {userCount}");
                    logBuilder.AppendLine(subDivider);

                    // Thread and Culture Information
                    logBuilder.AppendLine("THREAD & CULTURE INFORMATION");
                    logBuilder.AppendLine($"  Thread ID:            {Thread.CurrentThread.ManagedThreadId}");
                    logBuilder.AppendLine($"  Thread Name:          {Thread.CurrentThread.Name ?? "N/A"}");
                    logBuilder.AppendLine($"  Current Culture:      {CultureInfo.CurrentCulture}");
                    logBuilder.AppendLine($"  Current UI Culture:   {CultureInfo.CurrentUICulture}");
                    logBuilder.AppendLine(subDivider);

                    // Assembly Information
                    var entryAssembly = Assembly.GetEntryAssembly();
                    logBuilder.AppendLine("ASSEMBLY INFORMATION");
                    logBuilder.AppendLine($"  Assembly Name:        {entryAssembly?.GetName().Name ?? "N/A"}");
                    logBuilder.AppendLine($"  Assembly Version:     {entryAssembly?.GetName().Version?.ToString() ?? "N/A"}");
                    logBuilder.AppendLine(subDivider);

                    #region NEWY
                    //var stackTrace = new StackTrace(ex, true); // 'true' یعنی اطلاعات pdb یا خط را در صورت موجود بودن هم بگیرد
                    //logBuilder.AppendLine("DETAILED STACK FRAMES:");
                    //for (int i = 0; i < stackTrace.FrameCount; i++)
                    //{
                    //    var frame = stackTrace.GetFrame(i);
                    //    var method = frame?.GetMethod();
                    //    var fileName = frame?.GetFileName();            // نام فایلی که کد در آن قرار دارد
                    //    var lineNumber = frame?.GetFileLineNumber();    // شماره خط
                    //    var colNumber = frame?.GetFileColumnNumber();   // شماره ستون (در صورت دسترسی)
                    //    var methodName = method?.Name;
                    //    var className = method?.DeclaringType?.FullName;

                    //    // جهت تکمیلی حتی می‌توانید پارامترهای متد را نیز دریافت کنید (اگر نیاز دارید)
                    //    var parameters = method?.GetParameters();
                    //    var paramBuilder = new StringBuilder();
                    //    if (parameters?.Length > 0)
                    //    {
                    //        foreach (var param in parameters)
                    //        {
                    //            paramBuilder.Append($"{param.ParameterType.Name} {param.Name}, ");
                    //        }
                    //        // حذف کاما و فاصله اضافی انتهایی در صورت موجود بودن
                    //        if (paramBuilder.Length >= 2)
                    //            paramBuilder.Length -= 2;
                    //    }

                    //    logBuilder.AppendLine($"  Frame #{i}:");
                    //    logBuilder.AppendLine($"    Class:        {className}");
                    //    logBuilder.AppendLine($"    Method:       {methodName}({paramBuilder})");
                    //    logBuilder.AppendLine($"    File:         {fileName}");
                    //    logBuilder.AppendLine($"    Line/Column:  {lineNumber}/{colNumber}");
                    //    logBuilder.AppendLine(subDivider);
                    //}
                    #endregion

                    #region NEW2
                    try
                    {
                        string screenshotPath = ScreenCaptureUtility.CaptureScreenOnError(ex);
                        // Add screenshot information
                        if (!string.IsNullOrEmpty(screenshotPath))
                        {
                            logBuilder.AppendLine("SCREENSHOT INFORMATION");
                            logBuilder.AppendLine($"  Screenshot saved: {screenshotPath}");
                            logBuilder.AppendLine(subDivider);
                        }
                    }
                    catch { }
                    #endregion

                    logBuilder.AppendLine(divider + "End Of Error Log.");
                    // Write the formatted log to file
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.Write(logBuilder.ToString());
                    }
                }
            }
            catch (IOException ioEx)
            {
            }

        }

        public static ISplashScreen splashScreen;
        private ManualResetEvent ResetSplashCreated;
        private Thread SplashThread;
        private async void ShowSplash()
        {
            // Create the window
            WinSplashy animatedSplashScreenWindow = new WinSplashy();
            splashScreen = animatedSplashScreenWindow;

            // Show it
            animatedSplashScreenWindow.Show();

            // Now that the window is created, allow the rest of the startup to run
            ResetSplashCreated.Set();
            System.Windows.Threading.Dispatcher.Run();
        }
        #region STABLE_WINDOWS
        public static SEARCHMENIU_WIN SEARCHMENIU_WIN_ST { get; set; }
        public static void SEARCHMENIU_WIN_ST_Show()
        {
            // Check if the window is already open
            if (App.SEARCHMENIU_WIN_ST == null || !App.SEARCHMENIU_WIN_ST.IsLoaded)
            {
                // If the window is not open, create a new instance and show it
                App.SEARCHMENIU_WIN_ST = new SEARCHMENIU_WIN();
                App.SEARCHMENIU_WIN_ST.Show();
            }
            else
            {
                // If the window is already open, bring it to the foreground
                App.SEARCHMENIU_WIN_ST.Activate();
            }
        }

        #endregion


        #region GlobalHotKeys

        #region DLL Imports
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        // Checks if the application is in the foreground
        private static bool IsAppFocused()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            int foregroundProcessId;
            GetWindowThreadProcessId(foregroundWindow, out foregroundProcessId);

            int currentProcessId = Process.GetCurrentProcess().Id;
            return foregroundProcessId == currentProcessId;
        }

        #endregion

        // Constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // Delegate and Hook ID
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // OnStartup - Set up the global key listener
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule!;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Ensure that we process the event only if nCode >= 0
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN) && CL_Generaly.IsMrCorrectLoadedWinBase)
            {
                // Ensure the application is focused
                if (!IsAppFocused()) return CallNextHookEx(_hookID, nCode, wParam, lParam);

                int vkCode = Marshal.ReadInt32(lParam);
                Key pressedKey = KeyInterop.KeyFromVirtualKey(vkCode);

                // Check modifier keys
                bool ctrlPressed = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                bool altPressed = (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
                bool shiftPressed = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

                //if (ctrlPressed && shiftPressed && vkCode == KeyInterop.VirtualKeyFromKey(Key.F8))

                try
                {
                    if (shiftPressed)
                    {
                        switch (pressedKey)
                        {
                            case Key.F9:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FMENU_TARAZ_4_KOL_FT4, default);
                                });
                                break;
                            case Key.F12:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_ADVANC_F12, default);
                                });
                                break;
                        }
                    }
                    else if (altPressed)
                    {
                        switch (pressedKey)
                        {
                            case Key.F2:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.STUF_DEF_LST, default);
                                });
                                break;
                        }
                    }
                    else if (ctrlPressed) // Handle Ctrl+Key combinations
                    {
                        switch (pressedKey)
                        {
                            case Key.M:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.SQLSTATEFORM_CRTL_M, default);
                                });
                                break;
                            case Key.W:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, default);
                                });
                                break;
                            case Key.E:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, default);
                                });
                                break;
                            case Key.I:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, default);
                                });
                                break;
                            case Key.F8:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    //Ctrl + F8
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL, default);
                                });
                                break;
                            case Key.F7:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KART, default);
                                });
                                break;
                            case Key.D7:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.CHAPCHEK, default);
                                });
                                break;
                            default: break;
                        }
                    }
                    else //if (noModifiers)
                    {
                        switch (pressedKey)
                        {
                            case Key.F2:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.STUF_DEF_WIN, default);
                                });
                                break;

                            case Key.F3:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, default);
                                });
                                break;
                            case Key.F4:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, default);
                                });
                                break;
                            case Key.F5:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, default);
                                });
                                break;
                            case Key.F6:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.FCODE_CUSTOMER, default);
                                });
                                break;
                            case Key.F10:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, default);
                                });
                                break;
                            case Key.F8:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_TAF, default);
                                });
                                break;
                            case Key.F12:
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_SERCH_MAIN_F12, default);
                                });
                                break;
                            default: break;
                        }
                    }
                }
                catch { }
            }
            // Pass the event to the next hook
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            _hookID = SetHook(_proc); //Global HotKeys Initilize

            #region CALL_FROM_MSACCESS
            string[] args = e.Args;

            //string[] defaultArgs = new string[] { "default_value" };
            //args = args.Length > 0 ? args : defaultArgs;
            //args[0] = "Provider=Microsoft.Access.OLEDB.10.0;Persist Security Info=False;Data Source=MERCEDES\\SQL2019;Integrated Security=SSPI;Initial Catalog=YAZDSEPAR1401;Data Provider=SQLOLEDB.1#78#1#1";

            if (args.Length > 0)
            {
                string input = args[0];
                if (!(input is null))
                {
                    if (!string.IsNullOrEmpty(input) && input.Length != 0)
                    {
                        string[] Str_US = input.Split('#');

                        if (string.IsNullOrEmpty(Str_US[0])) { GoExitTheApplication(); }

                        CL_CCNNMANAGER.CONNECTION_STR = CL_CCNNMANAGER.ExtractConnectionString(Str_US[0]); //Connection String 0
                        Baseknow.USERCOD = Convert.ToInt32(Str_US[1]);
                        CL_Generaly.SHIFT_OF_USER = Convert.ToInt32(Str_US[2]);
                        CL_Generaly.VAHED_OF_USER = Convert.ToInt32(Str_US[3]);

                        CL_Generaly.SectionName = Str_US[4].ToString();

                        try
                        {
                            using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                            {
                                db.Open();
                                var results = db.Query<string>("SELECT GETDATE()");

                                CL_CCNNMANAGER.ConnectedToSQLDB = true;

                                List<SALA_DTL> USRLST = null;

                                USRLST = db.Query<SALA_DTL>($"SELECT SAL_NAME, GRSAL, IDD FROM SALA_DTL WHERE IDD = {Baseknow.USERCOD}").ToList();
                                foreach (var item in USRLST)
                                {
                                    item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME.ToString()).Replace("ي", "ی").Replace("ك", "ک");
                                }
                                var USF = USRLST.Where(x => x.SAL_NAME.Equals(USRLST.First().SAL_NAME.Replace("ي", "ی").Replace("ك", "ک"))).FirstOrDefault();
                                Baseknow.UUSER = CL_HESABDARI.Fixp(USRLST.First().SAL_NAME).ToString();
                                Baseknow.UGRP = USF.GRSAL.ToString();

                                CL_Generaly.IsCalledExternally = true;
                                db?.Close();
                            }
                        }
                        catch (Exception)
                        {
                            CL_LMethods.GoExitTheApplication();
                        }
                    }
                }
            }
            #endregion

        

            #region SplashWindowy

            // ManualResetEvent acts as a block.  It waits for a signal to be set.
            ResetSplashCreated = new ManualResetEvent(false);
            // Create a new thread for the splash screen to run on
            SplashThread = new Thread(ShowSplash);
            SplashThread.SetApartmentState(ApartmentState.STA);
            SplashThread.IsBackground = true;
            SplashThread.Name = "Splash Screen";
            SplashThread.Start();
            // Wait for the blocker to be signaled before continuing. This is essentially the same as: while(ResetSplashCreated.NotSet) {}
            ResetSplashCreated.WaitOne();

            #endregion

            //Licenses:
            try
            {
                StiLicense.Key = CL_Generaly.StiLicKey;
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODc4NkAzMjMwMkUzNDJFMzBsa2MvT0xqRTVEaHV1d01nNjUveFFoV2dWbHhhTVBIWVZ4alJjS3ltaVZnPQ==");
            }
            catch { }

            base.OnStartup(e);


            //RDP_CONFIGS
            SetGeneralRdpMode();

        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            CL_LMethods.CleanupBeforeExiting();
        }

        private void SetGeneralRdpMode()
        {
            if (GeneralOptionManager.IsRDPMode) //SystemParameters.IsRemoteSession &&
            {
                //// Reduce framerate universally
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 15 });

                //// Use software rendering instead of hardware acceleration
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                //3 Use solid colors instead of gradients
                //Application.Current.Resources["BackgroundBrush"] = Brushes.White;
            }
        }
    }
}
