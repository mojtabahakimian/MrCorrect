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
using System.Text;
using Syncfusion.Data;
using Prg_UI.HelperWins;
using System.Windows.Media;
using Prg_Proccessy.SQLMODELS;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;

namespace Prg_UI.Wins.WinSamplesEmpty
{
    /// <summary>
    /// Interaction logic for WinRealExampleSfDG.xaml
    /// </summary>
    public partial class WinRealExampleSfDG : Window
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
        public WinRealExampleSfDG()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ObservableCollection<TCOD_MAP> SFDG_DATA { get; set; } = new ObservableCollection<TCOD_MAP>();

        UniversControl universControl = new UniversControl();
        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public class _MAPF_MODEL_
        {
            public int? MPP { get; set; }
            public string? MPNAME { get; set; }
        }
        public TCOD_MAP? CURRENT_ROW_ITEMS { get; private set; }
        public int CURRENT_ROW_INDEX { get; private set; }

        private TCOD_MAP? wasRowSnapshot; // previous values for revert-on-empty in combo

        private int _defaultColIndex = -1;
        public int INVO_LST_SUB_DEF_INDEX_COL
        {
            get
            {
                if (_defaultColIndex == -1 && SYNCFUSION_DG.Columns.Count > 0)
                {
                    var col = SYNCFUSION_DG.Columns.FirstOrDefault(c => c.MappingName == "MPP");
                    _defaultColIndex = col != null ? SYNCFUSION_DG.Columns.IndexOf(col) : 0;
                }
                return _defaultColIndex < 0 ? 0 : _defaultColIndex;
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (SYNCFUSION_DG.IsKeyboardFocusWithin)
                    {
                        if (SYNCFUSION_DG.CurrentColumn != null)
                        {
                        }
                    }

                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab, true);

                }
            }
            catch { /*ignore*/ }

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                var currentCell = SYNCFUSION_DG.SelectionController.CurrentCellManager.CurrentCell;
                if (currentCell != null && !currentCell.IsEditing)
                {
                    var ROW = SYNCFUSION_DG.SelectedItem as TCOD_MAP;

                    if (ROW != null)
                    {
                    }
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.SYNCFUSION_DG.SelectionController = new GridSelectionControllerExt(this.SYNCFUSION_DG);

            FILL_ALL_COMBOBOXES();

            ReGetData(FocusOnLast: false);

            GenerateAutomaticSummary(SYNCFUSION_DG);

            CL_LMethods.FocusLastSfDataGridRow(SYNCFUSION_DG);
        }
        private void FILL_ALL_COMBOBOXES()
        {
            MPP_COLUMN.ItemsSource = dbms.DoGetDataSQL<_MAPF_MODEL_>("SELECT MPP, MPNAME FROM dbo.TCOD_MAP_GRP").ToList();
        }
        private void ReGetData(bool FocusOnLast = true)
        {
            SFDG_DATA?.Clear();
            var MasterHead = dbms.DoGetDataSQL<TCOD_MAP>($"SELECT * FROM TCOD_MAP").ToList();
            foreach (var item in MasterHead)
            {
                SFDG_DATA.Add(item);
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

        private readonly FilterService<TCOD_MAP> filterService = new FilterService<TCOD_MAP>();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as TCOD_MAP);
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

            var dataType = typeof(TCOD_MAP);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(TCOD_MAP).GetProperty(column.MappingName);
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

            //if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            //{
            //    e.Handled = true;
            //    var grid = sender as SfDataGrid;
            //    grid.ScrollInView(new RowColumnIndex(grid.GetLastDataRowIndex(), 0));
            //    grid.SelectionController.MoveCurrentCell(new RowColumnIndex(grid.GetLastDataRowIndex(), 0));
            //}

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
        private bool BodyIsValid(TCOD_MAP _row)
        {
            var ROW = _row;

            List<MsgModel> ErrosMessages = new List<MsgModel>();
            if (string.IsNullOrEmpty(ROW?.MPCODE.ToStringNullSafe()))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = "کد نمی تواند خالی باشد" });
            }
            if (ErrosMessages.Count > 0)
            {
                ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                new MsgListwin(false, ErrosMessages).ShowDialog();
                return false;
            }
            return true;
        }
        private void CancelGridEdit()
        {
            // Cancel current edit at row level (uses IEditableObject underneath)
            if (SYNCFUSION_DG?.View != null)
                SYNCFUSION_DG.View.CancelEdit();
        }
        private void TCOD_MAP_SUB_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            // Snapshot current row before edits (for revert)
            CURRENT_ROW_INDEX = e.RowColumnIndex.RowIndex;
            CURRENT_ROW_ITEMS = SYNCFUSION_DG.CurrentItem as TCOD_MAP;
            wasRowSnapshot = CURRENT_ROW_ITEMS?.Clone() as TCOD_MAP;
        }
        private void TCOD_MAP_SUB_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }

            // If user cleared MPP in the ComboBox, revert to previous value and cancel edit
            var columnIndex = SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var mapping = SYNCFUSION_DG.Columns[columnIndex].MappingName;

            var row = SYNCFUSION_DG.CurrentItem as TCOD_MAP;
            if (row == null) return;

            if (mapping == "MPP")
            {
                if (row.MPP == null)
                {
                    ////// revert MPP to previous snapshot and cancel whole row edit (same as your DataGrid logic)
                    //if (_wasRowSnapshot != null)
                    //    row.MPP = _wasRowSnapshot.MPP;

                    CancelGridEdit();
                }
            }
        }
        private void TCOD_MAP_SUB_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape)) { return; }
            // Don't validate the new row template or unchanged rows
            if (e.RowData == null) { return; }

            var row = e.RowData as TCOD_MAP;
            if (row == null) return;
            if (ConstructorRowDetector.IsPristine(row)) // Assuming IsPristine checks if it's an untouched new row
            {
                e.IsValid = false; // Prevents adding an empty row
                return;
            }

            if (!BodyIsValid(row))
            {
                e.IsValid = false; // keep the user in edit until valid
            }
        }
        private void TCOD_MAP_SUB_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var row = e.RowData as TCOD_MAP;
            if (row == null) return;

            long? newId = null;

            try
            {
                if (row.ID is null) // INSERT
                {
                    newId = dbms.DoGetDataSQL<long?>($@"
                        INSERT INTO dbo.TCOD_MAP(MPP, MPCODE, MPNAME)
                        OUTPUT INSERTED.ID
                        VALUES({row.MPP}, {row.MPCODE}, N'{row.MPNAME}')
                    ").FirstOrDefault();
                }
                else // UPDATE
                {
                    dbms.DoExecuteSQL($@"
                        UPDATE dbo.TCOD_MAP
                        SET MPP = {row.MPP},
                            MPCODE = {row.MPCODE},
                            MPNAME = N'{row.MPNAME}'
                        WHERE ID = {row.ID}
                    ");
                }
            }
            catch (SqlException ex)
            {
                CancelGridEdit();

                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    new Msgwin(false, "این کد تکراری است !").ShowDialog();
                }
                else
                {
                    new Msgwin(false, "خطا در انجام ذخیره!").ShowDialog(); return;
                }
                return;
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در انجام عملیات!").ShowDialog();
                return;
            }

            if (newId != null)
                row.ID = newId;
        }
        private void TCOD_MAP_SUB_RecordDeleting(object sender, RecordDeletingEventArgs e)
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

            // Audit once for the overall delete action
            _ = AuditLogger.LogActionAsync(
                    actionType: "DELETE",
                    tableName: "تعریف کد مپ شماره فنی",
                    recordId: string.Join(",", e.Items.Select(i => (i as TCOD_MAP)?.ID?.ToStringNullSafe())),
                    oldValue: null,
                    newValue: null,
                    additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            foreach (var obj in e.Items.ToList())
            {
                var item = obj as TCOD_MAP;
                if (item?.ID == null) continue;

                try
                {
                    dbms.DoExecuteSQL($@"DELETE FROM dbo.TCOD_MAP WHERE ID = {item.ID}");
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
        private void TCOD_MAP_SUB_RecordDeleted(object sender, RecordDeletedEventArgs e)
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
    }
}
