using AUTO_BAZ.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AUTO_BAZ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Exit += App_Exit;
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            currentProcess.EnableRaisingEvents = true;
            currentProcess.Exited += CurrentProcess_Exited;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            DelayedDurabilityGuard.TryDisableForcefully();
        }

        private void CurrentProcess_Exited(object? sender, EventArgs e)
        {
            DelayedDurabilityGuard.TryDisableForcefully();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DelayedDurabilityGuard.TryDisableForcefully();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                DelayedDurabilityGuard.TryDisableForcefully();

                var _er = e.Exception + Environment.NewLine + e.Handled + Environment.NewLine + e.Dispatcher;
                File.WriteAllText(@"C:\CORRECT\AUTO_BAZ_LOG\AUTO_EXCP.txt", _er);

                var er = e.Exception;
                string method_source = System.Reflection.MethodBase.GetCurrentMethod().Name;
                string methodName = er.TargetSite.Name;
                Exception baseException = er.GetBaseException();
                IDictionary data = er.Data;
                string helpLink = er.HelpLink;

                File.AppendAllText(@"C:\CORRECT\AUTO_BAZ_LOG\AUTO_EXCP.txt", $"UnhandledException : " +
                    $"{er.Message} \n {er.InnerException} \n {er.StackTrace} \n {er.Source} \n method_source : {method_source}" +
                    $"\n Method Name: {er.TargetSite.Name} \n Base Exception: {er.GetBaseException().Message} \n Exception Data: {er.Data}" +
                    $"\n Help Link: {er.HelpLink} \n  ExceptionType: {er.GetType().FullName} \n");

                var stackTrace = new StackTrace(er, true);
                var allFrames = stackTrace.GetFrames().ToList();
                StringBuilder logmsg = new StringBuilder();
                foreach (var frame in allFrames)
                {
                    logmsg.AppendLine($"FileName : {frame.GetFileName()}");
                    logmsg.AppendLine($"LineNumber : {frame.GetFileLineNumber()}");
                    logmsg.AppendLine($"method : {frame.GetMethod()}");
                    logmsg.AppendLine($"method name : {frame.GetMethod().Name}");
                    logmsg.AppendLine($"ClassName : {frame.GetMethod().DeclaringType.ToString()}");
                    logmsg.AppendLine(); // for an extra line space
                }
                File.AppendAllText(@"C:\CORRECT\AUTO_BAZ_LOG\AUTO_EXCP.txt", logmsg.ToString());
            }
            catch { }
        }
    }
}
