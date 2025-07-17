using System;
using System.Windows;
using System.Windows.Interop;

namespace Functions
{
    public static class WindowHandleExtensions
    {
        /// <summary>
        /// Ensures the Window’s HWND is ready, then invokes the action on the UI thread.
        /// </summary>
        //public static void InvokeWhenHandleReady(this Window window, Action<IntPtr> action)
        //{
        //    var helper = new WindowInteropHelper(window);
        //    IntPtr hwnd = helper.Handle;
        //    // اگر Handle حاضر بود، مستقیم اجرا کن:
        //    if (hwnd != IntPtr.Zero)
        //    {
        //        action(hwnd);
        //        return;
        //    }

        //    // وگرنه صبر کن تا SourceInitialized بیاید:
        //    void OnSourceInit(object s, EventArgs e)
        //    {
        //        window.SourceInitialized -= OnSourceInit;
        //        action(helper.Handle);
        //    }

        //    window.SourceInitialized += OnSourceInit;
        //}

        public static void InvokeWhenHandleReady(this Window window, Action<IntPtr> action, int timeoutMilliseconds = 6000)
        {
            var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.Handle;

            // If handle is already available, call the action immediately
            if (hwnd != IntPtr.Zero)
            {
                action(hwnd);
                return;
            }

            // Start a timeout mechanism, and track the elapsed time
            var timeout = DateTime.Now.AddMilliseconds(timeoutMilliseconds);

            void OnSourceInit(object sender, EventArgs e)
            {
                // If the timeout is exceeded, stop waiting
                if (DateTime.Now > timeout)
                {
                    Console.WriteLine("Window handle initialization timed out.");
                    window.SourceInitialized -= OnSourceInit;
                    return;
                }

                window.SourceInitialized -= OnSourceInit; // Unsubscribe after initialization
                action(helper.Handle); // Perform the action with the handle
            }

            window.SourceInitialized += OnSourceInit; // Subscribe to the SourceInitialized event
        }


    }
}
