using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Functions;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.BulletGraph;
using System.Windows.Controls;
using Prg_UI.UiTools;
using System.Text;
using Syncfusion.Data;
using Prg_UI.HelperWins;
using System.Windows.Media;
using Prg_Proccessy.SQLMODELS;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Threading;
using Prg_Proccessy.FUNCTIONS;
using System.Windows.Interop;
using System.Threading.Tasks;

namespace Prg_UI.Wins.WinMenus.Taarif
{
    public partial class PRICE_GRP_FORM : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon? packIcon = Btn_Max.Content as PackIcon;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowMaximize;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    if (packIcon != null)
                        packIcon.Kind = PackIconKind.WindowRestore;
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
        public PRICE_GRP_FORM()
        {
            InitializeComponent();

            this.DataContext = this;

            //اگر این خط فعال بشه جلوی فوکوس خودکار روی سطر جدید رو میگیره که البته راه حلش کامنت شده
            //SYNCFUSION_DG.SelectionController = new SafeGridSelectionController(SYNCFUSION_DG); //این فقط برای SfDataGrid های فقط خواندنی خوبه

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }
        public ObservableCollection<PRICE_GRP_MODEL> SFDG_DATA { get; set; } = new ObservableCollection<PRICE_GRP_MODEL>();

        UniversControl universControl = new UniversControl();
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        public PRICE_GRP_MODEL? CURRENT_ROW_ITEMS { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }

        private PRICE_GRP_MODEL? wasRowSnapshot; // previous values for revert-on-empty in combo

        public string OpenArgs { get; set; } = "";

