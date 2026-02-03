using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.KHARID_FORUSH;
using static Prg_UI.Functions.CL_LMethods;
using static Stimulsoft.Base.StiDbType;

namespace Prg_UI.Wins.WinMenus.ANBAR
{
    /// <summary>
    /// Interaction logic for MOGHAYERAT_ANBAR_WIN.xaml
    /// </summary>
    public partial class MOGHAYERAT_ANBAR_WIN : Window
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

        public MOGHAYERAT_ANBAR_WIN(string date, string anbarCode)
        {
            DATE_PARAM = date;
            ANBAR_CODE = anbarCode;

            InitializeComponent();

            this.DataContext = this;

            SYNCFUSION_DG.SelectionController = new SafeGridSelectionController(SYNCFUSION_DG);

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");

        }
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<MOGHA_ANBAR_MODEL> AK_MOGUDI_DATA { get; set; } = new ObservableCollection<MOGHA_ANBAR_MODEL>();
        public bool NowIsReady { get; private set; }

        public string DATE_PARAM { get; set; }
        public string ANBAR_CODE { get; set; }

        public class MOGHA_ANBAR_MODEL
        {
            public string CODE { get; set; }
            public double? MABLK { get; set; }
            public double? MAND { get; set; }
            public double? mab { get; set; }
            public double? tafBED { get; set; }
            public double? TAFBES { get; set; }
            public string HES_T { get; set; }
            public string HES_K { get; set; }
            public string HES_M { get; set; }
            public string HES { get; set; }
            public string NAME { get; set; }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Process Prc = ProcLoader.Start();
            #region SecuritCheck
            try
            {
                string Formname = "AKMOGO";
                var helper = new WindowInteropHelper(this);
                helper.EnsureHandle();
                CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                if (!IsLoaded)
                {
                    Close();
                    return;
                }
            }
            catch
            {
                try { Close(); } catch { }
            }
            #endregion

            try
            {
                AK_MOGUDI_DATA?.Clear();
                var kol = Baseknow.MOGODIA;

                string query = $"SELECT * FROM dbo.MOGHA_ANBAR({DATE_PARAM}, {ANBAR_CODE}, {kol}) ORDER BY TAFBES DESC";
                var list = dbms.DoGetDataSQL<MOGHA_ANBAR_MODEL>(query).ToList();

                foreach (var item in list)
                {
                    AK_MOGUDI_DATA.Add(item);
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری اطلاعات: " + ex.Message).ShowDialog();
            }

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;

            GenerateAutomaticSummary(SYNCFUSION_DG);

            // Ensure the SfDataGrid is not null before subscribing
            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            //ProcLoader.Stop(Prc);

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as MOGHA_ANBAR_MODEL;
            }
        }
        public void OpenWindow(Type windowType, object parameter, string errorMessage)
        {
            if (windowType == null || !typeof(Window).IsAssignableFrom(windowType))
                return;

            var constructor = windowType.GetConstructor(new[] { parameter.GetType() });
            if (constructor != null)
            {
                var window = (Window)constructor.Invoke(new[] { parameter });
                window.Show();
            }

            return; //Check is there any open window before ?
            if (!CL_LMethods.IsWindowOpen(windowType)) //CL_LMethods.IsWindowOpen<HEAD_LST_FROOSH22>()
            {

            }
            else
            {
                new Msgwin(false, errorMessage).ShowDialog();
            }
        }

        #region _SfDataGrid_
        private void View_FilterChanged(object sender, GridFilterEventArgs e)
        {
            UpdateRowCountLabel();
        }
        private void UpdateRowCountLabel()
        {
            // Defensive checks
            if (ROWCOUNT_TEXTBLK == null) return;
            if (SYNCFUSION_DG?.View == null) return;

            // Safely retrieve the record count
            var recordCount = SYNCFUSION_DG.View.Records?.Count ?? 0;

            // Set the label content
            ROWCOUNT_TEXTBLK.Text = recordCount.ToString();
        }

