using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Functions
{
    public static class DataGridNavigationManager
    {
        /// <summary>
        /// Selects the first row in the DataGrid if there are any items.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to navigate.</param>
        /// <exception cref="ArgumentNullException">Thrown if dataGrid is null.</exception>
        public static void GoFirst(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (!dataGrid.IsEnabled || dataGrid.Visibility != System.Windows.Visibility.Visible)
            {
                return;
            }

            if (dataGrid.Items.Count > 0)
            {
                dataGrid.SelectedIndex = 0;
                dataGrid.ScrollIntoView(dataGrid.Items[0]);
            }
        }

        /// <summary>
        /// Selects the last row in the DataGrid if there are any items.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to navigate.</param>
        /// <exception cref="ArgumentNullException">Thrown if dataGrid is null.</exception>
        public static void GoLast(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (!dataGrid.IsEnabled || dataGrid.Visibility != System.Windows.Visibility.Visible)
            {
                return;
            }

            if (dataGrid.Items.Count > 0)
            {
                dataGrid.SelectedIndex = dataGrid.Items.Count - 1;
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
            }
        }

        /// <summary>
        /// Selects the next row in the DataGrid. If no row is selected, selects the first row.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to navigate.</param>
        /// <exception cref="ArgumentNullException">Thrown if dataGrid is null.</exception>
        public static void GoNext(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (dataGrid.Items.Count == 0)
                return;

            if (!dataGrid.IsEnabled || dataGrid.Visibility != System.Windows.Visibility.Visible)
            {
                return;
            }

            if (dataGrid.SelectedIndex == -1)
            {
                dataGrid.SelectedIndex = 0;
                dataGrid.ScrollIntoView(dataGrid.Items[0]);
            }
            else if (dataGrid.SelectedIndex < dataGrid.Items.Count - 1)
            {
                dataGrid.SelectedIndex++;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }

        /// <summary>
        /// Selects the previous row in the DataGrid. If no row is selected, selects the last row.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to navigate.</param>
        /// <exception cref="ArgumentNullException">Thrown if dataGrid is null.</exception>
        public static void GoBack(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (dataGrid.Items.Count == 0)
                return;

            if (!dataGrid.IsEnabled || dataGrid.Visibility != System.Windows.Visibility.Visible)
            {
                return;
            }

            if (dataGrid.SelectedIndex == -1)
            {
                dataGrid.SelectedIndex = dataGrid.Items.Count - 1;
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
            }
            else if (dataGrid.SelectedIndex > 0)
            {
                dataGrid.SelectedIndex--;
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }

        /// <summary>
        /// Scrolls to the new row placeholder and begins editing in the specified column (or the first column if none specified).
        /// Requires CanUserAddRows to be true on the DataGrid.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to navigate.</param>
        /// <param name="column">The column to focus for editing; defaults to the first column if null.</param>
        /// <exception cref="ArgumentNullException">Thrown if dataGrid is null.</exception>
        public static void GoNewPlaceholder(DataGrid dataGrid, DataGridColumn column = null)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            if (!dataGrid.CanUserAddRows)
                return;

            if (!dataGrid.IsEnabled || dataGrid.IsReadOnly || dataGrid.Visibility != System.Windows.Visibility.Visible)
            {
                return;
            }

            dataGrid.ScrollIntoView(CollectionView.NewItemPlaceholder);
            dataGrid.UpdateLayout();

            if (column == null && dataGrid.Columns.Count > 0)
            {
                column = dataGrid.Columns[0];
            }

            if (column != null)
            {
                dataGrid.CurrentCell = new DataGridCellInfo(CollectionView.NewItemPlaceholder, column);
                dataGrid.BeginEdit();
            }
        }

    }
}
