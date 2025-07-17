using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class PGET_LST_SEARCH : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
                    break;
            }
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public PGET_LST_SEARCH()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public ObservableCollection<PGET_JOTEJU> PGET_JOTEJU_DATA { get; set; } = new ObservableCollection<PGET_JOTEJU>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PGET_JOTEJU_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<PGET_JOTEJU>(@$"SELECT        dbo.PGET_LST.ID, dbo.PGET_LST.DATE, dbo.PGET_LST.RADIF, dbo.PGET_LST.NO_AM, dbo.PGET_LST.NAHVA, dbo.PGET_LST.FHES_K, dbo.PGET_LST.FHES_M, dbo.PGET_LST.THES_K, dbo.PGET_LST.THES_M, 
                                                                                        dbo.PGET_LST.SHARH, dbo.PGET_LST.MABL, dbo.PGET_LST.N_SERI, dbo.PGET_LST.BANK, ISNULL(TOTA_HES_1.NAME, N' ') + N'-' + ISNULL(DETA_HES_1.NAME, N' ') + N'-' + ISNULL(TDETA_HES_1.NAME, N' ') AS FHES, 
                                                                                        ISNULL(TOTA_HES_1.NAME, N' ') + N'-' + ISNULL(DETA_HES_1.NAME, N' ') + N'-' + ISNULL(TDETA_HES_1.NAME, N' ') AS THES, dbo.PGET_LST.FHES_T, dbo.PGET_LST.THES_T, ISNULL(dbo.PAY_GETD.DATE_S, 
                                                                                        dbo.PAY_GETP.DATE_S) AS dates, dbo.PGET_LST.FHES AS Expr1, dbo.PGET_LST.THES AS Expr2, dbo.PGET_LST.ARZD, dbo.PGET_LST.FHES_T2, dbo.PGET_LST.THES_T2, dbo.PGET_LST.FHES_T3, dbo.PGET_LST.THES_T3, 
                                                                                        dbo.PGET_LST.FHES_T4, dbo.PGET_LST.THES_T4, dbo.PGET_HED.USER_NAME
                                                               FROM            dbo.TOTA_HES AS TOTA_HES_1 INNER JOIN
                                                                                        dbo.DETA_HES AS DETA_HES_1 INNER JOIN
                                                                                        dbo.TDETA_HES AS TDETA_HES_1 ON DETA_HES_1.NUMBER = TDETA_HES_1.NUMBER AND DETA_HES_1.N_KOL = TDETA_HES_1.N_KOL ON TOTA_HES_1.NUMBER = DETA_HES_1.N_KOL INNER JOIN
                                                                                        dbo.TOTA_HES AS TOTA_HES_2 INNER JOIN
                                                                                        dbo.DETA_HES AS DETA_HES_2 ON TOTA_HES_2.NUMBER = DETA_HES_2.N_KOL INNER JOIN
                                                                                        dbo.TDETA_HES AS TDETA_HES_2 INNER JOIN
                                                                                        dbo.PGET_LST ON TDETA_HES_2.TNUMBER = dbo.PGET_LST.FHES_T AND TDETA_HES_2.NUMBER = dbo.PGET_LST.FHES_M AND TDETA_HES_2.N_KOL = dbo.PGET_LST.FHES_K ON 
                                                                                        DETA_HES_2.NUMBER = TDETA_HES_2.NUMBER AND DETA_HES_2.N_KOL = TDETA_HES_2.N_KOL ON TDETA_HES_1.TNUMBER = dbo.PGET_LST.THES_T AND TDETA_HES_1.NUMBER = dbo.PGET_LST.THES_M AND 
                                                                                        TDETA_HES_1.N_KOL = dbo.PGET_LST.THES_K INNER JOIN
                                                                                        dbo.PGET_HED ON dbo.PGET_LST.ID = dbo.PGET_HED.ID AND dbo.PGET_LST.DATE = dbo.PGET_HED.DATE LEFT OUTER JOIN
                                                                                        dbo.PAY_GETP ON dbo.PGET_LST.N_SERI = dbo.PAY_GETP.N_SERI AND dbo.PGET_LST.BANK = dbo.PAY_GETP.BANK LEFT OUTER JOIN
                                                                                        dbo.PAY_GETD ON dbo.PGET_LST.N_SERI = dbo.PAY_GETD.N_SERI AND dbo.PGET_LST.BANK = dbo.PAY_GETD.BANK").ToList();
            foreach (var item in MasterHead)
            {
                PGET_JOTEJU_DATA.Add(item);
            }

        }

        private readonly FilterService<PGET_JOTEJU> filterService = new FilterService<PGET_JOTEJU>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void PGET_JOTEJU_DATA_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void PGET_JOTEJU_DATA_SUB_SelectionChanged(object sender, GridSelectionChangedEventArgs e) // Event handler for when the selection changes in the data grid
        {
            //// Get the selected row and column index
            //var currentCell = PGET_JOTEJU_DATA_SUB.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null)
            //{
            //    var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
            //    UpdateCurrentCellValue(rowColumnIndex);
            //}
        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex) // Method to update the current cell value
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.PGET_JOTEJU_DATA_SUB.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.PGET_JOTEJU_DATA_SUB.Columns[columnIndex].MappingName;
            var recordIndex = this.PGET_JOTEJU_DATA_SUB.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.PGET_JOTEJU_DATA_SUB.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName)?.GetValue(record)?.ToString();
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e) // Event handler for the "Filter by Selection" context menu item click
        {
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
            if (filterValue != null)
            {
                // Add the filter to the filter service
                filterService.AddFilter(columnName, filterValue);
                // Add the filter to the list of active filters
                ActiveFilters.Add($"{columnName} = {filterValue}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e) // Event handler for the "Filter Excluding Selection" context menu item click
        {
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
            if (filterValue != null)
            {
                // Add the exclusion filter to the filter service
                filterService.AddFilter(columnName, $"!{filterValue}");
                // Add the filter to the list of active filters
                ActiveFilters.Add($"{columnName} != {filterValue}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
        }
        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e) // Event handler to remove all filters and sorting
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails() // Method to get the details of the selected cell
        {
            // Check if there is a current cell selected in the data grid
            if (PGET_JOTEJU_DATA_SUB.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = PGET_JOTEJU_DATA_SUB.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                                 // Return the column name and the current cell value
                return (columnName, CurrentCellValue);
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter() // Method to apply all cumulative filters to the data grid
        {
            // Set the filter for the data grid view using the filter service
            PGET_JOTEJU_DATA_SUB.View.Filter = item => filterService.ApplyFilter(item as PGET_JOTEJU);
            // Refresh the filter to update the view
            PGET_JOTEJU_DATA_SUB.View.RefreshFilter();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && PGET_JOTEJU_DATA_SUB.SelectedItem != null)
            {
                e.Handled = true;

                var CurrentRow = PGET_JOTEJU_DATA_SUB.SelectedItem as PGET_JOTEJU;

                if (CurrentRow != null && CurrentRow?.ID != null)
                {
                    //برای اینکه خزانه یکبار بیشتر نمیتواند باز شود در کلاس منو ها به این شکل عمل میکنیم
                    //CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, this , Convert.ToDouble(CurrentRow.ID));
                    new Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED(Convert.ToDouble(CurrentRow.ID)).Show();
                }

            }
        }
    }
}
