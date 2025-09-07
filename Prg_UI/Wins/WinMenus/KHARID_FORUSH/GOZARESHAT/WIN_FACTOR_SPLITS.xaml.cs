using AUTO_BAZ.HelperWins;
using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.WinAutomasion;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    public partial class WIN_FACTOR_SPLITS : Window
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
        public WIN_FACTOR_SPLITS(string _DT1_, string _DT2_, string _WINNAME_)
        {
            InitializeComponent();

            DT1 = _DT1_;
            DT2 = _DT2_;
            WINNAME = _WINNAME_;

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<FACTOR_SPLIT_MODEL> FACTOR_DATA { get; set; } = new ObservableCollection<FACTOR_SPLIT_MODEL>();
        public bool NowIsReady { get; private set; }
        private string DT1 { get; set; } = "10000101";
        private string DT2 { get; set; } = "99991230";
        private string WINNAME { get; }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            System.Collections.Generic.List<FACTOR_SPLIT_MODEL> MasterHead = null;
            if (WINNAME == "KHLS")
            {
                WINTILENAME.Content = "گزارش خرید به تفکیک فاکتور";

                MasterHead = dbms.DoGetDataSQL<FACTOR_SPLIT_MODEL>(@$"SELECT dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.M_NAGHD, dbo.HEAD_LST.MABL_VAR, dbo.HEAD_LST.MABL_HAV, dbo.HEAD_LST.MABL_HAZ, dbo.HEAD_LST.TAKHFIF, dbo.HEAD_LST.FNUMCO, dbo.HEAD_LST.USER_NAME, dbo.HEAD_LST.MBAA, dbo.HEAD_LST.OKF, dbo.HEAD_LST.CDDATE, dbo.HEAD_LST.CDTIME, dbo.HEAD_LST.OKDATE, dbo.HEAD_LST.OKTIME, SUM(dbo.INVO_LST.MEGHk) AS MEGHkS, SUM(dbo.INVO_LST.MABL_K) AS MABL_KS, dbo.CUST_HESAB.NAME, dbo.CUST_HESAB.ADDRESS, dbo.CUST_HESAB.TEL, dbo.CUST_HESAB.CODE_E, dbo.CUST_HESAB.ECODE, dbo.CUST_HESAB.PCODE, dbo.CUST_HESAB.IYALAT, dbo.CUST_HESAB.CITY, dbo.CUST_HESAB.MCODEM, dbo.CUST_HESAB.TOZIH, dbo.CUST_HESAB.MOBILE, HEAD_LST.N_S, HEAD_LST.VAS, HEAD_LST.MAS, HEAD_LST.TAH, HEAD_LST.NUMBER1, HEAD_LST.ANBAR, HEAD_LST.MOIN_VAR, HEAD_LST.MOIN_HAV, HEAD_LST.MOIN_HAZ, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.SHARAYET, HEAD_LST.SGN1, HEAD_LST.SGN2, HEAD_LST.SGN3, HEAD_LST.SGN4, HEAD_LST.HMBAA, HEAD_LST.TAMIR, HEAD_LST.TICMBAA, HEAD_LST.TKHF, HEAD_LST.SADER, HEAD_LST.ARZD, HEAD_LST.ARZKIND, HEAD_LST.JAY, HEAD_LST.MODAT_PPID, HEAD_LST.PEPID, HEAD_LST.PEID, HEAD_LST_1.DATE_N AS date_h, HEAD_LST_1.DATE_N-dbo.HEAD_LST.DATE_N AS ddf
                                                                  FROM dbo.HEAD_LST
                                                                       INNER JOIN dbo.INVO_LST ON dbo.HEAD_LST.NUMBER=dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG-11=dbo.INVO_LST.TAG
                                                                       INNER JOIN dbo.CUST_HESAB ON dbo.HEAD_LST.CUST_NO=dbo.CUST_HESAB.hes
                                                                       INNER JOIN dbo.HEAD_LST HEAD_LST_1 ON dbo.INVO_LST.NUMBER=HEAD_LST_1.NUMBER AND dbo.INVO_LST.TAG=HEAD_LST_1.TAG
                                                                  GROUP BY dbo.HEAD_LST.NUMBER, dbo.HEAD_LST.TAG, dbo.HEAD_LST.DATE_N, dbo.HEAD_LST.CUST_NO, dbo.HEAD_LST.MOLAH, dbo.HEAD_LST.M_NAGHD, dbo.HEAD_LST.MABL_VAR, dbo.HEAD_LST.MABL_HAV, dbo.HEAD_LST.MABL_HAZ, dbo.HEAD_LST.TAKHFIF, dbo.HEAD_LST.FNUMCO, dbo.HEAD_LST.USER_NAME, dbo.HEAD_LST.MBAA, dbo.HEAD_LST.CDDATE, dbo.HEAD_LST.CDTIME, dbo.HEAD_LST.OKDATE, dbo.HEAD_LST.OKTIME, dbo.HEAD_LST.OKF, dbo.CUST_HESAB.NAME, dbo.CUST_HESAB.ADDRESS, dbo.CUST_HESAB.TEL, dbo.CUST_HESAB.CODE_E, dbo.CUST_HESAB.ECODE, dbo.CUST_HESAB.PCODE, dbo.CUST_HESAB.IYALAT, dbo.CUST_HESAB.CITY, dbo.CUST_HESAB.MCODEM, dbo.CUST_HESAB.TOZIH, dbo.CUST_HESAB.MOBILE, HEAD_LST.N_S, HEAD_LST.VAS, HEAD_LST.MAS, HEAD_LST.TAH, HEAD_LST.NUMBER1, HEAD_LST.ANBAR, HEAD_LST.MOIN_VAR, HEAD_LST.MOIN_HAV, HEAD_LST.MOIN_HAZ, HEAD_LST.MOIN_KHF, HEAD_LST.ANBARF, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.SHARAYET, HEAD_LST.HMBAA, HEAD_LST.TAMIR, HEAD_LST.SADER, HEAD_LST.ARZD, HEAD_LST.ARZKIND, HEAD_LST.MODAT_PPID, HEAD_LST.PEPID, HEAD_LST.PEID, HEAD_LST.SGN1, HEAD_LST.SGN2, HEAD_LST.SGN3, HEAD_LST.SGN4, HEAD_LST.TICMBAA, HEAD_LST.TKHF, HEAD_LST.JAY, HEAD_LST_1.DATE_N, HEAD_LST_1.DATE_N-dbo.HEAD_LST.DATE_N
                                                                  HAVING(dbo.HEAD_LST.TAG=12)AND(dbo.HEAD_LST.DATE_N BETWEEN {DT1} AND {DT2});").ToList();
            }

            FACTOR_DATA?.Clear();
         
            foreach (var item in MasterHead)
            {
                FACTOR_DATA.Add(item);
            }

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                //e.Handled = true;
            }
        }

        private readonly FilterService<FACTOR_SPLIT_MODEL> filterService = new FilterService<FACTOR_SPLIT_MODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e) // Event handler for when the selection changes in the data grid
        {
            //// Get the selected row and column index
            //var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
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
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
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
            if (SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                          // Return the column name and the current cell value
                return (columnName, CurrentCellValue);
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter() // Method to apply all cumulative filters to the data grid
        {
            // Set the filter for the data grid view using the filter service
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as FACTOR_SPLIT_MODEL);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }


    }
}
