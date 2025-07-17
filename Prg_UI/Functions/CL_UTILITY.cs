using System;
using System.Windows.Threading;

namespace Functions
{
    public class CL_UTILITY
    {
        public class DispatcherHelper
        {
            #region EXAMPLE_USE
            //var TopDispatcher = new DispatcherHelper(this.Dispatcher);
            //TopDispatcher.RunWithTopPriority(() =>
            //{
            //});
            #endregion

            private readonly Dispatcher _dispatcher;

            /// <summary>
            /// Initializes the helper with the given Dispatcher.
            /// </summary>
            /// <param name="dispatcher">The dispatcher associated with the UI thread.</param>
            public DispatcherHelper(Dispatcher dispatcher)
            {
                _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher), "Dispatcher cannot be null.");
            }

            /// <summary>
            /// Executes an action on the UI thread with topmost priority.
            /// </summary>
            /// <param name="action">The action to execute.</param>
            public void RunWithTopPriority(Action action)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action), "Action cannot be null.");


                if (_dispatcher.CheckAccess())
                {
                    // Already on the UI thread, execute immediately
                    action();
                }
                else
                {
                    // Dispatch the action with topmost priority
                    _dispatcher.Invoke(action, DispatcherPriority.Send);
                }

                //try
                //{
                //}
                //catch (Exception ex)
                //{
                //    // Log or handle exceptions here (customize this as needed)
                //    LogException(ex);
                //    throw; // Re-throw the exception if further handling is required
                //}
            }

            /// <summary>
            /// Logs exceptions for debugging purposes.
            /// </summary>
            /// <param name="ex">The exception to log.</param>
            private void LogException(Exception ex)
            {
                string logMessage = $"[{DateTime.Now}] Exception: {ex.Message}\nStack Trace: {ex.StackTrace}\n";
                //System.IO.File.AppendAllText("DispatcherErrors.log", logMessage);
            }
        }


    }
}
