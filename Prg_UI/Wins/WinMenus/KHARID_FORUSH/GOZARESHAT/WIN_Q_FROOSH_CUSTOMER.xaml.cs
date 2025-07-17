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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    public partial class WIN_Q_FROOSH_CUSTOMER : Window
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
        public WIN_Q_FROOSH_CUSTOMER(string? _dt1_, string? _dt2_, string? _WINNAME_)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(_dt1_))
            {
                DT1 = _dt1_;
            }

            if (!string.IsNullOrEmpty(_dt2_))
            {
                DT2 = _dt2_;
            }
            if (!string.IsNullOrEmpty(_WINNAME_))
            {
                this.Tag = _WINNAME_;
            }

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<Q_FROOSH_CUSTOMER> FACTOR_DATA { get; set; } = new ObservableCollection<Q_FROOSH_CUSTOMER>();
        public bool NowIsReady { get; private set; }

        public string DT1 { get; set; } = "10000101";
        public string DT2 { get; set; } = "99991230";
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FACTOR_DATA?.Clear();
            List<Q_FROOSH_CUSTOMER> MasterHead = null;
            switch (Tag)
            {
                case "FRCUST":
                    WINTILENAME.Content = "گزارش ارزش افزوده فروش - گزارش فصلی";
                    SALE_AMOUNT_COLUMN.IsHidden = false; //Show this Column
                    NET_SALE_COLUMN.IsHidden = false;
                    MasterHead = dbms.DoGetDataSQL<Q_FROOSH_CUSTOMER>(@$"SELECT hes, NAME, FROOSH, SumOfTAKHFIF, FROOSHKH, SMBAA, GHABEL, KK, ADDRESS, CITY, CODE_E, ECODE, IYALAT, MCODEM, MOBILE, PCODE, TEL, TOZIH FROM dbo.Q_FROOSH_CUSTOMER(N'{DT1}', N'{DT2}')").ToList();
                    break;

                case "FASLIBR":
                    WINTILENAME.Content = "گزارش فصلی برگشت فروش";
                    SALE_BACK_AMOUNT_COLUMN.IsHidden = false;
                    NET_SALE_COLUMN.IsHidden = false;
                    NET_SALE_COLUMN.HeaderText = "برگشت فروش خالص";
                    MasterHead = dbms.DoGetDataSQL<Q_FROOSH_CUSTOMER>(@$"SELECT hes, NAME, FROOSHbr, SumOfTAKHFIF, FROOSHKH, SMBAA, GHABEL, KK, ADDRESS, CITY, CODE_E, ECODE, IYALAT, MCODEM, MOBILE, PCODE, TEL, TOZIH FROM dbo.qsl_fasli_bargash_main(N'{DT1}', N'{DT2}')").ToList();
                    break;

                case "FASLIKHBR":
                    WINTILENAME.Content = "گزارش فصلی برگشت خرید";
                    KHAREEDKH_COLUMN.IsHidden = false;
                    KHAREEDbr_COLUMN.IsHidden = false;
                    MasterHead = dbms.DoGetDataSQL<Q_FROOSH_CUSTOMER>(@$"SELECT * FROM dbo.qsl_fasli_bargash_main_KH(N'{DT1}', N'{DT2}')").ToList();
                    break;

                case "KHCUST":
                    WINTILENAME.Content = "گزارش ارزش افزوده خرید - گزارش فصلی";
                    KHARED_COLUMN.IsHidden = false;
                    KHAREDKH_COLUMN.IsHidden = false;
                    MasterHead = dbms.DoGetDataSQL<Q_FROOSH_CUSTOMER>(@$"SELECT * FROM dbo.Q_KHARED_CUSTOMER(N'{DT1}', N'{DT2}')").ToList();
                    break;


                default: break;
            }



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

        private readonly FilterService<Q_FROOSH_CUSTOMER> filterService = new FilterService<Q_FROOSH_CUSTOMER>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
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

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as Q_FROOSH_CUSTOMER);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }
    }
}