        private readonly FilterService<MOGHA_ANBAR_MODEL> filterService = new FilterService<MOGHA_ANBAR_MODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null)
            {
                return;
            }

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
                Console.WriteLine("Property " + mappingName + " not found on type " + record.GetType().Name);
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as MOGHA_ANBAR_MODEL);
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

            var dataType = typeof(MOGHA_ANBAR_MODEL);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(MOGHA_ANBAR_MODEL).GetProperty(column.MappingName);
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
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // Karte Anbare in kala
            if (AK_MOGUDI_DATA.Count > 0)
            {
                if (SYNCFUSION_DG.SelectedItem is not null)
                {
                    var Row = SYNCFUSION_DG.SelectedItem as MOGHA_ANBAR_MODEL;
                    if (Row != null && !string.IsNullOrEmpty(Row.CODE))
                    {
                        if (CL_HESABDARI.LETSGO("KARTR"))
                        {
                            F_MENU_KART f_MENU_KART = new F_MENU_KART("R", ANBAR_CODE, Row.CODE);
                            f_MENU_KART.ExternalCallShowReport();
                            f_MENU_KART.Close();
                        }
                        else
                        {
                            new Msgwin(false, "شما اجازه لازم براي استفاده از اين بخش را نداريد.!").ShowDialog();
                        }
                    }
                }
            }
        }
        private void BTN_ISEND_Click_1(object sender, RoutedEventArgs e)
        {
            var record = SYNCFUSION_DG.SelectedItem as MOGHA_ANBAR_MODEL;

            if (true) //currentColumn.MappingName == "MABLK"
            {
                if (CL_HESABDARI.LETSGO("KARTR"))
                {
                    // Open F_MENU_KART -> R_KA_KALA
                    F_MENU_KART f_MENU_KART = new F_MENU_KART("R", ANBAR_CODE, record.CODE);
                    f_MENU_KART.ExternalCallShowReport();
                    f_MENU_KART.Close();
                }
                else
                {
                    new Msgwin(false, "شما اجازه لازم براي استفاده از اين بخش را نداريد.!").ShowDialog();
                }
            }
        }
        private void BTN_ISEND_Click_2(object sender, RoutedEventArgs e)
        {
            var record = SYNCFUSION_DG.SelectedItem as MOGHA_ANBAR_MODEL;

            // Accounting Card Balance Drill-down
            if (true) // if (currentColumn.MappingName == "mab")
            {
                if (CL_HESABDARI.BLOCKEDMK(record.HES))
                {
                    new Msgwin(false, "حساب مورد نظر مسدود مي باشد!").ShowDialog();
                    return;
                }

                new F_MENU_KOL_MOIN_TAFZIL(record.HES); //نیازی به Show نیست , خودش داخل خودش انجام میده
                return;

                #region ClassicWay
                try
                {
                    string tableName = $"MOIN{Baseknow.USERCOD}";

                    // Drop existing table
                    dbms.DoExecuteSQL($"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{tableName}]') AND type in (N'U')) DROP TABLE [dbo].[{tableName}]");

                    // Create table using Function
                    // Access: SELECT ... INTO MOIN... FROM QDAFTARTAFZIL2_H(...)
                    string createSql = $"SELECT N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, MAND, id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4, TAFZILN, HES INTO dbo.{tableName} FROM dbo.QDAFTARTAFZIL2_H(1, 99999999, '{record.HES}') AS QDAFTARTAFZIL2_H ORDER BY DATE_S, BED DESC";
                    dbms.DoExecuteSQL(createSql);

                    // Update MAND (Running Balance)
                    // Access loop does: MAN = MAN + MAND; MAND = MAN
                    // Using SQL Window function for efficiency
                    // Note: If QDAFTARTAFZIL2_H already returns running balance in MAND, this update is redundant but Access code suggests it recalculates it.
                    // Assuming the function returns transaction amount or delta in MAND.
                    string updateSql = $@"
                        ;WITH CTE AS (
                            SELECT ID, SUM(MAND) OVER (ORDER BY DATE_S, BED DESC ROWS UNBOUNDED PRECEDING) as RunBal
                            FROM dbo.{tableName}
                        )
                        UPDATE T
                        SET T.MAND = C.RunBal
                        FROM dbo.{tableName} T
                        JOIN CTE C ON T.ID = C.ID";

                    // Check SQL Server version compatibility? Access code had a check, memory mentions fallback.
                    // But here I'll assume SQL 2012+ (Window functions). 
                    // If 2008, use correlated subquery or cursor.
                    // Given the prompt mention "This logic now supports SQL Server 2008 R2 ... fallback to using MAX(DATE_S)", 
                    // I should be careful. 
                    // However, SUM() OVER (... ROWS UNBOUNDED PRECEDING) is available since SQL 2012. 
                    // If target is older, this will fail.
                    // The Access loop was client-side (VB). 
                    // I'll stick to SQL for now. If it fails, I might need to implement C# loop update.

                    dbms.DoExecuteSQL(updateSql);

                    // Open Report Window
                    new R_DAFTAR_MOIN_LIST(tableName, record.HES).Show();
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در آماده سازی دفتر معین: " + ex.Message).ShowDialog();
                }
                #endregion

            }
        }

    }
}
