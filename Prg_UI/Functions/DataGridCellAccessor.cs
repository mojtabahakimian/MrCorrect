using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.IO;
using System.Windows.Controls.Primitives;

namespace Functions
{
    /// <summary>
    /// Provides secure, high-performance access to DataGrid cells with financial data handling capabilities
    /// </summary>
    public static class DataGridCellAccessor
    {
        #region Configuration Fields
        private static readonly ConcurrentDictionary<Tuple<DataGrid, string>, DataGridColumn>
            _columnCache = new ConcurrentDictionary<Tuple<DataGrid, string>, DataGridColumn>();

        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars()
            .Concat(new[] { ':', '*', '?', '"', '<', '>', '|' }).ToArray();
        #endregion

        #region Core Access Methods

        /// <summary>
        /// Safely retrieves cell content with full security validation and financial data handling
        /// </summary>
        public static FrameworkElement SafeGetCell(this DataGrid grid, object item, string sortMemberPath)
        {
            ValidateCellAccess(grid, item, sortMemberPath);

            try
            {
                var column = GetCachedColumn(grid, sortMemberPath);
                // Get the row container for the given item.
                var row = GetRowContainingItem(grid, item);

                if (!IsItemVisible(grid, row))
                    throw new AccessViolationException("Attempted access to non-visible financial data");

                return GetCellContent(grid, column, row);
            }
            catch (SecurityException)
            {
                return null;
            }
            catch (AccessViolationException)
            {
                return null;
            }
        }
        #endregion

        #region Column Management

        public static DataGridColumn GetCachedColumn(this DataGrid grid, string path)
        {
            var key = Tuple.Create(grid, path);
            return _columnCache.GetOrAdd(key, k =>
                grid.Columns.FirstOrDefault(c =>
                    c.SortMemberPath.Equals(k.Item2, StringComparison.OrdinalIgnoreCase)));
        }
        #endregion

        #region Row Handling

        public static DataGridRow? GetRowContainingItem(this DataGrid grid, object item)
        {
            // Ensure layout is updated before retrieving the container.
            if (grid.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                grid.BeginInit();
                grid.ScrollIntoView(item);
                grid.UpdateLayout();
                grid.EndInit();
            }

            return grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        }
        #endregion

        #region Cell Content Extraction

        private static FrameworkElement? GetCellContent(DataGrid grid, DataGridColumn column, DataGridRow row)
        {
            if (row == null)
                return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(row);

            // The original code called: column?.UpdateCellContent(row, null);
            // Since UpdateCellContent does not exist, you can force template update by calling
            // ApplyTemplate on the row or cell as needed.
            row.ApplyTemplate();

            return column?.GetCellContent(row) as FrameworkElement;
        }
        #endregion

        #region Security System

        public static void ValidateCellAccess(DataGrid grid, object item, string path)
        {
            if (grid.ItemsSource == null)
                throw new InvalidOperationException("DataGrid contains no financial records");

            if (!grid.Items.Contains(item))
                throw new ArgumentException("Financial record not found in dataset");

            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(_invalidPathChars) >= 0)
                throw new SecurityException("Invalid data path structure");

            if (path.Contains("..") || path.StartsWith("/"))
                throw new SecurityException("Potential path traversal attempt detected");
        }
        #endregion

        #region Financial Data Handling

        public static decimal? GetDecimalValue(this DataGridCell cell)
        {
            if (cell?.DataContext == null) return null;

            var binding = (cell.Column as DataGridBoundColumn)?.Binding as Binding;
            var rawValue = binding?.Converter != null ?
                binding.Converter.Convert(cell.DataContext, typeof(object),
                    binding.ConverterParameter, CultureInfo.CurrentCulture)
                : cell.Content;

            return decimal.TryParse(rawValue?.ToString(), NumberStyles.Currency,
                CultureInfo.CurrentCulture, out decimal result)
                ? result
                : (decimal?)null;
        }
        #endregion

        #region Audit System Integration

        public static void EnableFinancialAuditing(this DataGrid grid)
        {
            grid.CellEditEnding += (sender, e) =>
            {
                var oldValue = e.EditingElement.GetCulturalValue();
                var newValue = (e.EditingElement as dynamic).Text;
                // Additional auditing logic can be added here.
            };
        }
        #endregion

        #region Visualization Helpers

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }

        private static bool IsItemVisible(DataGrid grid, DataGridRow row)
        {
            if (row == null)
                return false;

            var scrollViewer = FindVisualChild<ScrollViewer>(grid);
            if (scrollViewer == null)
                return true;

            // Compare the row's index with the VerticalOffset.
            int rowIndex = grid.ItemContainerGenerator.IndexFromContainer(row);
            return rowIndex >= scrollViewer.VerticalOffset;
        }
        #endregion

        #region Cultural Adaptation

        public static object? GetCulturalValue(this FrameworkElement element)
        {
            if (element is TextBlock textBlock)
                return decimal.Parse(textBlock.Text, NumberStyles.Currency);

            if (element is TextBox textBox)
                return decimal.Parse(textBox.Text, NumberStyles.Currency);

            // Use ContentControl.ContentProperty instead of a non-existing ContentProperty.
            return element?.GetValue(ContentControl.ContentProperty);
        }
        #endregion
    }

    /*
        public static void ExampleUsage()
        {
            DataGrid MyDataGrid = new DataGrid();
            // Attempt to get the cell's FrameworkElement.
            FrameworkElement cellElement = MyDataGrid.SafeGetCell(MyDataGrid.SelectedItem, "CODE");

            if (cellElement != null)
            {
                // You can, for example, cast this to a DataGridCell if necessary:
                DataGridCell cell = cellElement as DataGridCell;
                // Further processing...
            }
            else
            {
                // Handle the case when retrieval fails (security issues, record not found, etc.)
                MessageBox.Show("Unable to retrieve cell or record is not visible.");
            }

        }
     */
}
