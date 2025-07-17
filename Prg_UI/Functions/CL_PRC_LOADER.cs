using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Prg_UI.Functions
{
    public static class CL_PRC_LOADER
    {
        //private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<int, Process> _runningProcesses = new ConcurrentDictionary<int, Process>();
        private static readonly List<string> _tempExePaths = new List<string>();
        private static IntPtr _preloaderWindowHandle = IntPtr.Zero;

        // Import necessary Windows API functions
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static bool HasDisposed = false;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static async Task<Process> StartPreloader(string resourceName = null, string arguments = null)
        {
            _runningProcesses?.Clear(); //for safety

            resourceName ??= "Prg_UI.UiDrive.EXEFILES.Preloader.exe";
            arguments ??= "1354"; // Default argument

            using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException("Resource not found.");

                byte[] exeBytes = new byte[stream.Length];
                await stream.ReadAsync(exeBytes, 0, exeBytes.Length);

                string tempExePath = Path.GetTempFileName() + ".exe";
                _tempExePaths.Add(tempExePath);
                File.WriteAllBytes(tempExePath, exeBytes);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = tempExePath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                _runningProcesses.TryAdd(process.Id, process);

                await Task.Delay(600); // Asynchronous wait
                _preloaderWindowHandle = FindWindow(null, "PreloaderWindowTitle");

                if (_preloaderWindowHandle != IntPtr.Zero)
                    ShowWindow(_preloaderWindowHandle, SW_HIDE); // Hide the window initially

                return process;
            }
        }
        public static void ShowPreloader()
        {
            if (HasDisposed) { return; }

            if (!_runningProcesses.Any() || _runningProcesses.Values.FirstOrDefault()?.HasExited == true)
            {
                _ = StartPreloader();
            }
            else if (_preloaderWindowHandle != IntPtr.Zero)
            {
                ShowWindow(_preloaderWindowHandle, SW_SHOW); // Show the window
            }
        }

        public static void HidePreloader()
        {
            if (HasDisposed) { return; }

            if (_preloaderWindowHandle != IntPtr.Zero)
            {
                ShowWindow(_preloaderWindowHandle, SW_HIDE); // Hide the window
            }
        }

        public static void Stop(Process process)
        {
            if (process == null)
            {
                return;
            }
            if (_runningProcesses.TryRemove(process.Id, out var removedProcess))
            {
                removedProcess.Kill();
                removedProcess.WaitForExit();
                removedProcess.Dispose();

                // Clean up temporary file
                string tempExePath = _tempExePaths.FirstOrDefault(p => string.Equals(p, removedProcess.StartInfo.FileName, StringComparison.OrdinalIgnoreCase));
                if (tempExePath != null)
                {
                    try
                    {
                        File.Delete(tempExePath);
                        _tempExePaths.Remove(tempExePath);
                    }
                    catch (Exception)
                    {
                        // Handle the exception as needed
                    }
                }
            }
        }

        private static void ClearRunningProcesses()
        {
            foreach (var process in _runningProcesses.Values.ToList())
            {
                if (process.HasExited)
                {
                    Stop(process);
                }
            }
        }

        public static void Dispose()
        {
            foreach (var process in _runningProcesses.Values)
            {
                Stop(process);
            }

            HasDisposed = true;
        }
    }
}
