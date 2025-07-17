using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wins.WinOther;

namespace Prg_UI.Functions
{
    public static class DataGridExtension
    {
        public static readonly DependencyProperty EnableSearchProperty =
            DependencyProperty.RegisterAttached(
                "EnableSearch",
                typeof(bool),
                typeof(DataGridExtension),
                new PropertyMetadata(false, OnEnableSearchChanged));

        public static void SetEnableSearch(DataGrid element, bool value)
        {
            element.SetValue(EnableSearchProperty, value);
        }

        public static bool GetEnableSearch(DataGrid element)
        {
            return (bool)element.GetValue(EnableSearchProperty);
        }
        public static void HandleKeyPress(object sender, KeyEventArgs e, DataGrid dataGrid)
        {
            // Check if F7 (or another specific key) is pressed
            if (e.Key == Key.F7 && dataGrid.IsEnabled)
            {
                // Open the SearchWindow when F7 is pressed
                OpenSearchWindow(dataGrid);
            }
        }
        // This method will open the SearchWindow
        private static void OpenSearchWindow(DataGrid dataGrid)
        {
            // Assuming you are passing the DataGrid to the SearchWindow
            var searchWindow = new SearchWindow(dataGrid);
            searchWindow.ShowDialog();
        }
        private static void OnEnableSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is not DataGrid dataGrid) return;

            //// Open the search window when search is enabled
            //if ((bool)e.NewValue)
            //{
            //    var searchWindow = new SearchWindow(dataGrid);
            //    searchWindow.ShowDialog();
            //}
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}