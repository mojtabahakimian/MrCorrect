using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.BulletGraph;
using System.Windows.Controls;
using Prg_UI.UiTools;
using Prg_Proccessy.MODELS;
using System.Text;
using Syncfusion.Data;
using Prg_UI.HelperWins;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using System.Diagnostics;
using System.Windows.Media;
using Prg_Proccessy.FUNCTIONS;
using System.Reflection;
using System.Globalization;
using System.Threading;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    public partial class R_DAFTAR_MOIN_LIST : Window
    {
        public R_DAFTAR_MOIN_LIST(object acFormDS = null, string _fullhesabname = null)
        {
            OPEN_ARG = acFormDS;
            FULLHESAB_NAME = _fullhesabname;
            InitializeComponent();

            this.DataContext = this;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }
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
        public ObservableCollection<MOIN_CUSTOM> DAFTAR_DATA { get; set; } = new ObservableCollection<MOIN_CUSTOM>();
        UniversControl universControl = new UniversControl();
        public object OPEN_ARG { get; set; }
        public string FULLHESAB_NAME { get; set; }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();
            Label_hesab.Content = $"نتیجه لیست دفتر تفضیلی {FULLHESAB_NAME}";

            DAFTAR_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM {OPEN_ARG}").ToList();
            foreach (var item in MasterHead)
            {
                DAFTAR_DATA.Add(item);
            }

            #region BEFORE
            //mOIN132DataGrid.ItemsSource = dbms.DoGetDataSQL<MOIN_CUSTOM>($"SELECT * FROM {OPEN_ARG}").ToList();
            //if (mOIN132DataGrid.Items.Count > 0)
            //{
            //    CL_LMethods.MovingDG(mOIN132DataGrid, NavigationDirection.LastItem);
            //}
            #endregion

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;

            //var lastRowIndex = SYNCFUSION_DG.GetLastRowIndex();
            //if (lastRowIndex >= 0)
            //{
            //    SYNCFUSION_DG.ScrollInView(new RowColumnIndex(lastRowIndex, 0));
            //    SYNCFUSION_DG.SelectedIndex = lastRowIndex;
            //    SYNCFUSION_DG.Focus();
            //}

            try
            {
                dbms.DoExecuteSQL("INSERT INTO AMALIAT (USERID,USERNAME,ADATE,AMALID) VALUES (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.TruncateString(Baseknow.UUSER, 49) + "',GETDATE(),'" + CL_HESABDARI.TruncateString(OPEN_ARG.ToStringNullSafe(), 49) + "')");
                dbms.DoExecuteSQL("INSERT INTO AMALIAT (USERID,USERNAME,ADATE,AMALID) VALUES (" + Baseknow.USERCOD + ",'" + CL_HESABDARI.TruncateString(FULLHESAB_NAME, 49) + "',GETDATE(),'" + CL_HESABDARI.TruncateString(OPEN_ARG.ToStringNullSafe(), 49) + "')");
            }
            catch { }


            GenerateAutomaticSummary(SYNCFUSION_DG);

            CL_LMethods.FocusLastSfDataGridRow(SYNCFUSION_DG);

            ProcLoader.Stop(Prc);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
                if (currentCell != null && !currentCell.IsEditing)
                {
                    var ROW = SYNCFUSION_DG.SelectedItem as MOIN_CUSTOM;

                    if (ROW != null && ROW?.NO_S != null)
                    {
                        CL_MenuManager.MenuBaseOnKindOpen(this, dbms, (ROW?.TAG is null ? (int)ROW.NO_S : Convert.ToInt32(ROW?.TAG)), (ROW?.NUMBER is null ? ROW?.N_S : ROW?.NUMBER), false);
                    }
                }
            }
        }

        #region _SfDataGrid_
        private readonly FilterService<MOIN_CUSTOM> filterService = new FilterService<MOIN_CUSTOM>();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as MOIN_CUSTOM);
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
        private string GetSelectedText()
        {
            var dataGrid = SYNCFUSION_DG;
            var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;

            if (currentCell != null && currentCell.IsEditing)
            {
                // Find the editing element (which will be a TextBox in edit mode)
                var editingElement = dataGrid.FindElementOfType<TextBox>();
                if (editingElement != null)
                {
                    if (!string.IsNullOrEmpty(editingElement.SelectedText))
                    {
                        return editingElement.SelectedText; // Return the selected text
                    }
                }

                var editingElement2 = FindChildElement<TextBox>(dataGrid);
                if (editingElement2 != null)
                {
                    return editingElement2.SelectedText;
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
                    universControl.PopNotifyShowUp("متن مورد نظر کپی شد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
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

            //if (e.Key == Key.D1 && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    HandleNavigationToBalance();
            //    e.Handled = true;
            //    return;
            //}

            try
            {
                // NEW: Handle 'Ctrl+1' key for CURRENT ROW balance navigation
                if (e.Key == Key.D1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    HandleNavigationToBalanceFromCurrentRow();
                    e.Handled = true;
                    return;
                }

                // VBA: KeyAscii = 49 (1 key)
                // Handle '1' key for FINAL balance navigation (last row)
                if (e.Key == Key.D1 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    HandleNavigationToBalanceFromLastRow();
                    e.Handled = true;
                    return;
                }

                // Also handle NumPad1 for both cases
                if (e.Key == Key.NumPad1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    HandleNavigationToBalanceFromCurrentRow();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.NumPad1 && Keyboard.Modifiers == ModifierKeys.None)
                {
                    HandleNavigationToBalanceFromLastRow();
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
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

            var dataType = typeof(MOIN_CUSTOM);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(MOIN_CUSTOM).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                if (column.MappingName == "BED" || column.MappingName == "BES")
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

        #endregion

        #region Balance Navigation Methods
        /// <summary>
        /// Handle navigation to balance when '1' key is pressed
        /// Starts from LAST ROW and navigates backward
        /// VBA: KeyAscii = 49 (1 key) logic - Original functionality
        /// </summary>
        private void HandleNavigationToBalanceFromLastRow()
        {
            try
            {
                // Check if grid has data
                if (SYNCFUSION_DG.View.Records.Count == 0)
                {
                    universControl.PopNotifyShow("هیچ رکوردی برای جستجو وجود ندارد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }

                // Go to last record
                int lastIndex = SYNCFUSION_DG.View.Records.Count - 1;
                SYNCFUSION_DG.SelectedIndex = lastIndex;
                SYNCFUSION_DG.ScrollInView(new RowColumnIndex(lastIndex, 0));

                var lastRow = SYNCFUSION_DG.SelectedItem as MOIN_CUSTOM;
                if (lastRow == null)
                {
                    return;
                }

                // Get MAND value from last row
                decimal currentBalance = 0;
                if (lastRow.MAND != null)
                {
                    decimal.TryParse(lastRow.MAND.ToString(), out currentBalance);
                }

                // Check if balance is zero
                if (currentBalance == 0)
                {
                    universControl.PopNotifyShow("مانده نهایی صفر است", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                    return;
                }

                // Determine balance type and navigate
                string balanceType = DetermineBalanceType(currentBalance);

                if (balanceType == "بد") // Debit
                {
                    NavigateBackwardForDebit(currentBalance, lastIndex);
                }
                else // Credit
                {
                    currentBalance = Math.Abs(currentBalance);
                    NavigateBackwardForCredit(currentBalance, lastIndex);
                }
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShow("خطا در جستجوی ریشه مانده نهایی", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
            }
        }
        /// <summary>
        /// NEW: Handle navigation to balance from CURRENT SELECTED ROW
        /// Starts from current row and navigates backward
        /// Triggered by: Ctrl+1
        /// </summary>
        private void HandleNavigationToBalanceFromCurrentRow()
        {
            try
            {
                // Check if grid has data
                if (SYNCFUSION_DG.View.Records.Count == 0)
                {
                    universControl.PopNotifyShow("هیچ رکوردی برای جستجو وجود ندارد", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }

                // Get current selected row
                var currentRow = SYNCFUSION_DG.SelectedItem as MOIN_CUSTOM;
                if (currentRow == null)
                {
                    universControl.PopNotifyShow("لطفاً یک سطر را انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                    return;
                }

                // Get current row index
                int currentIndex = SYNCFUSION_DG.SelectedIndex;
                if (currentIndex < 0)
                {
                    return;
                }

                // Get MAND value from current row
                decimal currentBalance = 0;
                if (currentRow.MAND != null)
                {
                    decimal.TryParse(currentRow.MAND.ToString(), out currentBalance);
                }

                // Check if balance is zero
                if (currentBalance == 0)
                {
                    universControl.PopNotifyShow("مانده سطر انتخابی صفر است", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                    return;
                }

                // Determine balance type and navigate
                string balanceType = DetermineBalanceType(currentBalance);

                if (balanceType == "بد") // Debit
                {
                    NavigateBackwardForDebit(currentBalance, currentIndex);
                }
                else // Credit
                {
                    currentBalance = Math.Abs(currentBalance);
                    NavigateBackwardForCredit(currentBalance, currentIndex);
                }
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShow("خطا در جستجوی ریشه مانده سطر انتخابی", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
            }
        }
        /// <summary>
        /// Determine if balance is debit (بد) or credit (بس)
        /// </summary>
        private string DetermineBalanceType(decimal balance)
        {
            // Positive balance typically means debit, negative means credit
            // Adjust based on your accounting logic
            if (balance > 0)
            {
                return "بد"; // Debit
            }
            else
            {
                return "بس"; // Credit
            }
        }
        /// <summary>
        /// Navigate backward through records for debit balance
        /// VBA: While MN > 0 And MN - Me.BED >= 0
        /// </summary>
        /// <param name="remainingBalance">Starting balance amount</param>
        /// <param name="startIndex">Index to start navigation from</param>
        private void NavigateBackwardForDebit(decimal remainingBalance, int startIndex)
        {
            try
            {
                int currentIndex = startIndex;
                bool foundOrigin = false;

                while (remainingBalance > 0 && currentIndex > 0)
                {
                    // Move to previous row
                    currentIndex--;
                    SYNCFUSION_DG.SelectedIndex = currentIndex;
                    SYNCFUSION_DG.ScrollInView(new RowColumnIndex(currentIndex, 0));

                    var currentRow = SYNCFUSION_DG.SelectedItem as MOIN_CUSTOM;
                    if (currentRow == null)
                    {
                        break;
                    }

                    // Get BED (debit) value
                    decimal bedValue = 0;
                    if (currentRow.BED != null)
                    {
                        decimal.TryParse(currentRow.BED.ToString(), out bedValue);
                    }

                    // VBA: If MN - Me.BED > 0 Then
                    if (remainingBalance - bedValue > 0)
                    {
                        // VBA: MN = MN - Me.BED
                        remainingBalance = remainingBalance - bedValue;
                    }
                    else
                    {
                        // VBA: Exit Sub - Found the origin
                        foundOrigin = true;
                        break;
                    }
                }

                // Ensure UI updates
                SYNCFUSION_DG.Focus();

                // Show notification
                if (foundOrigin)
                {
                    //universControl.PopNotifyShowUp("ریشه مانده پیدا شد و سطر انتخاب گردید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                }
                else
                {
                    universControl.PopNotifyShow("به اولین سطر رسیدیم", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// Navigate backward through records for credit balance
        /// VBA: While MN > 0 And MN - Me.BES >= 0
        /// </summary>
        /// <param name="remainingBalance">Starting balance amount (absolute value)</param>
        /// <param name="startIndex">Index to start navigation from</param>
        private void NavigateBackwardForCredit(decimal remainingBalance, int startIndex)
        {
            try
            {
                int currentIndex = startIndex;
                bool foundOrigin = false;

                while (remainingBalance > 0 && currentIndex > 0)
                {
                    // Move to previous row
                    currentIndex--;
                    SYNCFUSION_DG.SelectedIndex = currentIndex;
                    SYNCFUSION_DG.ScrollInView(new RowColumnIndex(currentIndex, 0));

                    var currentRow = SYNCFUSION_DG.SelectedItem as MOIN_CUSTOM;
                    if (currentRow == null)
                    {
                        break;
                    }

                    // Get BES (credit) value
                    decimal besValue = 0;
                    if (currentRow.BES != null)
                    {
                        decimal.TryParse(currentRow.BES.ToString(), out besValue);
                    }

                    // VBA: If MN - Me.BES > 0 Then
                    if (remainingBalance - besValue > 0)
                    {
                        // VBA: MN = MN - Me.BES
                        remainingBalance = remainingBalance - besValue;
                    }
                    else
                    {
                        // VBA: Exit Sub - Found the origin
                        foundOrigin = true;
                        break;
                    }
                }

                // Ensure UI updates
                SYNCFUSION_DG.Focus();

                // Show notification
                if (foundOrigin)
                {
                    //universControl.PopNotifyShowUp("ریشه مانده پیدا شد و سطر انتخاب گردید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
                }
                else
                {
                    universControl.PopNotifyShow("به اولین سطر رسیدیم", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            HandleNavigationToBalanceFromLastRow();
        }
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            HandleNavigationToBalanceFromCurrentRow();
        }
    }
}