        private int _defaultColIndex = -1;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (_defaultColIndex == -1 && SYNCFUSION_DG.Columns.Count > 0)
                {
                    var col = SYNCFUSION_DG.Columns.FirstOrDefault(c => c.MappingName == "PGNAME");
                    _defaultColIndex = col != null ? SYNCFUSION_DG.Columns.IndexOf(col) : 0;
                }
                return _defaultColIndex < 0 ? 0 : _defaultColIndex;
            }
        }

        private bool _bl;
        public bool AllowDeletions
        {
            get { return _bl; }
            set
            {

                _bl = value;

                SYNCFUSION_DG.AllowDeleting = _bl;

                //// Get the window handle
                //IntPtr handle = new WindowInteropHelper(this).Handle;

                //// Only proceed if the handle is valid
                //if (handle != IntPtr.Zero)
                //{
                //    CL_LMethods.AllowDeletions(this.GetType().Name, _bl, handle);
                //}
                //else
                //{
                //    // Defer the operation until the window is fully rendered
                //    this.Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        // Try again after the window is fully initialized
                //        IntPtr newHandle = new WindowInteropHelper(this).Handle;
                //        if (newHandle != IntPtr.Zero)
                //        {
                //            CL_LMethods.AllowDeletions(this.GetType().Name, _bl, newHandle);
                //        }
                //    }), System.Windows.Threading.DispatcherPriority.Loaded);
                //}
            }
        }

        private bool can;
        public bool AllowEdits
        {
            get { return can; }
            set
            {
                can = value;

                SYNCFUSION_DG.AllowEditing = can;
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (false)
                    {
                        if (SYNCFUSION_DG.IsKeyboardFocusWithin)
                        {
                            if (SYNCFUSION_DG.CurrentColumn != null)
                            {
                                if (SYNCFUSION_DG.IsKeyboardFocusWithin)
                                {
                                    var currentCell = SYNCFUSION_DG.SelectionController?.CurrentCellManager?.CurrentCell;
                                    //if (currentCell != null)
                                    //{
                                    //    // Check if we're in the last editable column
                                    //    bool isInLastColumn = IsCurrentCellInLastColumn();
                                    //    if (isInLastColumn)
                                    //    {
                                    //    }
                                    //    // Prevent default Tab behavior
                                    //    e.Handled = true;
                                    //    // Check if current cell is being edited
                                    //    if (currentCell.IsEditing)
                                    //    {
                                    //        // End edit first, which will trigger validation
                                    //        SYNCFUSION_DG.SelectionController.CurrentCellManager.EndEdit();
                                    //    }
                                    //    // Move to new row (with delay to allow validation)
                                    //    MoveToNextRowOrCreateNew();
                                    //    return;
                                    //}

                                    //if (currentCell != null && !currentCell.IsEditing)
                                    //{
                                    //    var ROW = SYNCFUSION_DG.SelectedItem as PRICE_GRP_MODEL;
                                    //    if (ROW != null)
                                    //    {
                                    //    }
                                    //}
                                    int currentRowIndex = currentCell.RowIndex;
                                    //int recordIndex = SYNCFUSION_DG.ResolveToRecordIndex(currentRowIndex);
                                    int recordIndex = currentRowIndex;
                                    int totalRecords = SYNCFUSION_DG.View?.Records?.Count ?? 0;
                                    bool isLastDataRow = (recordIndex >= 0 && recordIndex - 1 == totalRecords);
                                    if (isLastDataRow && SYNCFUSION_DG.CurrentColumn != null && SYNCFUSION_DG.CurrentColumn.MappingName == "TR_DATE")
                                    {
                                        e.Handled = true;
                                        // End current edit
                                        //SYNCFUSION_DG.SelectionController.CurrentCellManager.EndEdit();
                                        //SYNCFUSION_DG.View.CommitEdit();

                                        CL_LMethods.SendKey_US(Key.Up, true); // حالا شبیه‌سازی Down Arrow

                                        // Use Dispatcher to ensure UI updates complete
                                        Dispatcher.BeginInvoke(new Action(async () =>
                                        {
                                            await Task.Delay(150); // Wait for validation

                                            int addNewRowIndex = SYNCFUSION_DG.GetAddNewRowIndex();
                                            int firstColIndex = GetFirstEditableColumnIndex();

                                            if (addNewRowIndex >= 0 && firstColIndex >= 0)
                                            {
                                                var scrollColIndex = SYNCFUSION_DG.ResolveToScrollColumnIndex(firstColIndex);
                                                var newRowPos = new RowColumnIndex(addNewRowIndex, scrollColIndex);

                                                SYNCFUSION_DG.ScrollInView(newRowPos);
                                                SYNCFUSION_DG.MoveCurrentCell(newRowPos);
                                                SYNCFUSION_DG.SelectionController?.CurrentCellManager?.BeginEdit();
                                            }
                                        }), System.Windows.Threading.DispatcherPriority.Background);

                                        //try
                                        //{
                                        //    e.Handled = true;
                                        //    var controller = SYNCFUSION_DG.SelectionController;
                                        //    if (controller?.CurrentCellManager?.CurrentCell == null) return;

                                        //    // Get the 0-based index for the current row and column.
                                        //    int rowIndex = SYNCFUSION_DG.ResolveToRecordIndex(controller.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                                        //    int columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(controller.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                                        //    // Ensure we are in a valid data row/column (not a header, etc.).
                                        //    if (columnIndex < 0) return;

                                        //    bool isLastRow = controller.CurrentCellManager.CurrentRowColumnIndex.RowIndex - 1 == SYNCFUSION_DG.View.Records.Count;
                                        //    if (isLastRow && SYNCFUSION_DG.AddNewRowPosition != AddNewRowPosition.None)
                                        //    {
                                        //        // Use the Dispatcher to ensure the UI has updated before we change the column and start editing.
                                        //        var addNewRowIndex = SYNCFUSION_DG.GetAddNewRowIndex();
                                        //        if (addNewRowIndex >= 0)
                                        //        {
                                        //            // Get the specific column index from your property.
                                        //            int startColumnIndex = SYNCFUSION_DG.ResolveToScrollColumnIndex(this.INVO_LST_SUB_DEF_INDEX_COL);
                                        //            SYNCFUSION_DG.ScrollInView(new RowColumnIndex(addNewRowIndex, 0));
                                        //            // Explicitly move the current cell to that column.
                                        //            SYNCFUSION_DG.MoveCurrentCell(new RowColumnIndex(addNewRowIndex, startColumnIndex));
                                        //        }
                                        //    }
                                        //}
                                        //catch { }
                                    }
                                    else
                                    {
                                        e.Handled = true;
                                        CL_LMethods.SendKey_US(Key.Tab, true);
                                    }
                                }
                            }
                        }
                    }

                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab, true);
                }
            }
            catch { /*ignore*/ }


        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "CUST", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            if (OpenArgs == "1")
            {
                AllowDeletions = false;
                AllowEdits = false;
            }

            FILL_ALL_COMBOBOXES();

            ReGetData(FocusOnLast: false);

            GenerateAutomaticSummary(SYNCFUSION_DG);

            CL_LMethods.FocusLastSfDataGridRow(SYNCFUSION_DG);
        }
        private void FILL_ALL_COMBOBOXES()
        {
            //MPP_COLUMN.ItemsSource = dbms.DoGetDataSQL<_MAPF_MODEL_>("SELECT MPP, MPNAME FROM dbo.TCOD_MAP_GRP").ToList();
        }
        private void ReGetData(bool FocusOnLast = true)
        {
            SFDG_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<PRICE_GRP_MODEL>($"SELECT PGID, PGNAME, TR_DATE, USERNAME, CRT, UID FROM PRICE_GRP").ToList();
            foreach (var item in MasterHead)
            {
                SFDG_DATA?.Add(item);
            }

            // Move selection to last row (if any)
            if (SYNCFUSION_DG.View != null && SFDG_DATA?.Count > 0)
                SYNCFUSION_DG.SelectedIndex = SFDG_DATA.Count - 1;

            if (FocusOnLast)
            {
                CL_LMethods.FocusLastSfDataGridRow(SYNCFUSION_DG);
            }
        }

        #region _SfDataGrid_

        private readonly FilterService<PRICE_GRP_MODEL> filterService = new FilterService<PRICE_GRP_MODEL>();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as PRICE_GRP_MODEL);
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

            var dataType = typeof(PRICE_GRP_MODEL);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(PRICE_GRP_MODEL).GetProperty(column.MappingName);
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
        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                CalculateSumForCurrentColumn(SYNCFUSION_DG);
                e.Handled = true; // Mark event as handled
            }

            // ============================================
            // ENTER: Navigate to next cell/row
            // ============================================


            //if (e.Key == Key.Delete)
            //{
            //    // Prevent deleting text inside a cell when pressing delete
            //    if (e.OriginalSource is TextBox textBox && !textBox.IsReadOnly)
            //    {
            //        return;
            //    }
            //    e.Handled = true; // Handle the key press to prevent default behavior
            //    DeleteSelectedRows();
            //}
        }
        private bool BodyIsValid(PRICE_GRP_MODEL _row)
        {
            var ROW = _row;
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //if (ROW?.PGID == null || ROW.PGID <= 0)
            //{
            //    ErrosMessages.Add(new MsgModel { MessageText_U = "شناسه گروه قیمت نمی‌تواند خالی یا صفر باشد" });
            //}

            if (string.IsNullOrEmpty(ROW?.PGNAME?.Trim()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام گروه قیمت نمی‌تواند خالی باشد" });
            }
            else if (ROW.PGNAME.Length > 250)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام گروه قیمت نمی‌تواند بیشتر از 250 کاراکتر باشد" });
            }

            if (string.IsNullOrEmpty(ROW?.USERNAME?.Trim()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کاربری نمی‌تواند خالی باشد" });
            }
            else if (ROW.USERNAME.Length > 50)
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام کاربری نمی‌تواند بیشتر از 50 کاراکتر باشد" });
            }

            if (!CheckDuplicatePGNAME(ROW))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "نام گروه قیمت تکراری است" });
            }

            // If there are errors, show them and return false
            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages
                    .Select(x => x.MessageText_U)
                    .Distinct()
                    .Select(message => new MsgModel { MessageText_U = message })
                    .ToList();

                new MsgListwin(false, ErrosMessages).ShowDialog();
                return false;
            }

            return true;
        }
        private bool CheckDuplicatePGNAME(PRICE_GRP_MODEL row)
        {
            try
            {
                if (string.IsNullOrEmpty(row?.PGNAME?.Trim()))
                {
                    return false;
                }

                var dbManager = new CL_CCNNMANAGER();

                string query;
                object parameters;

                if (row?.PGID == null)
                {
                    // For new records, check if name exists anywhere
                    query = @"SELECT COUNT(*) AS RecordCount 
                     FROM PRICE_GRP 
                     WHERE PGNAME = @PGNAME";
                    parameters = new { PGNAME = row.PGNAME.Trim() };
                }
                else
                {
                    // For existing records, check if name exists in other records (excluding current)
                    query = @"SELECT COUNT(*) AS RecordCount 
                     FROM PRICE_GRP 
                     WHERE PGNAME = @PGNAME 
                     AND PGID <> @PGID";
                    parameters = new { PGNAME = row.PGNAME.Trim(), PGID = row.PGID };
                }

                var result = dbManager.DoGetDataSQL<int>(query, parameters);

                if (result != null && result.Any())
                {
                    int count = result.First();
                    return count == 0; // No duplicates allowed
                }

                return true; // No records found, so no duplicate
            }
            catch (Exception ex)
            {
                return false; // Assume invalid on error for safety
            }
        }
        private void CancelGridEdit()
        {
            // Cancel current edit at row level (uses IEditableObject underneath)
            if (SYNCFUSION_DG?.View != null)
                SYNCFUSION_DG.View.CancelEdit();
        }
        private void DG_SUB_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            // Snapshot current row before edits (for revert)
            CURRENT_ROW_INDEX = e.RowColumnIndex.RowIndex;
            CURRENT_ROW_ITEMS = SYNCFUSION_DG.CurrentItem as PRICE_GRP_MODEL;
            wasRowSnapshot = CURRENT_ROW_ITEMS?.Clone() as PRICE_GRP_MODEL;
        }
        private void DG_SUB_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            // If user cleared MPP in the ComboBox, revert to previous value and cancel edit
            var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var mapping = SYNCFUSION_DG.Columns[columnIndex].MappingName;

            var row = SYNCFUSION_DG.CurrentItem as PRICE_GRP_MODEL;
            if (row == null) return;

            if (mapping == "MPP")
            {
                //if (row.MPP == null)
                //{
                //    ////// revert MPP to previous snapshot and cancel whole row edit (same as your DataGrid logic)
                //    //if (_wasRowSnapshot != null)
                //    //    row.MPP = _wasRowSnapshot.MPP;

                //    CancelGridEdit();
                //}
            }
        }
        private void DG_SUB_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            // Don't validate the new row template or unchanged rows
            if (e.RowData == null) { return; }

            var row = e.RowData as PRICE_GRP_MODEL;
            if (row == null) return;
            if (CL_LMethods.ConstructorRowDetector.IsPristine(row))
            {
                e.IsValid = false; // Prevents adding an empty row
                return;
            }

            if (!BodyIsValid(row))
            {
                e.IsValid = false; // keep the user in edit until valid
            }
        }
        private int GenerateNextPGID()
        {
            try
            {
                string query = "SELECT MAX(PGID) AS MaxPGID FROM PRICE_GRP";
                var result = dbms.DoGetDataSQL<int>(query);

                if (result != null && result.Any())
                {
                    var maxPGID = result.FirstOrDefault();

                    if (maxPGID > 0)
                    {
                        return maxPGID + 1;
                    }
                }
                return 1; // If no records exist, start with 1
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطا در تولید شناسه جدید", ex);
            }
        }

        private void DG_SUB_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var row = e.RowData as PRICE_GRP_MODEL;
            if (row == null) return;

            int? newId = null;

            try
            {
                if (row.PGID == 0 || row?.PGID == null) // INSERT - New Record
                {
                    // Generate next PGID if not already set
                    row.PGID = GenerateNextPGID();

                    string insertQuery = @"
                        INSERT INTO dbo.PRICE_GRP (PGID, PGNAME, TR_DATE, USERNAME,UID)
                        OUTPUT INSERTED.PGID
                        VALUES (@PGID, @PGNAME, @TR_DATE, @USERNAME, @UID)";

                    var result = dbms.DoGetDataSQL<int?>(insertQuery, new
                    {
                        PGID = row.PGID,
                        PGNAME = row.PGNAME?.Trim(),
                        TR_DATE = row.TR_DATE,
                        USERNAME = row.USERNAME?.Trim(),
                        UID = row.UID
                    });

                    if (result != null && result.Any())
                    {
                        newId = result.FirstOrDefault();
                        if (newId.HasValue)
                        {
                            row.PGID = (int)newId;
                        }
                    }
                }
                else
                {
                    string updateQuery = @"
                        UPDATE dbo.PRICE_GRP
                        SET PGNAME = @PGNAME,
                            TR_DATE = @TR_DATE,
                            USERNAME = @USERNAME
                        WHERE PGID = @PGID";

                    dbms.DoExecuteSQL(updateQuery, new
                    {
                        PGID = row.PGID,
                        PGNAME = row.PGNAME?.Trim(),
                        TR_DATE = row.TR_DATE,
                        USERNAME = row.USERNAME?.Trim(),
                    });

                }
            }
            catch (SqlException ex)
            {
                CancelGridEdit();

                if (ex.Number == 2601 || ex.Number == 2627) // Unique constraint violation
                {
                    new Msgwin(false, "این داده تکراری است!").ShowDialog();
                }
                else if (ex.Number == 515) // Cannot insert NULL
                {
                    new Msgwin(false, "فیلدهای الزامی نمی‌توانند خالی باشند").ShowDialog();
                }
                else
                {
                    new Msgwin(false, $"خطا در ذخیره‌سازی").ShowDialog();
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات!").ShowDialog();
                return;
            }
        }
        private void DG_SUB_RecordDeleting(object sender, RecordDeletingEventArgs e)
        {
            if (SYNCFUSION_DG.SelectedItems == null || SYNCFUSION_DG.SelectedItems.Count == 0) return;

            // Confirm delete
            var confirm = new Msgwin(true, "آیا مایل به حذف هستید ؟");
            confirm.ShowDialog();
            if (confirm.DialogResult != true)
            {
                e.Cancel = true;
                return;
            }

            bool anyError = false;
            List<MsgModel> errors = new List<MsgModel>();

            ////Audit once for the overall delete action
            var Rowefer = ((PRICE_GRP_MODEL)SYNCFUSION_DG.SelectedItem).Clone() as PRICE_GRP_MODEL;
            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعریف گروه بندی قیمیتی",
                    recordId: string.Join(",", e.Items.Select(i => (i as PRICE_GRP_MODEL)?.PGID.ToStringNullSafe())),
                    oldValue: Rowefer,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var obj in e.Items.ToList())
            {
                var item = obj as PRICE_GRP_MODEL;
                if (item?.PGID == null || item?.PGID == 0) continue;

                try
                {
                    dbms.DoExecuteSQL($@"DELETE FROM dbo.PRICE_GRP WHERE PGID = {item.PGID}");
                }
                catch (SqlException ex)
                {
                    anyError = true;
                    if (ex.Number == 547) // FK constraint
                        errors.Add(new MsgModel { MessageText_U = "این کد دارای گردش است و نمیتوان آنرا پاک کرد!" });
                    else
                        errors.Add(new MsgModel { MessageText_U = "حذف به دلیل خطا در بروزرسانی پایگاه داده انجام نشد!" });
                }
                catch (Exception)
                {
                    anyError = true;
                    errors.Add(new MsgModel { MessageText_U = "خطا در انجام عملیات حذف!" });
                }
            }

            if (anyError)
            {
                e.Cancel = true;
                if (errors.Any())
                {
                    errors = errors.Select(x => x.MessageText_U).Distinct()
                                   .Select(m => new MsgModel { MessageText_U = m }).ToList();
                    new MsgListwin(false, errors).ShowDialog();
                }
            }
        }
        private void DG_SUB_RecordDeleted(object sender, RecordDeletedEventArgs e)
        {
            // Keep DB & UI in sync
            //ReGetData();
        }
        private void txtMPName_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text.Length; // Place cursor at end
            }
        }

        #region Navigation Helper Methods
        /// <summary>
        /// Gets the index of the last editable column in the grid
        /// </summary>
        private int GetLastEditableColumnIndex()
        {
            try
            {
                // Get all visible, non-hidden columns that allow editing
                var editableColumns = SYNCFUSION_DG.Columns
                    .Where(c => !c.IsHidden && c.AllowEditing)
                    .ToList();

                if (editableColumns.Count == 0)
                {
                    return -1;
                }

                // Get the last editable column
                var lastEditableColumn = editableColumns.Last();

                // Return its index in the main Columns collection
                return SYNCFUSION_DG.Columns.IndexOf(lastEditableColumn);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting last editable column: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Gets the index of the first editable column in the grid
        /// </summary>
        private int GetFirstEditableColumnIndex()
        {
            try
            {
                // Get all visible, non-hidden columns that allow editing
                var editableColumns = SYNCFUSION_DG.Columns
                    .Where(c => !c.IsHidden && c.AllowEditing && !c.IsReadOnly)
                    .ToList();

                if (editableColumns.Count == 0)
                {
                    return -1;
                }

                // Get the first editable column
                var firstEditableColumn = editableColumns.First();

                // Return its index in the main Columns collection
                return SYNCFUSION_DG.Columns.IndexOf(firstEditableColumn);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting first editable column: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Checks if the current cell is in the last editable column
        /// </summary>
        private bool IsCurrentCellInLastColumn()
        {
            try
            {
                var currentCell = SYNCFUSION_DG.SelectionController?.CurrentCellManager?.CurrentCell;
                if (currentCell == null)
                {
                    return false;
                }

                int currentColumnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(currentCell.ColumnIndex);
                int lastEditableColumnIndex = GetLastEditableColumnIndex();

                return currentColumnIndex == lastEditableColumnIndex;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if current cell is in last column: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Moves focus to the AddNewRow (new row at bottom) and focuses first editable cell
        /// </summary>
        private async void MoveToNewRow()
        {
            try
            {
                // End any current edit to commit the row
                if (SYNCFUSION_DG.SelectionController?.CurrentCellManager?.CurrentCell?.IsEditing == true)
                {
                    SYNCFUSION_DG.SelectionController.CurrentCellManager.EndEdit();
                }

                // Small delay to allow row validation and commit to complete
                await Task.Delay(100);

                // Check if AddNewRow is enabled
                if (SYNCFUSION_DG.AddNewRowPosition == AddNewRowPosition.None)
                {
                    return;
                }

                // Get the AddNewRow index
                int addNewRowIndex = SYNCFUSION_DG.GetAddNewRowIndex();

                if (addNewRowIndex < 0)
                {
                    // If AddNewRow doesn't exist, try to refresh view
                    SYNCFUSION_DG.View?.Refresh();
                    await Task.Delay(50);
                    addNewRowIndex = SYNCFUSION_DG.GetAddNewRowIndex();

                    if (addNewRowIndex < 0)
                    {
                        return; // Still can't find it, exit
                    }
                }

                // Get first editable column index
                int firstEditableColumnIndex = GetFirstEditableColumnIndex();

                if (firstEditableColumnIndex < 0)
                {
                    return; // No editable columns
                }

                // Resolve to scrollable column index
                int scrollColumnIndex = SYNCFUSION_DG.ResolveToScrollColumnIndex(firstEditableColumnIndex);

                // Create RowColumnIndex for the new row's first cell
                var newRowColumnIndex = new RowColumnIndex(addNewRowIndex, scrollColumnIndex);

                // Scroll the new row into view
                SYNCFUSION_DG.ScrollInView(newRowColumnIndex);

                // Small delay to ensure scrolling completes
                await Task.Delay(50);

                // Move current cell to the new row
                SYNCFUSION_DG.MoveCurrentCell(newRowColumnIndex);

                // Begin edit on the new cell
                SYNCFUSION_DG.SelectionController?.CurrentCellManager?.BeginEdit();

                // Set focus to the grid
                SYNCFUSION_DG.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving to new row: {ex.Message}");
            }
        }

        /// <summary>
        /// Alternative method: Moves to next row (creates new if at last data row)
        /// </summary>
        private async void MoveToNextRowOrCreateNew()
        {
            try
            {
                var currentCell = SYNCFUSION_DG.SelectionController?.CurrentCellManager?.CurrentCell;
                if (currentCell == null)
                {
                    return;
                }

                // End current edit first
                if (currentCell.IsEditing)
                {
                    SYNCFUSION_DG.SelectionController.CurrentCellManager.EndEdit();
                }

                await Task.Delay(100); // Allow validation to complete

                int currentRowIndex = currentCell.RowIndex;
                //int recordIndex = SYNCFUSION_DG.ResolveToRecordIndex(currentRowIndex);
                int recordIndex = currentRowIndex;

                // Check if we're at the last data row
                int totalRecords = SYNCFUSION_DG.View?.Records?.Count ?? 0;
                bool isLastDataRow = (recordIndex >= 0 && recordIndex - 1 == totalRecords);

                if (isLastDataRow)
                {
                    // Move to AddNewRow
                    MoveToNewRow();
                }
                else
                {
                    // Move to next existing row
                    int nextRowIndex = currentRowIndex + 1;
                    int firstEditableColumnIndex = GetFirstEditableColumnIndex();

                    if (firstEditableColumnIndex < 0)
                    {
                        return;
                    }

                    int scrollColumnIndex = SYNCFUSION_DG.ResolveToScrollColumnIndex(firstEditableColumnIndex);
                    var nextRowColumnIndex = new RowColumnIndex(nextRowIndex, scrollColumnIndex);

                    SYNCFUSION_DG.ScrollInView(nextRowColumnIndex);
                    await Task.Delay(50);
                    SYNCFUSION_DG.MoveCurrentCell(nextRowColumnIndex);
                    SYNCFUSION_DG.SelectionController?.CurrentCellManager?.BeginEdit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving to next row: {ex.Message}");
            }
        }
        #endregion

    }
}
