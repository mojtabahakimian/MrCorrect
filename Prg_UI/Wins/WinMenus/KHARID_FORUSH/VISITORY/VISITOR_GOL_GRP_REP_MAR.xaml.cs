using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.Generaly;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Rpts;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using static Functions.InventoryManager;
using static Prg_UI.Functions.CL_LMethods;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for VISITOR_GOL_GRP_REP_MAR.xaml
    /// </summary>
    public partial class VISITOR_GOL_GRP_REP_MAR : Window
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

        public VISITOR_GOL_GRP_REP_MAR()
        {
            InitializeComponent();

            DataContext = this;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }

        public ObservableCollection<SQ1> VISITOR_GOL_GRP_REP_MAR_DATA { get; set; } = new ObservableCollection<SQ1>();

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
        public string Condition { get; private set; } = "";
        public string Real_Month { get; set; }

        private int FG_BYTE => (Option58.IsChecked == true) ? 1 : 0;

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
            public string? HES { get; set; }
            public byte MAH { get; set; }
            public string? NAMES { get; set; }
            public double? MABL_KS { get; set; }
            public double? MABMARS { get; set; }
            public double? MEGH_MARS { get; set; }
            public double? MEGHkS { get; set; }
            public double? MEGHkGOL { get; set; }
            public double? MANDMEGH { get; set; }
            public double? DARSADFR { get; set; }
            public int? DAYMAND { get; set; }
            public string? VISITOR { get; set; }
            public double? MENUIT { get; set; }
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

            if (CL_HESABDARI.LETSGO("DEPEMAL"))
            {
                if (Condition == "")
                {
                    Condition = " AND (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }
                else
                {
                    Condition = Condition + " AND  (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }

            }

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
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
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
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private string GetSelectedText()
        {
            var dataGrid = VGR_GRID;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedRowsToClipboard();
        }
        private void CopySelectedRowsToClipboard()
        {
            try
            {
                //این توی حالتی که کاربر از فیلتر SfDataGrid Filter استفاده کرده باشه درست کار نمیکنه !
                var _SelectedTextCell_ = GetSelectedText();
                if (!string.IsNullOrEmpty(_SelectedTextCell_))
                {
                    Clipboard.SetText(_SelectedTextCell_);
                    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 1);
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

            //var dataGrid = VGR_GRID;
            //var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;
            //if (currentCell != null && currentCell.IsEditing)
            //{
            //    System.Windows.Forms.SendKeys.SendWait("^(c)"); //Fire Send Keys : Ctrl + C
            //    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 1);
            //    return;
            //}

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

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(VGR_GRID, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
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
        private void VGR_GRID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
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

            //foreach (var column in VGR_GRID.Columns)
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
            string year = Baseknow.YEA.ToString();
            string dt1 = year + _selectedMonth.ToString("D2") + "01";
            string dt2 = year + _selectedMonth.ToString("D2") + "31";
            string hes = HES.SelectedValue?.ToString() ?? "";

            var parameters = new { DT1 = dt1, DT2 = dt2, HES = hes, MAH = _selectedMonth };

            var emruz = CL_HESABDARI.DIFF(Convert.ToInt64(dt1), CL_HESABDARI.FARSIDATE());
            if (emruz - 31 > 0)
                emruz = 0;
            else
                emruz = Math.Abs(emruz - 31);

            string sql;
            if (FG_BYTE == 1)
            {
                sql = $@"
                SELECT
                    d.HES, d.MAH, m.NAMES,
                    SUM(ISNULL(v.MABL_K, 0)) AS MABL_KS,
                    SUM(ISNULL(v.MABMAR, 0)) AS MABMARS,
                    SUM(ISNULL(v.MEGH_MAR, 0)) AS MEGH_MARS,
                    SUM(ISNULL(v.MEGHk, 0)) AS MEGHkS,
                    SUM(d.MEGHk) AS MEGHkGOL,
                    SUM(ISNULL(d.MEGHk, 0) - ISNULL(v.MEGHk, 0)) AS MANDMEGH,
                    CASE WHEN SUM(d.MEGHk) = 0 THEN 0 ELSE SUM(ISNULL(v.MEGHk, 0)) * 100.0 / SUM(d.MEGHk) END AS DARSADFR,
                    0 AS DAYMAND,
                    c.NAME AS VISITOR,
                    m.CODE AS MENUIT
                FROM dbo.STUF_DEF s
                INNER JOIN dbo.visitgol_dtl d ON s.CODE = d.CODE
                INNER JOIN dbo.TCODE_MENUITEM m ON s.MENUIT = m.CODE
                INNER JOIN dbo.CUST_HESAB c ON d.HES = c.hes
                LEFT JOIN dbo.VISITOR_DTL_KALA_MARA({dt1}, {dt2}, N'{hes}') v
                    ON d.CODE = v.CODE AND d.HES = v.CUST_NO
                WHERE d.MAH = {_selectedMonth} AND d.HES = N'{hes}' {Condition}
                GROUP BY d.HES, d.MAH, m.NAMES, c.NAME, m.CODE
                ORDER BY m.NAMES
                ";
            }
            else
            {
                sql = $@"
                SELECT
                    d.HES, d.MAH, g.NAMES,
                    SUM(ISNULL(v.MABL_K, 0)) AS MABL_KS,
                    SUM(ISNULL(v.MABMAR, 0)) AS MABMARS,
                    SUM(ISNULL(v.MEGH_MAR, 0)) AS MEGH_MARS,
                    SUM(ISNULL(v.MEGHk, 0)) AS MEGHkS,
                    SUM(d.MEGHk) AS MEGHkGOL,
                    SUM(ISNULL(d.MEGHk, 0) - ISNULL(v.MEGHk, 0)) AS MANDMEGH,
                    CASE WHEN SUM(d.MEGHk) = 0 THEN 0 ELSE SUM(ISNULL(v.MEGHk, 0)) * 100.0 / SUM(d.MEGHk) END AS DARSADFR,
                    0 AS DAYMAND,
                    c.NAME AS VISITOR,
                    g.CODE AS MENUIT
                FROM dbo.STUF_DEF s
                INNER JOIN dbo.visitgol_dtl d ON s.CODE = d.CODE
                INNER JOIN dbo.TCOD_STUFGROUP g ON s.RADAH = g.CODE
                INNER JOIN dbo.CUST_HESAB c ON d.HES = c.hes
                LEFT JOIN dbo.VISITOR_DTL_KALA_MARA({dt1}, {dt2}, N'{hes}') v
                    ON d.CODE = v.CODE AND d.HES = v.CUST_NO
                WHERE d.MAH = {_selectedMonth} AND d.HES = N'{hes}' {Condition}
                GROUP BY d.HES, d.MAH, g.NAMES, c.NAME, g.CODE
                ORDER BY g.NAMES"
                ;
            }

            _sql_query = sql;
            var data = dbms.DoGetDataSQL<SQ1>(sql).ToList();
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
            var pathreport = Assembly.GetEntryAssembly().GetManifestResourceStream($"Prg_UI.Rpts.Visitory.Visit_gol_dtl_REP_print_grp.mrt");
            report.Load(pathreport);
            string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=900";
            report.Dictionary.Databases.Clear();
            report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));

            report.Dictionary.Variables.Add("Q_PARM", _sql_query);

            (report.GetComponentByName("MAH_N") as StiText).Text = Real_Month;
            (report.GetComponentByName("DATE_N") as StiText).Text = Tarikh.FullCurrentDate.ToString();


            new WINRPT(report, "چاپ عملکرد ویزیتور ها به تفکیک گروه کالا").Show();
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ReGetData();
        }
    }
}
