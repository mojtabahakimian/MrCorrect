using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.UiTools
{
    /// <summary>
    /// A defensive <see cref="GridSelectionController"/> that guards against Syncfusion's
    /// internal column index getting out of range when keyboard navigation occurs.
    /// It normalises the current cell before delegating to the base controller and,
    /// if an <see cref="ArgumentOutOfRangeException"/> is still thrown, it attempts to
    /// recover and swallow the invalid navigation request instead of letting the
    /// application crash.
    /// </summary>
    public class SafeGridSelectionController : GridSelectionController
    {
        public SafeGridSelectionController(SfDataGrid dataGrid) : base(dataGrid)
        {
        }

        protected override void ProcessKeyDown(KeyEventArgs args)
        {
            EnsureCurrentCellWithinBounds();

            try
            {
                base.ProcessKeyDown(args);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"SfDataGrid navigation adjusted because of invalid column index. {ex}");

                if (!EnsureCurrentCellWithinBounds())
                {
                    args.Handled = true;
                    return;
                }

                try
                {
                    base.ProcessKeyDown(args);
                }
                catch (ArgumentOutOfRangeException retryEx)
                {
                    Debug.WriteLine($"SfDataGrid navigation ignored after retry. {retryEx}");
                    args.Handled = true;
                }
            }
        }

        private bool EnsureCurrentCellWithinBounds()
        {
            var dataGrid = DataGrid;
            if (dataGrid?.Columns == null)
            {
                return false;
            }

            var columnCount = dataGrid.Columns.Count;
            if (columnCount == 0)
            {
                return false;
            }

            var currentCell = CurrentCellManager?.CurrentCell;
            if (currentCell == null)
            {
                return false;
            }

            var columnIndex = currentCell.ColumnIndex;
            var rowIndex = currentCell.RowIndex;

            var safeColumnIndex = columnIndex;
            if (columnIndex < 0 || columnIndex >= columnCount)
            {
                safeColumnIndex = Math.Clamp(columnIndex, 0, columnCount - 1);
            }

            if (rowIndex < 0)
            {
                rowIndex = GetSafeRowIndex(dataGrid);
            }

            if (safeColumnIndex == columnIndex && rowIndex == currentCell.RowIndex)
            {
                return true;
            }

            if (rowIndex < 0)
            {
                return false;
            }

            try
            {
                dataGrid.MoveCurrentCell(new RowColumnIndex(rowIndex, safeColumnIndex));
                return true;
            }
            catch (ArgumentOutOfRangeException moveEx)
            {
                Debug.WriteLine($"Unable to normalise SfDataGrid current cell. {moveEx}");
                return false;
            }
        }

        private int GetSafeRowIndex(SfDataGrid dataGrid)
        {
            if (dataGrid.View == null)
            {
                return -1;
            }

            try
            {
                var recordIndex = dataGrid.SelectedIndex;
                if (recordIndex >= 0 && recordIndex < dataGrid.View.Records.Count)
                {
                    var selectedRowIndex = dataGrid.ResolveToRowIndex(recordIndex);
                    if (selectedRowIndex >= 0)
                    {
                        return selectedRowIndex;
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"Selected record index out of range. {ex}");
            }

            if (dataGrid.View.Records.Count > 0)
            {
                try
                {
                    var firstRowIndex = dataGrid.ResolveToRowIndex(0);
                    if (firstRowIndex >= 0)
                    {
                        return firstRowIndex;
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.WriteLine($"Unable to resolve first row index. {ex}");
                }
            }

            return -1;
        }
    }
}
