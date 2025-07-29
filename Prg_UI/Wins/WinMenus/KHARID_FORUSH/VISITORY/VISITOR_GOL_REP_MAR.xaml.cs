using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_UI.Functions.CL_LMethods;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.BulletGraph;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Syncfusion.Windows.Shared;
using static Functions.InventoryManager;
using Rpts;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for VISITOR_GOL_REP_MAR.xaml
    /// </summary>
    public partial class VISITOR_GOL_REP_MAR : Window
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

        public VISITOR_GOL_REP_MAR()
        {
            InitializeComponent();

            DataContext = this;
        }

        public ObservableCollection<SQ1> VISITOR_GOL_REP_MAR_DATA { get; set; } = new ObservableCollection<SQ1>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();

        public bool ChangeIsHappend { get; private set; } = false;

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                // Get the window handle
                IntPtr handle = new WindowInteropHelper(this).Handle;

                // Only proceed if the handle is valid
                if (handle != IntPtr.Zero)
                {
                    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                }
                else
                {
                    // Defer the operation until the window is fully rendered
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        // Try again after the window is fully initialized
                        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                        if (newHandle != IntPtr.Zero)
                        {
                            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private bool ican;
        public bool AllowEdits
        {
            get { return ican; }
            set
            {
                ican = value;

                //DETAIL_VOSUL_SUB.IsReadOnly = !ican;
            }
        }

        public bool NowIsReady { get; private set; }
        public string _sql_query { get; set; }
        public string Real_Month { get; set; }

        private int _selectedMonth = 1;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                ReGetData(); // هر بار که ماه عوض شد، دیتا آپدیت کن
            }
        }

        public class SQ1
        {
            public string? CODE { get; set; }
            public string? CUST_NO { get; set; }
            public string? HES { get; set; }
            public byte MAH { get; set; }
            public string? kala { get; set; }
            public double? MABL_K { get; set; }
            public double? MABMAR { get; set; }
            public double? MEGH_MAR { get; set; }
            public double? MEGHk { get; set; }
            public string? VISITOR { get; set; }
            public double? MEGHkGOL { get; set; }
            public double? MANDMEGH { get; set; }
            public double? DARSADFR { get; set; }
            public int? DAYMAND { get; set; }
        }

        public class Q1
        {
            public string? HES { get; set; }
        }

        public class Q3
        {
            public string? HES { get; set; }
            public string? NAME { get; set; }
        }

        private void Form_Open()
        {
            try
            {

                string year = Baseknow.YEA.ToString(); // مثلاً متد جدا درست کن یا مستقیماً از کنترل خاصی مقدار بخوان
                var rst = dbms.DoGetDataSQL<Q1>("SELECT HES FROM visitgol_head").FirstOrDefault();

                if (rst != null)
                {
                    HES.SelectedValue = rst.HES;
                    HES2.SelectedValue = rst.HES;

                    int dt1 = int.Parse(year + "0101");
                    int dt2 = int.Parse(year + "0131");

                    // محاسبه EMRUZ:
                    var emruz = CL_HESABDARI.DIFF(dt1, Convert.ToInt64(CL_HESABDARI.FARSIDATE)); // این دو تابع رو خودت بنویس یا معادلش رو بیار
                    if (emruz - 31 > 0)
                        emruz = 0;
                    else
                        emruz = Math.Abs(emruz - 31);

                    // ماه پیش‌فرض: مثلاً 1 (فروردین) یا ماه جاری (اختیاری)
                    SelectedMonth = 1; // یا مثلاً SelectedMonth = DateTime.Now.Month;

                    // دیتا را برای ماه و HES اولیه لود کن
                    ReGetData();
                }
                else
                {
                    // هندل اگر دیتا وجود نداشت (اختیاری)
                }
            }
            catch (Exception ex)
            {
                // هندل خطا (مثلاً پیام یا لاگ)
            }
        }

        private void FILL_ALL_COMBOBOXES()
        {
            HES.ItemsSource = dbms.DoGetDataSQL<Q3>("SELECT visitgol_head.HES, CUST_HESAB.NAME FROM visitgol_head INNER JOIN CUST_HESAB ON visitgol_head.HES = CUST_HESAB.hes GROUP BY visitgol_head.HES, CUST_HESAB.NAME").ToList();
            HES.DisplayMemberPath = "NAME";
            HES.SelectedValuePath = "HES";

            HES2.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT HES FROM visitgol_head GROUP BY HES").ToList();
            HES2.DisplayMemberPath = "HES";
            HES2.SelectedValuePath = "HES";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            FILL_ALL_COMBOBOXES();

            Form_Open();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;

                CL_LMethods.SendKey_US(Key.Tab);
            }
        }

        #region _SfDataGrid_
        private readonly FilterService<SQ1> filterService = new FilterService<SQ1>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        private void VGR_GRID_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void VGR_GRID_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            //// Get the selected row and column index
            var currentCell = VGR_GRID.SelectionController.CurrentCellManager.CurrentCell;
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
            int columnIndex = this.VGR_GRID.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            var mappingName = this.VGR_GRID.Columns[columnIndex].MappingName;
            var recordIndex = this.VGR_GRID.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.VGR_GRID.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName)?.GetValue(record)?.ToString();
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
                    filterService.AddFilter(columnName, filterValue, isExclusion: true);
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
            if (VGR_GRID.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = VGR_GRID.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
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
            VGR_GRID.View.Filter = item => filterService.ApplyFilter(item as SQ1);
            // Refresh the filter to update the view
            VGR_GRID.View.RefreshFilter();
        }
        private void VGR_GRID_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }

        }
        private string GetSelectedText()
        {
            var dataGrid = VGR_GRID;
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
            if (VGR_GRID.SelectedItems == null || !VGR_GRID.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            var sb = new StringBuilder();

            try
            {
                // Add headers
                foreach (var column in VGR_GRID.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in VGR_GRID.SelectedItems)
                {
                    foreach (var column in VGR_GRID.Columns)
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
                universControl.PopNotifyShow($"{VGR_GRID.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private void VGR_GRID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(VGR_GRID);

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
                VGR_GRID.TableSummaryRows.Clear();
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

            var dataType = typeof(SQ1);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(SQ1).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType))
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
                await UniversalExcelExporter.ExportToExcelAsync(VGR_GRID, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        #endregion

        private void HES_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            if (HES.SelectedItem == null) return;

            // HES2 را با HES جدید مقداردهی کن
            HES2.SelectedValue = HES.SelectedValue;

            // ماه انتخابی را از پراپرتی مربوطه بخوان
            int selectedMonth = SelectedMonth; // پراپرتی یا مقدار عدد ماه جاری
            string year = Baseknow.YEA.ToString(); // سال جاری (از متد یا TextBox مناسب)
            string dt1 = year + selectedMonth.ToString("D2") + "01";
            string dt2 = year + selectedMonth.ToString("D2") + "31";

            // محاسبه EMRUZ
            var emruz = CL_HESABDARI.DIFF(Convert.ToInt64(dt1), Convert.ToInt64(CL_HESABDARI.FARSIDATE()));
            if (emruz - 31 > 0)
                emruz = 0;
            else
                emruz = Math.Abs(emruz - 31);

            // دیتا را با مقدار جدید HES لود کن
            ReGetData();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int month))
            {
                SelectedMonth = month;
            }
        }

        private void ReGetData()
        {
            if (HES.SelectedValue is null)
            {
                return;
            }
            string year = Baseknow.YEA.ToString(); // باید مقدار سال جاری رو از جای درست بگیری (مثلاً TextBox یا منبع دیتا)
            string dt1 = year + _selectedMonth.ToString("D2") + "01";
            string dt2 = year + _selectedMonth.ToString("D2") + "31";
            string hes = HES.SelectedValue.ToString(); // یا مقدار جاری ویزیتور

            // همان کوئری که در VBA داشتی، اما با جایگزینی پارامترها:
            string sql = $@"
            SELECT TOP 100 PERCENT 
                dbo.visitgol_dtl.CODE, VISITOR_DTL_KALA_marA.CUST_NO, dbo.visitgol_dtl.HES, dbo.visitgol_dtl.MAH, dbo.STUF_DEF.NAME AS kala,
                ISNULL(VISITOR_DTL_KALA_marA.MABL_K, 0) AS MABL_K, ISNULL(VISITOR_DTL_KALA_marA.MABMAR, 0) AS MABMAR, ISNULL(VISITOR_DTL_KALA_marA.MEGH_MAR,0) AS MEGH_MAR,
                ISNULL(VISITOR_DTL_KALA_marA.MEGHk, 0) AS MEGHk, VISITOR_DTL_KALA_marA.VISITOR, dbo.visitgol_dtl.MEGHk AS MEGHkGOL,
                ISNULL(dbo.visitgol_dtl.MEGHk - VISITOR_DTL_KALA_marA.MEGHk, 0) AS MANDMEGH,
                ISNULL(VISITOR_DTL_KALA_marA.MEGHk / dbo.UIIF(dbo.visitgol_dtl.MEGHk, N'=', 0, 1, dbo.visitgol_dtl.MEGHk) * 100, 0) AS DARSADFR,
                0 AS DAYMAND
            FROM dbo.STUF_DEF 
            INNER JOIN dbo.visitgol_dtl ON dbo.STUF_DEF.CODE = dbo.visitgol_dtl.CODE 
            LEFT OUTER JOIN dbo.VISITOR_DTL_KALA_marA({dt1}, {dt2}, N'{hes}') VISITOR_DTL_KALA_marA
                ON dbo.visitgol_dtl.CODE = VISITOR_DTL_KALA_marA.CODE AND dbo.visitgol_dtl.HES = VISITOR_DTL_KALA_marA.CUST_NO
            WHERE (dbo.visitgol_dtl.MAH = {_selectedMonth}) AND (dbo.visitgol_dtl.HES = N'{hes}')
            ORDER BY dbo.STUF_DEF.NAME
            ";


            _sql_query = sql;

            //var parameters = new { DT1 = dt1, DT2 = dt2, HES = hes, MAH = _selectedMonth };
            var data = dbms.DoGetDataSQL<SQ1>(sql).ToList();
            // حالا دیتا را به گرید یا لیست متصل کن
            VGR_GRID.ItemsSource = data;

        }

        private void HES2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HES2.IsEditable) { if (!(e.OriginalSource is TextBox)) return; } //اگر چیزی جز خود محتوای متن کمبوباکس صداش زده ندادیه بگیر

            HES.SelectedValue = HES2.SelectedValue;
        }

        private void Command55_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(_sql_query))
            //{
                return;
            //}

            switch (SelectedMonth)
            {
                case 1:
                    Real_Month = "فروردین";
                    break;

                case 2:
                    Real_Month = "اردیبهشت";
                    break;

                case 3:
                    Real_Month = "خرداد";
                    break;

                case 4:
                    Real_Month = "تیر";
                    break;

                case 5:
                    Real_Month = "مرداد";
                    break;

                case 6:
                    Real_Month = "شهریور";
                    break;

                case 7:
                    Real_Month = "مهر";
                    break;

                case 8:
                    Real_Month = "آبان";
                    break;

                case 9:
                    Real_Month = "آذر";
                    break;

                case 10:
                    Real_Month = "دی";
                    break;

                case 11:
                    Real_Month = "بهمن";
                    break;

                case 12:
                    Real_Month = "اسفند";
                    break;

                default:
                    break;
            }

            OpenReport();
        }

        private void OpenReport()
        {

            var report = new StiReport();
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Visitory.Visit_gol_dtl_Rep.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            (report.GetComponentByName("MAH_N") as StiText).Text = Real_Month;


            new WINRPT(report, "چاپ عملکرد ویزیتور ها به تفکیک کالا").Show();
        }
    }
}
