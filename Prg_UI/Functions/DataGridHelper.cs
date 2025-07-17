using DocumentFormat.OpenXml.Presentation;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Functions
{
    public static class DataGridHelper
    {
        /// <summary>
        /// Sets focus to a specific cell in a DataGrid and begins editing.
        /// </summary>
        /// <param name="dataGrid">The target DataGrid.</param>
        /// <param name="rowIndex">The index of the row to edit.</param>
        /// <param name="columnIndex">The index of the column to edit.</param>
        public static void FocusAndEditCell(DataGrid dataGrid, string sortMemberPath, int rowIndex, bool isEditMode = true)
        {
            var columnIndex = dataGrid.Columns.FirstOrDefault(c => c.SortMemberPath == sortMemberPath).DisplayIndex;

            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (rowIndex < 0 || rowIndex >= dataGrid.Items.Count)
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index is out of range.");

            if (columnIndex < 0 || columnIndex >= dataGrid.Columns.Count)
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index is out of range.");

            // Ensure DataGrid is loaded and ready
            if (!dataGrid.IsLoaded)
            {
                dataGrid.Loaded += (s, e) => FocusAndEditCell(dataGrid, sortMemberPath, rowIndex, isEditMode);
                return;
            }

            // Scroll to the specified row to make it visible
            dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);

            // Get the row container for the specified row
            DataGridRow rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            if (rowContainer == null)
            {
                // Row container may not be generated yet, so delay the call slightly
                dataGrid.Dispatcher.InvokeAsync(() => FocusAndEditCell(dataGrid, sortMemberPath, rowIndex, isEditMode), DispatcherPriority.Loaded);
                return;
            }

            // Ensure column is valid
            DataGridColumn column = dataGrid.Columns[columnIndex];
            if (column == null || column.Visibility != Visibility.Visible)
                throw new InvalidOperationException("Specified column is not visible or doesn't exist.");

            // Bring the cell into view
            dataGrid.ScrollIntoView(rowContainer, column);

            // Delay focus to ensure UI thread is ready
            dataGrid.Dispatcher.InvokeAsync(() =>
            {
                // Get the cell
                DataGridCell cell = GetCell(dataGrid, rowContainer, columnIndex);
                if (cell == null)
                    throw new InvalidOperationException("Unable to locate the specified cell.");

                // Focus the cell
                cell.Focus();
                if (!cell.IsFocused)
                    throw new InvalidOperationException("Cell could not receive focus.");

                if (isEditMode)
                {
                    // Set the cell into edit mode if possible
                    dataGrid.BeginEdit();
                }

            }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Retrieves a specific cell from a DataGrid.
        /// </summary>
        /// <param name="dataGrid">The target DataGrid.</param>
        /// <param name="row">The DataGridRow containing the cell.</param>
        /// <param name="columnIndex">The column index of the cell.</param>
        /// <returns>The requested DataGridCell if available; otherwise, null.</returns>
        private static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int columnIndex)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            // Attempt to retrieve the DataGridCellsPresenter for the row. This provides access to the containers.
            DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null)
            {
                // If the presenter is null, force generation and try again after updating layout
                row.ApplyTemplate();
                presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                    return null;
            }

            DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);

            // If cell is still null (possibly due to virtualization) force it to be generated.
            if (cell == null)
            {
                // Try to bring the cell into view
                dataGrid.ScrollIntoView(row, dataGrid.Columns[columnIndex]);
                dataGrid.UpdateLayout();
                cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            }

            return cell;
        }

        /// <summary>
        /// Helper method to locate a child of a specific type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the child to find.</typeparam>
        /// <param name="parent">The parent element.</param>
        /// <returns>The child element if found; otherwise, null.</returns>
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                T result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        // متد کمکی برای بالا رفتن در درخت بصری و پیدا کردن والد از نوع T
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        /// <summary>
        /// Copies the value from the cell directly above the current cell in the specified DataGrid.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to operate on.</param>
        /// <param name="e">The KeyEventArgs to check for key combinations. Optional.</param>
        /// <param name="modifierKey">The required modifier key for the operation. Defaults to Control.</param>
        /// <param name="key">The key to trigger the operation. Defaults to OemQuotes.</param>
        public static void CopyValueFromAboveCell(
            DataGrid dataGrid,
            KeyEventArgs e = null,
            ModifierKeys modifierKey = ModifierKeys.Control,
            Key key = Key.OemQuotes)
        {
            try
            {
                // Check for key combination if KeyEventArgs is provided
                if (e != null && ((Keyboard.Modifiers & modifierKey) != modifierKey || e.Key != key))
                    return;

                // Ensure the DataGrid is not null
                if (dataGrid == null || dataGrid.CurrentCell == null)
                    return;

                // Get the current cell
                DataGridCellInfo currentCell = dataGrid.CurrentCell;
                if (currentCell.Item == null || currentCell.Column == null)
                    return;

                // Get the row and column indices
                int rowIndex = dataGrid.Items.IndexOf(currentCell.Item);
                int columnIndex = dataGrid.Columns.IndexOf(currentCell.Column);

                // Ensure row and column indices are valid
                if (rowIndex <= 0 || columnIndex < 0 || columnIndex >= dataGrid.Columns.Count)
                    return;

                // Get the item from the row above
                object rowAbove = dataGrid.Items[rowIndex - 1];
                if (rowAbove == null)
                    return;

                // Get the column property information
                var column = dataGrid.Columns[columnIndex];
                if (string.IsNullOrEmpty(column.SortMemberPath))
                    return;

                // Use reflection to get the property value from the above row
                PropertyInfo propertyInfoAbove = rowAbove.GetType().GetProperty(column.SortMemberPath);
                if (propertyInfoAbove == null)
                    return;

                object valueAbove = propertyInfoAbove.GetValue(rowAbove);

                // Set the value in the current cell's item
                object currentItem = currentCell.Item;
                PropertyInfo currentCellProperty = currentItem.GetType().GetProperty(column.SortMemberPath);
                if (currentCellProperty != null)
                {
                    currentCellProperty.SetValue(currentItem, valueAbove);

                    // Refresh the DataGrid to show changes
                    dataGrid.BeginEdit();
                    //dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                }

                // Mark the event as handled
                if (e != null)
                    e.Handled = true;
            }
            catch (Exception ex)
            {
            }
        }


        #region DATAGRID_CELL
        /// <returns>The DataGridCell if found; otherwise, null.</returns>
        public static DataGridCell GetCellBySortMemberPath(this DataGrid dataGrid, object rowItem, string sortMemberPath)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));
            if (rowItem == null)
                throw new ArgumentNullException(nameof(rowItem));
            if (string.IsNullOrWhiteSpace(sortMemberPath))
                throw new ArgumentException("SortMemberPath cannot be null or whitespace.", nameof(sortMemberPath));

            // Find the column with the matching SortMemberPath.
            DataGridColumn targetColumn = null;
            foreach (var column in dataGrid.Columns)
            {
                if (string.Compare(column.SortMemberPath, sortMemberPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    targetColumn = column;
                    break;
                }
            }

            if (targetColumn == null)
            {
                // Column matching the sortMemberPath was not found.
                return null;
            }

            // First, make sure the row is realized in the DataGrid.
            // Scroll the DataGrid to bring the item into view -- this ensures the container is generated
            dataGrid.ScrollIntoView(rowItem, targetColumn);

            // Force UI update – sometimes cells are not generated immediately.
            dataGrid.UpdateLayout();

            // Get the DataGridRow for the item.
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(rowItem);

            if (row == null)
            {
                // If the row container is still null, attempt to force generation by delaying a bit.
                // Note: Consider using Dispatcher.BeginInvoke in production instead of Thread.Sleep.
                dataGrid.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(rowItem);
                if (row == null)
                {
                    // If still null, then the row might not be in view yet.
                    return null;
                }
            }

            // Attempt to get the cell using the helper method.
            DataGridCell cell = GetCell(dataGrid, row, targetColumn.DisplayIndex);
            return cell;
        }

        // Example usage in your Window or UserControl code-behind:
          /* 
           var cell = myDataGrid.GetCellBySortMemberPath(rowData, "YourPropertyName");

            if (cell != null)
            {
                // Now you have access to cell. For example, setting focus:
                cell.Focus();
                // And you can also access or change content, styles, etc.
            }
            else
            {
                // Handle cell not available scenario.
            }
          */

    }
    #endregion
}
