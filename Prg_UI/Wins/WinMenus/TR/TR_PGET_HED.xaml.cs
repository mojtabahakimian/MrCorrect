using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.TR
{
    public partial class TR_PGET_HED : Window
    {
        #region Models
        // Extended Header Model for History (Inherits from your PGET_HED)
        public class TR_PGET_HED_EXT : PGET_HED
        {
            // History Specific Fields
            public string? UP_DATE { get; set; }
            public double? UP_TIME { get; set; }
            public string? UP_USER_NAME { get; set; }
            public string? PC_NAME { get; set; }
            public string? IPADD { get; set; }

            public int? ID { get; set; }
            public long? DATE { get; set; }
            public string? MOLAH { get; set; }
            public double? N_S { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string? USER_NAME { get; set; }
            public short? KIND { get; set; }
            public int? IDK { get; set; }
            public bool? OKF { get; set; }
            public int? RPLICA { get; set; }
            public bool? SGN1 { get; set; }
            public bool? SGN2 { get; set; }
            public bool? SGN3 { get; set; }
            public int? sgn1usid { get; set; }
            public int? sgn2usid { get; set; }
            public int? sgn3usid { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }

            public string UpTimeDisplay
            {
                get
                {
                    if (UP_TIME.HasValue)
                    {
                        try
                        {
                            return DateTime.FromOADate(UP_TIME.Value).ToString("HH:mm:ss");
                        }
                        catch { return UP_TIME.Value.ToString(); }
                    }
                    return "";
                }
            }
        }
        public class TR_PGET_LST
        {
            public int? ID { get; set; }
            public long? DATE { get; set; }
            public double? RADIF { get; set; }
            public int? NO_AM { get; set; }
            public double? NAHVA { get; set; }
            public int? FHES_K { get; set; }
            public int? FHES_M { get; set; }
            public int? FHES_T { get; set; }
            public int? THES_K { get; set; }
            public int? THES_M { get; set; }
            public int? THES_T { get; set; }
            public string? SHARH { get; set; }
            public double? MABL { get; set; }
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public int? IDH { get; set; }
            public string? FHES { get; set; }
            public string? THES { get; set; }
            public double? ARZD { get; set; }
            public int? FHES_T2 { get; set; }
            public int? THES_T2 { get; set; }
            public int? FHES_T3 { get; set; }
            public int? THES_T3 { get; set; }
            public int? FHES_T4 { get; set; }
            public int? THES_T4 { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }

            // Extra display properties
            public string? NAME_FHES { get; set; }
            public string? NAME_THES { get; set; }

            // History fields
            public long? UP_DATE { get; set; }
            public double? UP_TIME { get; set; }
            public string? UP_USER_NAME { get; set; }
        }
        public class TR_PAY_GETD
        {
            public string? N_SERI { get; set; }
            public string? DATE_S { get; set; }
            public double? MABL { get; set; }
            public string? BANK_NAME { get; set; }
            public string? SHOBEH { get; set; }
            public string? N_HESAB { get; set; }
            public string? SAYADI { get; set; }
            public int? BANK { get; set; }
        }

        public class TreasuryFullDetails
        {
            public TR_PGET_HED_EXT Header { get; set; }
            public List<PGET_LST> Rows { get; set; } = new List<PGET_LST>();
            public List<TR_PAY_GETD> Checks { get; set; } = new List<TR_PAY_GETD>();
        }
        #endregion

        #region Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
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
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public ObservableCollection<TR_PGET_HED_EXT> HISTORY_DATA { get; set; } = new ObservableCollection<TR_PGET_HED_EXT>();
        public ObservableCollection<PGET_LST> ROW_DATA { get; set; } = new ObservableCollection<PGET_LST>(); //TR_PGET_LST
        public ObservableCollection<TR_PAY_GETD> CHECK_DATA { get; set; } = new ObservableCollection<TR_PAY_GETD>();

        public bool NowIsReady { get; private set; }
        public byte PARAMS { get; private set; }

        public TR_PGET_HED(byte? _PARAM_ = null)
        {
            InitializeComponent();
            this.DataContext = this;

            if (_PARAM_ != null)
                PARAMS = (byte)_PARAM_;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string formName = "";
            switch (PARAMS)
            {
                case 1:
                    WINTILENAME.Content = "سابقه برگه های دریافت (خزانه)";
                    formName = "TR_PGET_D";
                    break;
                case 2:
                    WINTILENAME.Content = "سابقه برگه های پرداخت (خزانه)";
                    formName = "TR_PGET_P";
                    break;
                default:
                    WINTILENAME.Content = "سابقه خزانه داری";
                    break;
            }

            #region Security Check
            if (!string.IsNullOrWhiteSpace(formName))
            {
                try
                {
                    var helper = new WindowInteropHelper(this);
                    helper.EnsureHandle();
                    CL_HESABDARI.SETSECURITY(this.GetType().Name, formName, helper.Handle, this.GetType().Name);
                    if (!this.IsLoaded) { this.Close(); return; }
                }
                catch { try { this.Close(); } catch { } }
            }
            #endregion

            FILL_ALL_COMBOBOXES();

            ReGetHeadMaster();

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();
                UpdateRowCountLabel();
            }

            SYNCFUSION_DG.Visibility = Visibility.Visible;

            SetupGridNavigation();
            AttachRecordCountUpdater(PGET_LST_SUB, TXT_COUNT_ROWS);
            AttachRecordCountUpdater(PAY_GETD_SUB, TXT_COUNT_CHECKS);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            //نوع عملیات
            NO_AM_COL.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>("SELECT TCOD_DPS.CODE, TCOD_DPS.NAMES FROM TCOD_DPS ORDER BY TCOD_DPS.CODE, TCOD_DPS.NAMES").ToList();
            NO_AM_COL.DisplayMemberPath = "NAMES";
            NO_AM_COL.SelectedValuePath = "CODE";

            //نحوه
            NAHVA_COL.ItemsSource = dbms.DoGetDataSQL<TCOD_DPSKIND>("SELECT TCOD_DPSKIND.CODE, TCOD_DPSKIND.NAMES FROM TCOD_DPSKIND ORDER BY TCOD_DPSKIND.CODE, TCOD_DPSKIND.NAMES").ToList();
            NAHVA_COL.DisplayMemberPath = "NAMES";
            NAHVA_COL.SelectedValuePath = "CODE";
        }

        private void ReGetHeadMaster()
        {
            HISTORY_DATA?.Clear();

            string WhereCondition = PARAMS > 0 ? $" WHERE (TAG = {PARAMS}) " : " ";

            // Optimized Query selecting ALL columns into the Extended Model
            var query = $@"
                    SELECT *
                    FROM dbo.TR_PGET_HED
                    {WhereCondition}
                    ORDER BY UP_DATE DESC, UP_TIME DESC";

            var data = dbms.DoGetDataSQL<TR_PGET_HED_EXT>(query).ToList();

            foreach (var item in data)
            {
                HISTORY_DATA?.Add(item);
            }
        }

        private async Task<TreasuryFullDetails> GetTreasuryFullDetailsAsync(TR_PGET_HED_EXT TrRow)
        {
            var fullDetails = new TreasuryFullDetails();
            const int ROUND_PRECISION = 6;

            string sql = $@"
        -- 1. List Details with Account Names
        SELECT 
            l.*,
            ch1.NAME AS NAME_FHES,
            ch2.NAME AS NAME_THES
        FROM dbo.TR_PGET_LST l
        LEFT JOIN dbo.CUST_HESAB ch1 ON l.FHES = ch1.hes
        LEFT JOIN dbo.CUST_HESAB ch2 ON l.THES = ch2.hes
        WHERE l.ID = @ID 
          AND l.UP_DATE = @UpDate 
          AND ROUND(l.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

        -- 2. Details Snapshot
        -- Matching Details that share the same N_S and History Timestamp
        SELECT * FROM dbo.TR_PAY_GETD 
        WHERE N_S = @N_S 
          AND UP_DATE = @UpDate 
          AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});
    ";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);

            var parameters = new
            {
                ID = TrRow.ID,
                N_S = TrRow.N_S,
                UpDate = TrRow.UP_DATE,
                UpTime = TrRow.UP_TIME ?? 0
            };

            using (var multi = await db.QueryMultipleAsync(sql, parameters))
            {
                fullDetails.Rows = (await multi.ReadAsync<PGET_LST>()).ToList();

                fullDetails.Checks = (await multi.ReadAsync<TR_PAY_GETD>()).ToList();
            }

            // Set the header from the passed parameter
            fullDetails.Header = TrRow;

            return fullDetails;
        }

        private async void ReGetData(TR_PGET_HED_EXT row)
        {
            if (row == null) return;

            var dataFetchTask = GetTreasuryFullDetailsAsync(row);
            var delayTask = Task.Delay(300);
            var completedTask = await Task.WhenAny(dataFetchTask, delayTask);

            bool loaderShown = false;
            if (completedTask == delayTask)
            {
                BusyOverlay.Visibility = Visibility.Visible;
                loaderShown = true;
            }

            try
            {
                var details = await dataFetchTask;

                if (details == null || details.Header == null)
                {
                    if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
                    return;
                }

                ROW_DATA.Clear();
                details.Rows.ForEach(ROW_DATA.Add);

                CHECK_DATA.Clear();
                details.Checks.ForEach(CHECK_DATA.Add);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری جزئیات").ShowDialog();
            }
            finally
            {
                if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
            }
        }

        #region Grid Events & Navigation
        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (!NowIsReady) return;

            if (SYNCFUSION_DG.SelectedItem is TR_PGET_HED_EXT selected)
            {
                if (selected.ID != null)
                {
                    ReGetData(selected);
                }
                else
                {
                    ROW_DATA.Clear();
                    CHECK_DATA.Clear();
                }
            }
        }

        #region _SfDataGrid_
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }
        private void UpdateRowCountLabel()
        {
            //// Defensive checks
            //if (ROWCOUNT_TEXTBLK == null) return;
            //if (SYNCFUSION_DG?.View == null) return;

            //// Safely retrieve the record count
            //var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;

            //// Set the label content
            //ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
        }

        private readonly FilterService<TR_PGET_HED_EXT> filterService = new FilterService<TR_PGET_HED_EXT>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        public bool IsExporty { get; private set; } = false;

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private bool isFactory = false;

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }

        private void UpdateCurrentCellValue(RowColumnIndex rowColumnIndex) // Method to update the current cell value
        {
            CurrentCellIndex = rowColumnIndex; // Update current cell index
            CurrentCellValue = null; // Reset current cell value

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0)
            {
                return;
            }

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);


            if (record == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(mappingName))
            {
                return;
            }
            var property = record.GetType().GetProperty(mappingName);
            if (property == null)
            {
                //Console.WriteLine("Property " + mappingName + " not found on type " + record.GetType().Name);
                return;
            }

            //CurrentCellValue = property.GetValue(record)?.ToString();
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController?.CurrentCellManager?.CurrentCell;

            if (currentCell == null)
                return string.Empty;

            // حالت 1: Edit Mode
            if (currentCell.IsEditing)
            {
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null && !string.IsNullOrEmpty(editingElement.SelectedText))
                {
                    return editingElement.SelectedText;
                }
            }

            // حالت 2: جستجوی ساده - بدون GetCellElement
            try
            {
                var gridCellElement = currentCell?.ColumnElement;
                if (gridCellElement != null)
                {
                    var textBox = FindVisualChild<TextBox>(gridCellElement);
                    if (textBox != null && !string.IsNullOrWhiteSpace(textBox.SelectedText))
                    {
                        return textBox.SelectedText;
                    }
                }
            }
            catch { }

            return string.Empty;
        }
        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر Contains
                filterService.AddFilter(columnName, selectedText, isExclusion: false, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Contains \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است
            if (filterValue != null)
            {
                // فیلتر Exact Match
                filterService.AddFilter(columnName, filterValue, isExclusion: false, isExactMatch: true);

                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} = {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // فیلتر برای null values
                filterService.AddFilter(columnName, null, isExclusion: false, isExactMatch: true);
                ActiveFilters.Add($"{columnName} = NULL");
                ApplyCumulativeFilter();
            }
        }
        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedText = GetSelectedText();
            var (columnName, filterValue) = GetSelectedCellDetails();

            // اگر ستون یا مقدار معتبر نیست، خروج
            if (string.IsNullOrEmpty(columnName))
            {
                universControl.PopNotifyShow("لطفاً یک سلول انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            // حالت 1: بخشی از متن انتخاب شده است (partial selection)
            if (!string.IsNullOrEmpty(selectedText))
            {
                // فیلتر "Does Not Contain" - برای متن
                filterService.AddFilter(columnName, selectedText, isExclusion: true, isExactMatch: false);
                ActiveFilters.Add($"{columnName} Does Not Contain \"{selectedText}\"");
                ApplyCumulativeFilter();
                return;
            }

            // حالت 2: کل سلول انتخاب شده است (exact value)
            if (filterValue != null)
            {
                // فیلتر Exclusion با Exact Match - برای مقدار دقیق
                filterService.AddFilter(columnName, filterValue, isExclusion: true, isExactMatch: true);

                // نمایش بهتر در لیست فیلترها
                string displayValue = FormatValueForDisplay(filterValue);
                ActiveFilters.Add($"{columnName} != {displayValue}");

                ApplyCumulativeFilter();
            }
            else
            {
                // اگر مقدار null است
                filterService.AddFilter(columnName, null, isExclusion: true, isExactMatch: true);
                ActiveFilters.Add($"{columnName} != NULL");
                ApplyCumulativeFilter();
            }
        }
        private string FormatValueForDisplay(object value)
        {
            if (value == null)
                return "NULL";

            // برای مقادیر عددی، فرمت هزارگان اعمال می‌شود
            if (value is double || value is decimal || value is float)
            {
                try
                {
                    return Convert.ToDecimal(value).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            if (value is int || value is long || value is short || value is byte)
            {
                try
                {
                    return Convert.ToInt64(value).ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    return value.ToString();
                }
            }

            return value.ToString();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as TR_PGET_HED_EXT);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();

            UpdateRowCountLabel();
        }
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShow("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }
            }
            catch { return; }

            // Check if there are selected rows
            if (SYNCFUSION_DG.SelectedItems == null || !SYNCFUSION_DG.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();

            try
            {
                // Add headers
                foreach (var column in SYNCFUSION_DG.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in SYNCFUSION_DG.SelectedItems)
                {
                    foreach (var column in SYNCFUSION_DG.Columns)
                    {
                        if (!column.IsHidden) // Include only columns that are not hidden
                        {
                            var propertyValue = item.GetType().GetProperty(column.MappingName)?.GetValue(item, null);
                            sb.Append(propertyValue?.ToString() + "\t");
                        }
                    }
                    sb.AppendLine();
                }

                // Copy to clipboard
                Clipboard.SetText(sb.ToString());
                universControl.PopNotifyShow($"{SYNCFUSION_DG.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true; // Mark event as handled
            }
        }
        private void CalculateSumForCurrentColumn(SfDataGrid _DG_)
        {
            // Ensure rows are selected
            if (_DG_.SelectedItems == null || _DG_.SelectedItems.Count == 0)
            {
                return;
            }

            // Detect the current column
            var currentColumn = _DG_.CurrentColumn;
            if (currentColumn == null)
            {
                return;
            }

            string columnName = currentColumn.MappingName; // Get the column name
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            decimal sum = 0;
            bool isNumericColumn = false;

            // Iterate through the selected rows
            foreach (var selectedItem in _DG_.SelectedItems)
            {
                // Get the cell value for the detected column
                var cellValue = GetCellValue(selectedItem, columnName);

                if (cellValue != null && decimal.TryParse(cellValue.ToStringNullSafe(), out decimal numericValue))
                {
                    sum += numericValue;
                    isNumericColumn = true;
                }
            }

            if (isNumericColumn)
            {
                string formattedSum = sum.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

                new Msgwin(false, $"جمع سطر های انتخاب شده در ستون [{currentColumn.HeaderText}] برار است با : {formattedSum}").ShowDialog();

            }
        }
        private object GetCellValue(object record, string columnName)
        {
            try
            {
                // Use reflection to get the property value from the record
                var property = record.GetType().GetProperty(columnName);
                return property?.GetValue(record);
            }
            catch
            {
                return null;
            }
        }
        public void GenerateAutomaticSummary(SfDataGrid _DG_, bool _ClearAnySummaryBefore_ = false)
        {
            if (_ClearAnySummaryBefore_)
            {
                SYNCFUSION_DG.TableSummaryRows.Clear();
            }
            else
            {
                // Check if a summary row already exists
                if (_DG_.TableSummaryRows.Count > 0)
                {
                    return; // Exit the method if a summary row already exists
                }
            }

            var summaryRow = new GridTableSummaryRow();
            summaryRow.ShowSummaryInRow = false;
            summaryRow.Position = TableSummaryRowPosition.Bottom;

            var summaryColumns = new ObservableCollection<ISummaryColumn>();

            var dataType = typeof(TR_PGET_HED_EXT);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(TR_PGET_HED_EXT).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "meghk" || column.MappingName.ToLower() == "mablk"))
                {
                    var summaryColumn = new GridSummaryColumn
                    {
                        Name = column.MappingName + "Sum",
                        MappingName = column.MappingName,
                        SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate,
                        //Format = "{Sum:N0}"
                        Format = "{Sum:N0}"
                    };
                    summaryColumns.Add(summaryColumn);
                }
            }

            summaryRow.SummaryColumns = summaryColumns;

            _DG_.TableSummaryRows.Add(summaryRow);


        }
        private bool IsNumericType(Type type)
        {
            if (type == null)
                return false;

            // Handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            // Handle object type that might represent a number
            if (type == typeof(object))
            {
                return true; // Assume it might be numeric
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }


        #region Navigation Logic
        // 1. این متد را در انتهای Window_Loaded صدا بزنید
        // --- Safe Navigation Logic ---

        private void SetupGridNavigation()
        {
            // 1. بررسی ایمنی: اگر کنترل‌ها هنوز ساخته نشده‌اند، خارج شو
            // این خط جلوی خطای NullReference را می‌گیرد اگر XAML آپدیت نشده باشد
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null)
            {
                // جهت دیباگ: اگر این خط اجرا شد یعنی یکی از نام‌ها در XAML اشتباه است
                return;
            }

            // 2. اتصال رویداد تغییر انتخاب (Selection)
            // ابتدا حذف می‌کنیم تا دوبار متصل نشود (-=)
            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;

            // 3. اتصال رویداد تغییر تعداد رکوردها (Collection Changed)
            // نکته مهم: بررسی می‌کنیم که View آماده است یا نه
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                // اگر ویو آماده بود، وصل شو
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
            }
            else
            {
                // اگر ویو هنوز نال بود، به رویداد Loaded خود گرید وصل می‌شویم تا بعدا انجام دهیم
                SYNCFUSION_DG.Loaded -= OnGridLoadedForNav;
                SYNCFUSION_DG.Loaded += OnGridLoadedForNav;
            }

            // 4. آپدیت اولیه متن‌ها (با بررسی نال)
            UpdateNavigationText();
        }

        // اگر گرید در ابتدا آماده نبود، این متد بعداً صدا زده می‌شود
        private void OnGridLoadedForNav(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((System.Collections.Specialized.INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
                UpdateNavigationText();
            }
        }

        // هندلرهای کمکی برای جلوگیری از خطای ترد
        private void OnNavSelectionChanged(object sender, GridSelectionChangedEventArgs e) => UpdateNavigationText();
        private void OnNavCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => UpdateNavigationText();

        private void UpdateNavigationText()
        {
            // بررسی ایمنی مجدد
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            int total = 0;
            int current = 0;

            try
            {
                // محاسبه تعداد کل (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
                {
                    total = SYNCFUSION_DG.View.Records.Count;
                }

                // محاسبه ایندکس جاری (ایمن)
                if (SYNCFUSION_DG != null && SYNCFUSION_DG.SelectedIndex >= 0)
                {
                    current = SYNCFUSION_DG.SelectedIndex + 1;
                }
            }
            catch
            {
                // نادیده گرفتن خطا در شرایط خاص
            }

            // نمایش
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        // 3. رویدادهای کلیک دکمه‌ها
        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAllSfDataFilters(); // حذف فیلترها
                ReGetHeadMaster();
                Btn_First_Click(default, default);
            }
            catch { }
        }
        private void Btn_First_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    SYNCFUSION_DG.SelectedIndex = 0;
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(1, 0));
                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToHome();
                }
            }
            catch { }
        }

        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex > 0)
                {
                    SYNCFUSION_DG.SelectedIndex--;
                    // اسکرول به ایندکس جدید (ایندکس رکورد + هدرها)
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(SYNCFUSION_DG.SelectedIndex + 1, 0));
                    //var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);
                    //SYNCFUSION_DG.GetVisualContainer().ScrollRows.ScrollInView(rowIndex, 0);

                    SYNCFUSION_DG.SelectedIndex--;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید (با احتساب هدرها و فیلترها)
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون قابل مشاهده (برای ساخت RowColumnIndex صحیح)
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex < SYNCFUSION_DG.View.Records.Count - 1)
                {
                    SYNCFUSION_DG.SelectedIndex++;

                    // 1. پیدا کردن ایندکس واقعی سطر در گرید
                    var rowIndex = SYNCFUSION_DG.ResolveToRowIndex(SYNCFUSION_DG.SelectedIndex);

                    // 2. پیدا کردن اولین ستون
                    var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(0);
                    if (columnIndex < 0) columnIndex = 0;

                    // 3. اسکرول کردن به آن نقطه
                    SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(rowIndex, columnIndex));
                }
            }
            catch { }
        }
        private void Btn_Last_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records.Count > 0)
                {
                    var lastIndex = SYNCFUSION_DG.View.Records.Count - 1;
                    SYNCFUSION_DG.SelectedIndex = lastIndex;
                    //SYNCFUSION_DG.ScrollInView(new Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex(lastIndex + 1, 0));

                    SYNCFUSION_DG.GetVisualContainer().ScrollOwner.ScrollToBottom();
                }
            }
            catch { }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e) => ClearAllSfDataFilters();

        private void ClearAllSfDataFilters()
        {
            try
            {
                filterService.ClearFilters();
                ActiveFilters.Clear();
                SYNCFUSION_DG.View.Filter = null;
                SYNCFUSION_DG.View.RefreshFilter();
                SYNCFUSION_DG.ClearFilters();
                PGET_LST_SUB.ClearFilters();
                PAY_GETD_SUB.ClearFilters();
            }
            catch { }
        }

        private void AttachRecordCountUpdater(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;
            void UpdateLabel()
            {
                int count = 0;
                if (dataGrid.ItemsSource is ICollection collection) count = collection.Count;
                else if (dataGrid.View?.Records != null) count = dataGrid.View.Records.Count;
                Dispatcher.Invoke(() => targetTextBlock.Text = count.ToString("N0"));
            }
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                if (e.NewItemsSource is INotifyCollectionChanged nc) nc.CollectionChanged += (sender, args) => UpdateLabel();
                UpdateLabel();
            };
            if (dataGrid.ItemsSource != null)
            {
                if (dataGrid.ItemsSource is INotifyCollectionChanged nc) nc.CollectionChanged += (sender, args) => UpdateLabel();
                UpdateLabel();
            }
        }
            
        #endregion
    }
}
