using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using System.Diagnostics;
using Prg_SendInvoice.CNNMANAGER;
using Prg_Proccessy.Generaly;

namespace AUTO_BAZ.Functions
{
    public static class CL_LMethods
    {
        public class LogWriter
        {
            private static readonly object lockObject = new object();
            private static string logFolderPath = @"C:\CORRECT\AUTO_BAZ_LOG";
            private const long maxFolderSizeBytes = 1024 * 1024 * 500; // 500 mb
            private static string currentLogFileName = "";

            public static void WriteLog(string message, string _path = "C:\\CORRECT\\AUTO_BAZ_LOG")
            {
                try
                {
                    string logFileName = CL_Generaly.General_Servername.Replace("\"", "_").Replace("\\", "_") + "__" + CL_Generaly.General_DBname + "_" + $"{DateTime.Now:yyyy-MM-dd}.txt";
                    string logFilePath = Path.Combine(logFolderPath, logFileName);

                    if (!Directory.Exists(logFolderPath))
                    {
                        Directory.CreateDirectory(logFolderPath);
                    }

                    lock (lockObject)
                    {
                        if (!logFileName.Equals(currentLogFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            currentLogFileName = logFileName;
                            CheckAndDeleteOldLogFiles();
                        }

                        using (StreamWriter writer = File.AppendText(logFilePath))
                        {
                            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff");
                            writer.WriteLine($"{{\n   time: {timestamp},\n   message: {message}\n}}\n");
                        }
                    }
                }
                catch (Exception ex)
                {

                    try { File.WriteAllText("C:\\CORRECT\\AUTO_BAZ_LOG\\Exception_" + CL_Generaly.General_Servername.Replace("\"", "_").Replace("\\", "_") + "_" + CL_Generaly.General_DBname + ".txt", ex.Message + "[[[" + CL_CCNNMANAGER.CONNECTION_STR + " ]]] "); } catch { }
                    try { ExpectionLogWriter.WriteLog(ex, ""); } catch { }
                    Console.WriteLine($"Error writing to log: {ex.Message}");
                }
            }

            private static void CheckAndDeleteOldLogFiles()
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(logFolderPath);
                    if (directoryInfo.Exists)
                    {
                        long folderSizeBytes = directoryInfo.GetFiles().Sum(file => file.Length);

                        if (folderSizeBytes >= maxFolderSizeBytes)
                        {
                            var logFiles = directoryInfo.GetFiles("*.txt")
                                .OrderBy(file => file.CreationTime)
                                .ToList();

                            int filesToDelete = logFiles.Count - 50;

                            for (int i = 0; i < filesToDelete; i++)
                            {
                                logFiles[i].Delete();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
       
        public class ExpectionLogWriter
        {
            public static void WriteLog(Exception er, string tittle, string _path = "C:\\CORRECT\\AUTO_BAZ_LOG\\MatterLog.txt")
            {
                if (!Directory.Exists("C:\\CORRECT\\AUTO_BAZ_LOG")) { Directory.CreateDirectory("C:\\CORRECT\\AUTO_BAZ_LOG"); }

                if (_path == "C:\\CORRECT\\AUTO_BAZ_LOG\\MatterLog.txt")
                {
                    _path = "C:\\CORRECT\\AUTO_BAZ_LOG\\" + $"MatterLog_{CL_Generaly.General_Servername.Replace("\"", "_").Replace("\\", "_") + "_" + CL_Generaly.General_DBname}.txt";
                }

                string method_source = System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "Unknown";
                string methodName = er.TargetSite?.Name ?? "Unknown";
                Exception baseException = er.GetBaseException();
                IDictionary data = er.Data;
                string helpLink = er.HelpLink;


                File.AppendAllText(_path, $"{tittle} : " +
                    $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source} \n method_source : {method_source}" +
                    $"\n Method Name: {methodName} \n Base Exception: {er.GetBaseException().Message} \n Exception Data: {er.Data}" +
                    $"\n Help Link: {er.HelpLink} \n  ExceptionType: {er.GetType().FullName} \n" +
                    $"[[[ {CL_CCNNMANAGER.CONNECTION_STR} ]]]");

                var stackTrace = new StackTrace(er, true);
                var allFrames = stackTrace.GetFrames().ToList();
                StringBuilder logmsg = new StringBuilder();
                //foreach (var frame in allFrames)
                //{
                //    logmsg.AppendLine($"FileName : {frame.GetFileName()}");
                //    logmsg.AppendLine($"LineNumber : {frame.GetFileLineNumber()}");
                //    logmsg.AppendLine($"method : {frame.GetMethod()}");
                //    logmsg.AppendLine($"method name : {frame.GetMethod().Name}");
                //    logmsg.AppendLine($"ClassName : {frame.GetMethod().DeclaringType.ToString()}");
                //    logmsg.AppendLine(); // for an extra line space
                //}
                File.AppendAllText(_path, logmsg.ToString());
            }
        }

        public static string ToStringNullSafe(this object value)
        {
            return (value ?? string.Empty).ToString();
        }
        public static void GoExitTheApplication()
        {
            var dispatcher = Application.Current.Dispatcher;

            if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                    return;

                Application.Current.Shutdown();

                //Commented because it may leads some error before cleaning up !
                //try { System.Environment.Exit(0); } catch { } //Just in case
            }));
        }
        public static void AutoScrollToCurrentItem(ListBox listBox, int index)
        {
            // Find a container
            UIElement container = null;
            for (int i = index; i > 0; i--)
            {
                container = listBox.ItemContainerGenerator.ContainerFromIndex(i) as UIElement;
                if (container != null)
                {
                    break;
                }
            }
            if (container == null)
                return;

            // Find the ScrollContentPresenter
            ScrollContentPresenter presenter = null;
            for (Visual vis = container; vis != null && vis != listBox; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if ((presenter = vis as ScrollContentPresenter) != null)
                    break;
            if (presenter == null)
                return;

            // Find the IScrollInfo
            var scrollInfo =
                !presenter.CanContentScroll ? presenter :
                presenter.Content as IScrollInfo ??
                FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo ??
                presenter;

            // Find the amount of items that is "Visible" in the ListBox
            var height = (container as ListBoxItem).ActualHeight;
            var lbHeight = listBox.ActualHeight;
            var showCount = (int)(lbHeight / height) - 1;

            //Set the scrollbar
            if (scrollInfo.CanVerticallyScroll)
                scrollInfo.SetVerticalOffset(index - showCount);
        }

        private static DependencyObject FirstVisualChild(Visual visual)
        {
            if (visual == null) return null;
            if (VisualTreeHelper.GetChildrenCount(visual) == 0) return null;
            return VisualTreeHelper.GetChild(visual, 0);
        }

        public static void SendKey_US(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);

                    // Note: Based on your requirements you may also need to fire events for:
                    // RoutedEvent = Keyboard.PreviewKeyDownEvent
                    // RoutedEvent = Keyboard.KeyUpEvent
                    // RoutedEvent = Keyboard.PreviewKeyUpEvent
                }
            }
        }
    }
}
