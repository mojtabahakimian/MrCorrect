using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
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
using System.Windows.Media;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.Checkha
{
    public partial class CHEK_VOSUL_LES : Window
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
        public CHEK_VOSUL_LES(string _dt1_, string _dt2_)
        {
            InitializeComponent();

            this.DataContext = this;

            DT1_PASSED = _dt1_;
            DT2_PASSED = _dt2_;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }
        public ObservableCollection<CHKE_VLIST> SFDATAGRID_DATA { get; set; } = new ObservableCollection<CHKE_VLIST>();
        public ObservableCollection<COMBOYMODEL> VAZ_DATA { get; set; } = new ObservableCollection<COMBOYMODEL>
        {
            new COMBOYMODEL { ID = 1, NAME = "نزد صندوق" },
            new COMBOYMODEL { ID = 2, NAME = "نزد بانك" },
            new COMBOYMODEL { ID = 3, NAME = "وصول شده" },
            new COMBOYMODEL { ID = 4, NAME = "واگذار شده" },
            new COMBOYMODEL { ID = 5, NAME = "برگشت شده" },
            new COMBOYMODEL { ID = 6, NAME = "مسترد شده" },
            new COMBOYMODEL { ID = 7, NAME = "حذف شده" }
        };

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

        public string DT1_PASSED { get; set; }
        public string DT2_PASSED { get; set; }
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
            else
            {
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}

            I_AM_CHEK_VOSUL_LES = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            FILL_ALL_COMBOBOX();

            ReGetData();

            GenerateAutomaticSummary(SFDATAGRID_SUB);

            CL_LMethods.FocusLastSfDataGridRow(SFDATAGRID_SUB);
        }

        private void FILL_ALL_COMBOBOX()
        {
            //بانکها

            //MappingName="BANK" SelectedValuePath="CODE" DisplayMemberPath="NAMES"
            BANK_COLUMN.ItemsSource = dbms.DoGetDataSQL<TCOD_BANKS>($"SELECT * FROM dbo.TCOD_BANKS").ToList();

            //VAZ_COLUMN.ItemsSource = dbms.DoGetDataSQL<COMBOYMODEL>($"SELECT * FROM dbo.COMBOYMODEL").ToList();

            //وضعیت چک
            // MappingName="VAZ" SelectedValuePath="ID" DisplayMemberPath="NAME" 
            //var RST_VAZ = new List<COMBOYMODEL>
            //{
            //    new COMBOYMODEL { ID = 1, NAME = "نزد صندوق" },
            //    new COMBOYMODEL { ID = 2, NAME = "نزد بانك" },
            //    new COMBOYMODEL { ID = 3, NAME = "وصول شده" },
            //    new COMBOYMODEL { ID = 4, NAME = "واگذار شده" },
            //    new COMBOYMODEL { ID = 5, NAME = "برگشت شده" },
            //    new COMBOYMODEL { ID = 6, NAME = "مسترد شده" },
            //    new COMBOYMODEL { ID = 7, NAME = "حذف شده" }
            //};

            //VAZ_COLUMN.ItemsSource = RST_VAZ;

            //MappingName="VAZ" SelectedValuePath="TNUMBER" DisplayMemberPath="NAME" 
            //VAZ_COLUMN.ItemsSource = dbms.DoGetDataSQL<TDETA_HES>($"SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = {CL_HESABDARI.GETKOL(Baseknow.ADA)}) AND (NUMBER = {CL_HESABDARI.GETMOIN(Baseknow.ADA)})").ToList();

            ////موقعیت چک
            //var SANDUGH_RST = dbms.DoGetDataSQL<TDETA_HES>($"SELECT * FROM TDETA_HES WHERE(N_KOL = {CL_HESABDARI.GETKOL(Baseknow.ADA)}) AND(NUMBER = 1)").ToList();
            //SANDUGH_COLUMN.ItemsSource = SANDUGH_RST;
        }

        private void ReGetData()
        {

            SFDATAGRID_DATA?.Clear();
            var RST = dbms.DoGetDataSQL<CHKE_VLIST>($"SELECT * FROM dbo.CHKE_VLIST({Baseknow.BANKHA}) WHERE (DATE_S >= " + DT1_PASSED + " AND DATE_S <= " + DT2_PASSED + " ) ORDER BY DATE_S").ToList();
            foreach (var item in RST)
            {
                SFDATAGRID_DATA.Add(item);
            }

            ROWCOUNT_LABEL.Content = SFDATAGRID_DATA.Count;
        }

        #region _SfDataGrid_
        private readonly FilterService<CHKE_VLIST> filterService = new FilterService<CHKE_VLIST>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private int CurrentColumnIndex;
        private void SFDATAGRID_SUB_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e)
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void SFDATAGRID_SUB_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            //// Get the selected row and column index
            var currentCell = SFDATAGRID_SUB.SelectionController.CurrentCellManager.CurrentCell;
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
            int columnIndex = this.SFDATAGRID_SUB.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;

            CurrentColumnIndex = columnIndex;

            var mappingName = this.SFDATAGRID_SUB.Columns[columnIndex].MappingName;
            var recordIndex = this.SFDATAGRID_SUB.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SFDATAGRID_SUB.View.Records.GetItemAt(recordIndex);
            CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
        }
        private string GetSelectedText()
        {
            var dataGrid = SFDATAGRID_SUB;
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
            if (SFDATAGRID_SUB.SelectedItems == null || !SFDATAGRID_SUB.SelectedItems.Any())
            {
                universControl.PopNotifyShow("چیزی برای کپی انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            //var dataGrid = SYNCFUSION_DG;
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
                foreach (var column in SFDATAGRID_SUB.Columns)
                {
                    if (!column.IsHidden) // Include only columns that are not hidden
                        sb.Append(column.HeaderText + "\t");
                }
                sb.AppendLine();

                // Add selected rows
                foreach (var item in SFDATAGRID_SUB.SelectedItems)
                {
                    foreach (var column in SFDATAGRID_SUB.Columns)
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
                universControl.PopNotifyShow($"{SFDATAGRID_SUB.SelectedItems.Count} تعداد رکورد در حافظه کپی شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
            catch { }

        }
        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SFDATAGRID_SUB, "ExportedExcel");
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
            if (SFDATAGRID_SUB.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                var columnName = SFDATAGRID_SUB.SelectionController.CurrentCellManager.CurrentCell.GridColumn.MappingName; // Get the name of the column
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
            SFDATAGRID_SUB.View.Filter = item => filterService.ApplyFilter(item as CHKE_VLIST);
            // Refresh the filter to update the view
            SFDATAGRID_SUB.View.RefreshFilter();
        }

        private void SFDATAGRID_SUB_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }
        private void SFDATAGRID_SUB_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
            }
        }
        private void SFDATAGRID_SUB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.L) || (Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.L))
            {
                CalculateSumForCurrentColumn(SFDATAGRID_SUB);
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
                SFDATAGRID_SUB.TableSummaryRows.Clear();
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

            var dataType = typeof(CHKE_VLIST);

            //foreach (var column in SFDATAGRID_SUB.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                if (column.MappingName == "MABL")
                {
                    var propertyInfo = typeof(CHKE_VLIST).GetProperty(column.MappingName);
                    if (propertyInfo == null)
                        continue;

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

        private void SFDATAGRID_SUB_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {

        }
        private void SFDATAGRID_SUB_CurrentCellValidating(object sender, CurrentCellValidatingEventArgs e)
        {

        }

        public Visual I_AM_CHEK_VOSUL_LES { get; private set; }
        public string? OpenArgs { get; }

    }
}
