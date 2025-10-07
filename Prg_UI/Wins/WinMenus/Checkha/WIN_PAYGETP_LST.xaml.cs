using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.HelperWins;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.BulletGraph;
using System.Windows.Controls;
using Prg_Proccessy.FUNCTIONS;
using System.Windows.Interop;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_UI.UiTools;
using Prg_Proccessy.MODELS;

namespace Wins.WinMenus.Checkha
{
    public partial class WIN_PAYGETP_LST : Window
    {
        #region Header Window Begin
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
        #endregion
        public WIN_PAYGETP_LST()
        {
            InitializeComponent();

            this.DataContext = this;
        }
        public ObservableCollection<PAY_GETP_MODEL> PAY_GETP_DATA { get; set; } = new ObservableCollection<PAY_GETP_MODEL>();
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "CHEKP", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            ReGetData();

            CL_LMethods.FocusLastSfDataGridRow(PAY_GETP_SUB);
            //PAY_GETP_SUB.ColumnSizer = GridLengthUnitType.Auto;
        }

        private void ReGetData()
        {
            PAY_GETP_DATA?.Clear();

            var MasterHead = dbms.DoGetDataSQL<PAY_GETP_MODEL>(@$"SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK, dbo.PAY_GETP.DATE_S, dbo.PAY_GETP.DATE, dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.MABL, dbo.PAY_GETP.NAME_TAH, dbo.PAY_GETP.N_HESAB, 
                                                                     dbo.PAY_GETP.N_S, dbo.PAY_GETP.N_KOL, dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF, dbo.PAY_GETP.N_KOL2, dbo.PAY_GETP.N_MOIN2, dbo.PAY_GETP.N_TAF2, dbo.PAY_GETP.N_KOL3, dbo.PAY_GETP.N_MOIN3, 
                                                                     dbo.PAY_GETP.N_TAF3, dbo.PAY_GETP.NUMBER, dbo.PAY_GETP.TAG, dbo.PAY_GETP.ANBAR, dbo.PAY_GETP.RADIF, dbo.PAY_GETP.CUST_NO, dbo.PAY_GETP.VAZ, dbo.PAY_GETP.KIND, 
                                                                     dbo.PAY_GETP.HES1, dbo.PAY_GETP.HES2, dbo.PAY_GETP.HES3, dbo.PAY_GETP.CRT, dbo.PAY_GETP.UID, dbo.PAY_GETP.SAYADI, dbo.PAY_GETP.ID,
                                                                         (SELECT NAME FROM dbo.TOTA_HES WHERE (NUMBER = dbo.PAY_GETP.N_KOL)) AS N_KOL_NAME,
                                                                         (SELECT NAME FROM dbo.TOTA_HES AS TOTA_HES_2 WHERE (NUMBER = dbo.PAY_GETP.N_KOL2)) AS N_KOL2_NAME,
                                                                         (SELECT NAME FROM dbo.TOTA_HES AS TOTA_HES_1 WHERE (NUMBER = dbo.PAY_GETP.N_KOL3)) AS N_KOL3_NAME,
                                                                         (SELECT NAME FROM dbo.DETA_HES WHERE (N_KOL = dbo.PAY_GETP.N_KOL) AND (NUMBER = dbo.PAY_GETP.N_MOIN)) AS N_MOIN_NAME,
                                                                         (SELECT NAME FROM dbo.DETA_HES AS DETA_HES_2 WHERE (N_KOL = dbo.PAY_GETP.N_KOL2) AND (NUMBER = dbo.PAY_GETP.N_MOIN2)) AS N_MOIN2_NAME,
                                                                         (SELECT NAME FROM dbo.DETA_HES AS DETA_HES_1 WHERE (N_KOL = dbo.PAY_GETP.N_KOL3) AND (NUMBER = dbo.PAY_GETP.N_MOIN3)) AS N_MOIN3_NAME,
                                                                         (SELECT NAME FROM dbo.TDETA_HES WHERE (N_KOL = dbo.PAY_GETP.N_KOL) AND (NUMBER = dbo.PAY_GETP.N_MOIN) AND (TNUMBER = dbo.PAY_GETP.N_TAF)) AS N_TAF_NAME,
                                                                         (SELECT NAME  FROM dbo.TDETA_HES AS TDETA_HES_2 WHERE (N_KOL = dbo.PAY_GETP.N_KOL2) AND (NUMBER = dbo.PAY_GETP.N_MOIN2) AND (TNUMBER = dbo.PAY_GETP.N_TAF2)) AS N_TAF2_NAME,
                                                                         (SELECT NAME FROM dbo.TDETA_HES AS TDETA_HES_1 WHERE (N_KOL = dbo.PAY_GETP.N_KOL3) AND (NUMBER = dbo.PAY_GETP.N_MOIN3) AND (TNUMBER = dbo.PAY_GETP.N_TAF3)) AS N_TAF3_NAME, dbo.TCOD_BANKS.NAMES AS BANK_NAME
                                                                  FROM dbo.PAY_GETP LEFT OUTER JOIN
                                                                     dbo.TCOD_BANKS ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE
                                                                  ORDER BY dbo.PAY_GETP.N_SERI").ToList();
            foreach (var item in MasterHead)
            {

                PAY_GETP_DATA.Add(item);
            }
        }
        void _ReGetData0()
        {
            // Clear the existing data to prepare for new data load
            PAY_GETP_DATA?.Clear();

            // Step 1: Fetch all data in one query to minimize database calls
            var masterHead = dbms.DoGetDataSQL<PAY_GETP_MODEL>(
                "SELECT N_SERI, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, N_S, N_KOL, N_MOIN, N_TAF, N_KOL2, N_MOIN2, N_TAF2, N_KOL3, N_MOIN3, N_TAF3, NUMBER, TAG, ANBAR, RADIF, CUST_NO, VAZ, LIST_NO, KIND, SANDUGH, HES1, HES2, HES3, ESTELAM, CRT, UID, SAYADI, ID FROM dbo.PAY_GETP ORDER BY N_SERI ASC"
            ).ToList();

            // Dictionary to cache results of GET_NAME_HES to avoid repeated database calls
            Dictionary<string, string> nameCache = new Dictionary<string, string>();

            // HashSet to collect unique codes that need to be queried
            var uniqueCodes = new HashSet<string>();

            // Step 2: Iterate over each item and gather unique account codes
            foreach (var item in masterHead)
            {
                if (item != null)
                {
                    // Add the N_KOL to the set of codes needing name resolution
                    uniqueCodes.Add(item.N_KOL.ToStringNullSafe());

                    // Construct the KOL-MOIN code string
                    var kolMoin = item.N_KOL + "-" + item.N_MOIN;

                    // Add the KOL-MOIN composite to the set
                    uniqueCodes.Add(kolMoin);
                }
            }

            // Step 3: Execute GET_NAME_HES for each unique code once and store results
            foreach (var code in uniqueCodes)
            {
                // Retrieve name for each unique code and cache the result
                var name = CL_LMethods.GET_NAME_HES(code, dbms);
                if (!string.IsNullOrEmpty(name))
                {
                    nameCache[code] = name; // Cache the result to avoid future database calls
                }
            }

            // Step 4: Assign cached name results back to each item in the master list
            foreach (var item in masterHead)
            {
                if (item != null)
                {
                    // Retrieve the cached name for N_KOL
                    nameCache.TryGetValue(item.N_KOL.ToStringNullSafe(), out var nKolName);
                    item.N_KOL_NAME = nKolName;

                    // Retrieve the cached name for combined KOL-MOIN
                    var kolMoin = item.N_KOL + "-" + item.N_MOIN;
                    nameCache.TryGetValue(kolMoin, out var nMoinName);
                    item.N_MOIN_NAME = nMoinName;
                }

                // Add the item back to the data collection
                PAY_GETP_DATA.Add(item);
            }
        }

        #region _SfDataGrid_
        private readonly FilterService<PAY_GETP_MODEL> filterService = new FilterService<PAY_GETP_MODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool NowIsReady { get; private set; }

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        private void PAY_GETP_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void PAY_GETP_SUB_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            //// Get the selected row and column index
            var currentCell = PAY_GETP_SUB.SelectionController.CurrentCellManager.CurrentCell;
            if (currentCell != null)
            {
                var rowColumnIndex = new RowColumnIndex(currentCell.RowIndex, currentCell.ColumnIndex);
                UpdateCurrentCellValue(rowColumnIndex);
            }

        }
        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex)
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.PAY_GETP_SUB.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.PAY_GETP_SUB.Columns[columnIndex].MappingName;
            var recordIndex = this.PAY_GETP_SUB.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.PAY_GETP_SUB.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell

            if (!string.IsNullOrEmpty(selectedText))
            {
                // Add the Contains filter to the filter service (inclusion filter)
                filterService.AddFilter(columnName, selectedText, isExclusion: false); // False means it's an inclusion filter
                ActiveFilters.Add($"{columnName} Contains {selectedText}");
                // Apply the cumulative filter to the data grid
                ApplyCumulativeFilter();
            }
            else
            {
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

        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            if (!string.IsNullOrEmpty(selectedText))
            {
                var (columnName, filterValue) = GetSelectedCellDetails(); // Get the details of the selected cell
                if (filterValue != null)
                {
                    // Add the Not Contains filter to the filter service (exclusion filter)
                    filterService.AddFilter(columnName, selectedText, isExclusion: true); // True means it's an exclusion filter
                                                                                          // Add the exclusion filter to the list of active filters
                    ActiveFilters.Add($"{columnName} Does Not Contain {selectedText}");
                    // Apply the cumulative filter to the data grid
                    ApplyCumulativeFilter();
                }
            }
            else
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
        }
        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e)
        {
            // Clear all filters in the filter service
            filterService.ClearFilters();
            // Clear the list of active filters
            ActiveFilters.Clear();
            // Apply the cumulative filter to the data grid
            ApplyCumulativeFilter();
        }
        private (string ColumnName, object FilterValue) GetSelectedCellDetails()
        {
            // Check if there is a current cell selected in the data grid
            if (PAY_GETP_SUB.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = PAY_GETP_SUB.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
                                                                                                                         // Return the column name and the current cell value
                                                                                                                         //if (CurrentCellValue == null)
                                                                                                                         //{
                                                                                                                         //    return (columnName, SelectedSfDgTextCell);
                                                                                                                         //}
                                                                                                                         //else
                {
                    return (columnName, CurrentCellValue);
                }
            }
            return (null, null); // If no cell is selected, return null values
        }
        private void ApplyCumulativeFilter()
        {
            // Set the filter for the data grid view using the filter service
            PAY_GETP_SUB.View.Filter = item => filterService.ApplyFilter(item as PAY_GETP_MODEL);
            // Refresh the filter to update the view
            PAY_GETP_SUB.View.RefreshFilter();
        }

        private void PAY_GETP_SUB_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }
        private void PAY_GETP_SUB_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }

            return;

            var point = e.GetPosition(PAY_GETP_SUB);

            // Get the element under the mouse
            var hitElement = e.OriginalSource as DependencyObject;
            if (hitElement == null) return;

            // Find the cell element
            var cell = FindParent<Syncfusion.UI.Xaml.Grid.GridCell>(hitElement);
            if (cell == null) return;

            // Check if the cell is in edit mode
            if (PAY_GETP_SUB.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
            {
                var editingElement = PAY_GETP_SUB.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    // Capture the selected text instead of the full text
                    SelectedSfDgTextCell = editingElement.SelectedText;
                }
            }

            ////// Get the DataGrid and the VisualContainer
            //var visualContainer = PAY_GETP_SUB?.GetVisualContainer();

            //if (visualContainer == null) return;

            //// Get the position of the mouse click
            //var position = e.GetPosition(visualContainer);

            //// Get the cell's RowColumnIndex
            //var cellIndex = visualContainer.PointToCellRowColumnIndex(position);
            //if (cellIndex.RowIndex < 0 || cellIndex.ColumnIndex < 0) return;

            //var rowColumnIndex = new RowColumnIndex(cellIndex.RowIndex, cellIndex.ColumnIndex);
            //UpdateCurrentCellValue(rowColumnIndex);

            //// Check if the cell is in edit mode
            //if (PAY_GETP_SUB.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
            //{
            //    var editingElement = PAY_GETP_SUB.FindElementOfType<TextBox>();
            //    if (editingElement != null)
            //    {
            //        CurrentCellValue = editingElement.Text; // Update with the text being edited
            //    }
            //}

        }
        private string GetSelectedText()
        {
            var dataGrid = PAY_GETP_SUB;
            var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;

            if (currentCell != null && currentCell.IsEditing)
            {
                // Find the editing element (which will be a TextBox in edit mode)
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    return editingElement.SelectedText; // Return the selected text
                }
            }
            return string.Empty;
        }
        #endregion

        private void PAY_GETP_SUB_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            var column = PAY_GETP_SUB.Columns["PERSON_NAME"] as GridTextColumn;
            if (column != null)
            {
                var currentCell = PAY_GETP_SUB.SelectionController.CurrentCellManager.CurrentCell;
                if (currentCell != null && currentCell.IsEditing)
                {
                    var editingElement = PAY_GETP_SUB.FindElementOfType<TextBox>();
                    if (editingElement != null)
                    {
                        editingElement.MaxLength = 10;
                    }
                }
            }
        }
        private void PAY_GETP_SUB_CurrentCellValidating(object sender, CurrentCellValidatingEventArgs e)
        {
            if (e.Column.MappingName == "PERSON_NAME")
            {
                string? value = e.NewValue?.ToString();
                if (value != null && value.Length > 10) //Max Length
                {
                    e.IsValid = false;
                    e.ErrorMessage = "Maximum length exceeded (10 characters)";
                }
            }
        }

        private void BTN_DELETE_PAYCH_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = PAY_GETP_SUB.SelectedItem as PAY_GETP_MODEL;

            if (CurrentRow != null && CurrentRow?.ID != null)
            {
                Msgwin msgwin = new Msgwin(true, "آیا از حذف اطمینان دارید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == true)
                {
                    try
                    {
                        var rst0 = dbms.DoGetDataSQL<dynamic>("SELECT PAY_GETP.N_SERI, PAY_GETP.BANK, PGET_LST.DATE FROM PAY_GETP INNER JOIN PGET_LST ON (PAY_GETP.BANK = PGET_LST.BANK) AND (PAY_GETP.N_SERI = PGET_LST.N_SERI) WHERE (((PAY_GETP.N_SERI)=" + CurrentRow.N_SERI + ") AND ((PAY_GETP.BANK)=" + CurrentRow.BANK + "))").FirstOrDefault();
                        if (rst0 != null)
                        {
                            new Msgwin(false, " اين چك در خزانه داري مورخ  " + Strings.Format(rst0.DATE, "####/##/##") + " داراي گردش مي باشدو نمي توانيد آن را حذف كنيد...!").ShowDialog();
                            return;
                        }
                        else
                        {
                            var rst = dbms.DoGetDataSQL<double?>("SELECT dbo.DEED_DTL.N_S FROM dbo.PAY_GETP INNER JOIN  dbo.DEED_DTL ON dbo.PAY_GETP.N_SERI = dbo.DEED_DTL.N_SERI AND dbo.PAY_GETP.BANK = dbo.DEED_DTL.BANK WHERE (dbo.PAY_GETP.N_SERI = " + CurrentRow.N_SERI + ") And (dbo.PAY_GETP.BANK = " + CurrentRow.BANK + ") GROUP BY dbo.DEED_DTL.N_S").FirstOrDefault();
                            if (rst != null)
                            {
                                new Msgwin(false, "  اين چك در سند شماره " + rst + " داراي گردش مي باشدو نمي توانيد آن را حذف كنيد...!").ShowDialog();
                                return;
                            }
                            else
                            {
                                CurrentRow.N_KOL = 911;
                                CurrentRow.N_MOIN = 1;
                                CurrentRow.N_TAF = 1;
                                CurrentRow.N_KOL2 = 911;
                                CurrentRow.N_MOIN2 = 1;
                                CurrentRow.N_TAF2 = 1;
                                CurrentRow.N_KOL3 = null;
                                CurrentRow.N_MOIN3 = null;
                                CurrentRow.N_TAF3 = null;

                                DoCmdSavePAY_GETP(CurrentRow);

                                new Msgwin(false, "چك بصورت مجازي از دفتر چك حذف گرديد").ShowDialog();
                            }
                        }

                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                        {
                            new Msgwin(false, "این چک دارای گردش است و نمیتوان آنرا حذف کرد").ShowDialog();
                        }
                        else
                        {
                            new Msgwin(false, "خطا در انجام عملیات حذف چک").ShowDialog();
                        }
                    }
                    catch (Exception)
                    {
                        new Msgwin(false, "خطا در انجام عملیات حذف چک").ShowDialog();
                    }
                }
            }
        }

        private void BTN_CLEAR_VOSUL_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = PAY_GETP_SUB.SelectedItem as PAY_GETP_MODEL;

            if (CurrentRow != null && CurrentRow?.ID != null)
            {
                if (CurrentRow.BANK == null || string.IsNullOrEmpty(CurrentRow.N_SERI.ToStringNullSafe()))
                {
                    return;
                }

                var rst = dbms.DoGetDataSQL<double?>("SELECT N_S FROM dbo.DEED_DTL WHERE (HES = '" + Baseknow.APA + "' OR HES = '" + Baseknow.APV + "' ) AND (BES > 0) AND (BANK = " + CurrentRow.BANK + ") AND (N_SERI = " + CurrentRow.N_SERI + ")").FirstOrDefault();
                if (rst != null)
                {
                    new Msgwin(false, "اين چك در سند شماره " + rst + " داراي گردش بستانكار است و نمي توانيد حساب واگذاري يا برگشتي يا وصولي آن را حذف نماييد").ShowDialog();
                    return;
                }
                else
                {
                    if (CurrentRow.N_KOL.ToStringNullSafe() != Baseknow.BANKHA.ToStringNullSafe())
                    {
                        CurrentRow.N_KOL = null;
                        CurrentRow.N_MOIN = null;
                        CurrentRow.N_TAF = null;
                        CurrentRow.HES1 = null;
                    }
                    CurrentRow.N_KOL2 = null;
                    CurrentRow.N_MOIN2 = null;
                    CurrentRow.N_TAF2 = null;
                    CurrentRow.N_KOL3 = null;
                    CurrentRow.N_MOIN3 = null;
                    CurrentRow.N_TAF3 = null;
                    CurrentRow.N_S = null;
                    CurrentRow.HES2 = null;
                    CurrentRow.HES3 = null;
                }

                DoCmdSavePAY_GETP(CurrentRow);
            }
        }

        private bool DoCmdSavePAY_GETP(PAY_GETP_MODEL _ROW_)
        {
            try
            {
                dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETP SET 
                                     N_S = {(string.IsNullOrEmpty(_ROW_.N_S.ToStringNullSafe()) ? "NULL" : _ROW_.N_S.ToString())},
                                     N_KOL = {(string.IsNullOrEmpty(_ROW_.N_KOL.ToStringNullSafe()) ? "NULL" : _ROW_.N_KOL.ToString())},
                                     N_MOIN = {(string.IsNullOrEmpty(_ROW_.N_MOIN.ToStringNullSafe()) ? "NULL" : _ROW_.N_MOIN.ToString())},
                                     N_TAF = {(string.IsNullOrEmpty(_ROW_.N_TAF.ToStringNullSafe()) ? "NULL" : _ROW_.N_TAF.ToString())},
                                     N_KOL2 = {(string.IsNullOrEmpty(_ROW_.N_KOL2.ToStringNullSafe()) ? "NULL" : _ROW_.N_KOL2.ToString())},
                                     N_MOIN2 = {(string.IsNullOrEmpty(_ROW_.N_MOIN2.ToStringNullSafe()) ? "NULL" : _ROW_.N_MOIN2.ToString())},
                                     N_TAF2 = {(string.IsNullOrEmpty(_ROW_.N_TAF2.ToStringNullSafe()) ? "NULL" : _ROW_.N_TAF2.ToString())},
                                     N_KOL3 = {(string.IsNullOrEmpty(_ROW_.N_KOL3.ToStringNullSafe()) ? "NULL" : _ROW_.N_KOL3.ToString())},
                                     N_MOIN3 = {(string.IsNullOrEmpty(_ROW_.N_MOIN3.ToStringNullSafe()) ? "NULL" : _ROW_.N_MOIN3.ToString())},
                                     N_TAF3 = {(string.IsNullOrEmpty(_ROW_.N_TAF3.ToStringNullSafe()) ? "NULL" : _ROW_.N_TAF3.ToString())}
                                  WHERE ID = {_ROW_.ID}");

                universControl.PopNotifyShow("وضعیت چک ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "خطا چک تکراری در ذخیره وضعیت چک").ShowDialog();
                    return false;
                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات ذخیره وضعیت این چک دریافتی!").ShowDialog();
                return false;
            }

            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = PAY_GETP_SUB.SelectedItem as PAY_GETP_MODEL;

            if (CurrentRow != null && CurrentRow?.N_SERI != null)
            {
                string _WHERE_ = $" WHERE N_S IN ( (SELECT N_S FROM dbo.DEED_DTL WHERE SHARH LIKE N'%{CurrentRow.N_SERI}%') )  ";
                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD_LIST, this, _WHERE_);
            }
        }
    }
}
