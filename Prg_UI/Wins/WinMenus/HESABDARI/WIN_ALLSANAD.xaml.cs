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
using Prg_UI.Wins.WinMenus.HESABDARI;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.HESABDARI
{
    public partial class WIN_ALLSANAD : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnm_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnmx_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }
        private void nor_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
                    //(button.FindName("MDPacki_Btn_Max") as PackIcon).Kind = PackIconKind.WindowMaximize;
                    //TitleDrawBar.CornerRadius = new CornerRadius(25, 15, 0, 0);
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
        public WIN_ALLSANAD(string _WhereCondtion_ = "")
        {
            InitializeComponent();

            this.DataContext = this;

            WHERE_CONDITION = _WhereCondtion_;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }
        public ObservableCollection<DEED_HED> HEADSANAD_DATA { get; set; } = new ObservableCollection<DEED_HED>();
        public ObservableCollection<DEED_DTL> SANAD_DATA { get; set; } = new ObservableCollection<DEED_DTL>();

        UniversControl universControl = new UniversControl();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public string WHERE_CONDITION { get; set; } = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Process Prc = ProcLoader.Start();

            //" WHERE SHARH_S LIKE N'%1%' ";

            HEADSANAD_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<DEED_HED>($"SELECT N_S, DATE_S, SHARH_S, NO_S, ANBAR, N_FACTOR, GHATEI, USER_NAME, base, SGN1, SGN2, SGN3, SGN4, OKF, sgn1usid, sgn2usid, sgn3usid, CRT, UID, BAYEG " +
                $" FROM dbo.DEED_HED {WHERE_CONDITION}" + $" ORDER BY N_S").ToList();
            foreach (var item in MasterHead)
            {
                HEADSANAD_DATA.Add(item);
            }

            CL_LMethods.FocusLastSfDataGridRow(SYNCFUSION_DG);

            //ProcLoader.Stop(Prc);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (SYNCFUSION_DG.IsKeyboardFocusWithin)
                {
                    var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
                    if (currentCell != null && !currentCell.IsEditing)
                    {
                        var ROW = SYNCFUSION_DG.SelectedItem as DEED_HED;

                        if (ROW != null && ROW?.N_S != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, this, Convert.ToDouble(ROW.N_S));
                        }
                    }
                }
                else if (false)
                {

                }
            }
        }

        #region _SfDataGrid_
        private readonly FilterService<DEED_HED> filterService = new FilterService<DEED_HED>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            if (e?.CurrentRowColumnIndex == null) return; UpdateCurrentCellValue(e.CurrentRowColumnIndex);
        }
        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e) // Event handler for when the selection changes in the data grid
        {
            var CurrentHeadRow = SYNCFUSION_DG.SelectedItem as DEED_HED;

            if (CurrentHeadRow != null)
            {
                ReGetSubSanad(CurrentHeadRow.N_S);
            }

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
            if (SYNCFUSION_DG.SelectedItems == null || !SYNCFUSION_DG.SelectedItems.Any())
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

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "ExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as DEED_HED);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(GetSelectedText()))
            {
                var element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
                }
            }
        }


        private T FindChildElement<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                var result = FindChildElement<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
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

            var dataType = typeof(DEED_HED);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(DEED_HED).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                if (column.MappingName == "MAND" || column.MappingName == "BED" || column.MappingName == "BES")
                {
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

        private void F8_CUSTOMER_Click(object sender, RoutedEventArgs e)
        {
            if (SANAD_DETAIL.IsEnabled == true)
            {
                var CurrentData = SANAD_DETAIL.SelectedItem as DEED_DTL;

                if (CurrentData != null)
                {
                    if (CurrentData.HES is not null)
                    {
                        new F_MENU_KOL_MOIN_TAFZIL(CurrentData.HES.ToString());
                    }
                }
            }
        }
        private async void EXPORTEXCEL_BTNSUB(object sender, RoutedEventArgs e)
        {
            if (SANAD_DATA.Count == 0)
            {
                return;
            }
            try
            {
                await UniversalExcelExporter.ExportToExcelAsync(SANAD_DETAIL, "DGExportedExcel");
            }
            catch (Exception)
            {
                new Msgwin(false, "خروجی اکسل به دلیل بروز خطا انجام نشد").ShowDialog();
            }
        }
        private void COPY_CLICK(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            DataGridClipboardManager.CopySelectedItems<DEED_DTL>(SANAD_DETAIL);
        }

        private void ReGetSubSanad(double _N_S_)
        {
            SANAD_DATA?.Clear();
            if (_N_S_ != null)
            {

                var Sanaddata = dbms.DoGetDataSQL<DEED_DTL>(@$"SELECT D.N_S, D.RADIF, D.HES_K, D.HES_M, D.HES_T, D.SHARH, D.BED, D.BES, D.N_SERI, D.BANK, D.NUMBER, D.TAG, D.HES, C.NAME AS NAME_HES,
                                                               D.id, D.ARZD, D.MHAZ_NO, D.HES_T2, D.HES_T3, D.HES_T4, D.CRT, D.UID
                                                               FROM dbo.DEED_DTL D
                                                                    LEFT JOIN dbo.CUST_HESAB C ON D.HES=C.hes
                                                               WHERE D.N_S= {_N_S_}").ToList();
                if (Sanaddata.Count > 0)
                {
                    for (int i = 0; i < Sanaddata.Count; i++)
                    {
                        SANAD_DATA.Add(Sanaddata[i]);
                    }
                }
            }

            SUM_BED_TEXTBOX.Text = SANAD_DATA.Sum(i => i.BED).ToStringNullSafe();
            SUM_BES_TEXTBOX.Text = SANAD_DATA.Sum(i => i.BES).ToStringNullSafe();

            var PassedValue = CL_HESABDARI.GetBetweenStr(WHERE_CONDITION, "%", "%");
            if (!string.IsNullOrWhiteSpace(PassedValue))
            {
                FocusRowBySharh(PassedValue);
            }

        }
        private void FocusRowBySharh(string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue) || SANAD_DATA == null || SANAD_DATA.Count == 0)
                return;

            try
            {
                // Find the item with matching SHARH value (case-insensitive partial match)
                var targetItem = SANAD_DATA.FirstOrDefault(item =>
                    item.SHARH != null &&
                    item.SHARH.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) >= 0);

                if (targetItem != null)
                {
                    // Ensure UI is updated
                    SANAD_DETAIL.UpdateLayout();

                    // Set the selected item
                    SANAD_DETAIL.SelectedItem = targetItem;

                    // Scroll the item into view (important for virtualized rows)
                    SANAD_DETAIL.ScrollIntoView(targetItem);

                    // Get the row container
                    var row = SANAD_DETAIL.ItemContainerGenerator.ContainerFromItem(targetItem) as DataGridRow;

                    // If row is null due to virtualization, force generation
                    if (row == null)
                    {
                        SANAD_DETAIL.UpdateLayout();
                        SANAD_DETAIL.ScrollIntoView(targetItem);
                        row = SANAD_DETAIL.ItemContainerGenerator.ContainerFromItem(targetItem) as DataGridRow;
                    }

                    if (row != null)
                    {
                        // Set focus to the row
                        row.Focus();
                        //FocusSpecificCell(row, 2); // Column index 2 is SHARH
                    }
                }
            }
            catch { }
        }

        private void SANAD_DETAIL_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.F7 && Keyboard.Modifiers == ModifierKeys.None)
            {
                DataGridExtension.HandleKeyPress(sender, e, SANAD_DETAIL);
            }
        }

        private void SANAD_DETAIL_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }
            base.OnContextMenuOpening(e);
        }

        private void SANAD_DETAIL_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 0)
            {
                return;
            }

            // Find the row under the mouse
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            DataGridRow row = dep as DataGridRow;
            if (row != null && row.Item != null && row.Item != CollectionView.NewItemPlaceholder)
            {
                // Select the row under the mouse
                dataGrid.SelectedItem = row.Item;

                // Show the context menu
                dataGrid.ContextMenu.IsOpen = true;

                // Mark the event as handled to prevent the default context menu behavior
                e.Handled = true;
            }
            else
            {
                // No valid row, don't show context menu
                e.Handled = true;
            }
        }
    }
}
