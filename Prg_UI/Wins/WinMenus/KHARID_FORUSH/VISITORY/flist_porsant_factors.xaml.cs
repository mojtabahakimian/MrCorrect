using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wins.WinMenus.ANBAR;
using static Prg_UI.Functions.CL_LMethods;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.BulletGraph;
using Prg_Proccessy.MODELS;
using Syncfusion.Data;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using System.Diagnostics;
using System.Windows.Controls;
using Prg_Proccessy.Generaly;
using static Stimulsoft.Report.StiOptions;

namespace Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY
{
    /// <summary>
    /// Interaction logic for flist_porsant_factors.xaml
    /// </summary>
    public partial class flist_porsant_factors : Window
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

        public flist_porsant_factors(string _cONDITION)
        {
            if (_cONDITION is not null)
            {
                Condition = _cONDITION;
            }

            InitializeComponent();

            this.DataContext = this;
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();


        UniversControl universControl = new UniversControl();
        public ObservableCollection<FLP> FLIST_PORSANT_DATA { get; set; } = new ObservableCollection<FLP>();
        public bool NowIsReady { get; private set; }
        public string Condition { get; private set; } = "";
        public DataTemplate CheckedTemplate { get; set; }
        public DataTemplate UncheckedTemplate { get; set; }
        public class FLP
        {
            public double? NUMBER { get; set; }
            public string? hes { get; set; }
            public string? NAME { get; set; }
            public double? Expr2 { get; set; }
            public long? DATE_N { get; set; }
            public string? CUST_NO { get; set; }
            public double? DARSAD { get; set; }
            public double? PURSANT { get; set; }
            public string? TOZIH { get; set; }
            public bool? STAT { get; set; }
            public int? PORID { get; set; }
            public string? Expr1 { get; set; }
            public string? TEL { get; set; }
            public string? MOBILE { get; set; }
            public double? TAG { get; set; }
            public int? AGHLAM { get; set; }
            public int? DEPATMAN { get; set; }
            public int? SHIFT { get; set; }
            public int? CUST_KIND { get; set; }
            public string? USER_NAME { get; set; }
            public string? ROUTE_NAME { get; set; }
            public double? Expr3 { get; set; }
            public double? NUMBER1 { get; set; }
            public int? mm { get; set; }
            public double? SumOfMABL_K { get; set; }
        }

        #region ComboBoxes
        public class Q1
        {
            public int? PORID { get; set; }
            public string Expr1 { get; set; }
        }
        public class Q2
        {
            public int? DEPATMAN { get; set; }
            public string DEPNAME { get; set; }
        }
        public class Q3
        {
            public int? SHIFT_ID { get; set; }
            public string? SHNAME { get; set; }
        }
        public class Q4
        {
            public int? CUST_COD { get; set; }
            public string? CUSTKNAME { get; set; }
        }
        #endregion

        public void FILL_ALL_COMBOBOXES()
        {
            PORID_COLUMN.ItemsSource = dbms.DoGetDataSQL<Q1>("SELECT VISITORS_PORSANT.PORID, CAST(VISITORS_PORSANT.PORID AS nvarchar) + N' - ' + CAST(VISITORS_PORSANT.VDATE AS nvarchar) + N' - ' + ISNULL(CUSTKIND.CUSTKNAME, N'بدون گروه (همه)') + N' - ' + ISNULL(VISITORS_PORSANT.COMMENT, N' ') + N' - ' + CUST_HESAB.NAME AS Expr1 FROM VISITORS_PORSANT INNER JOIN CUST_HESAB ON VISITORS_PORSANT.HES = CUST_HESAB.hes LEFT OUTER JOIN CUSTKIND ON VISITORS_PORSANT.CUST_COD = CUSTKIND.CUST_COD").ToList();

            DEPATMAN_COLUMN.ItemsSource = dbms.DoGetDataSQL<Q2>("SELECT DEPATMAN, DEPNAME FROM DEPART").ToList();

            SHIFT_COLUMN.ItemsSource = dbms.DoGetDataSQL<Q3>("SELECT SHIFT_ID, SHNAME FROM SHIFT").ToList();

            CUST_KIND_COLUMN.ItemsSource = dbms.DoGetDataSQL<Q4>("SELECT CUST_COD, CUSTKNAME FROM CUSTKIND").ToList();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            if ( CL_HESABDARI.LETSGO("DEPEMAL"))
            {
                if (Condition == "")
                {
                    Condition = " WHERE (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }
                else
                {
                    Condition = Condition + " AND  (DEPATMAN = " + CL_Generaly.VAHED_OF_USER + ")";
                }

            }

            FLIST_PORSANT_DATA?.Clear();

            var MasterHead = dbms.DoGetDataSQL<FLP>(@$"SELECT * FROM list_porsant_factors {Condition}").ToList();

            foreach (var item in MasterHead)
            {
                FLIST_PORSANT_DATA.Add(item);
            }

            GenerateAutomaticSummary(SYNCFUSION_DG);
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            //{
            //    e.Handled = true;

            //    var currentRow = SYNCFUSION_DG.SelectedItem as FLP;

            //    if (currentRow?.NUMBER != null)
            //    {
            //        OpenWindow(typeof(HEAD_LST_RASID_OTHER_WIN), (double)currentRow.NUMBER, "یک پنجره رسید انبار از قبل باز شده ابتدا آنرا ببندید.");
            //    }

            //}
        }
        public void OpenWindow(Type windowType, object parameter, string errorMessage)
        {
            if (windowType == null || !typeof(Window).IsAssignableFrom(windowType))
                return;

            if (!CL_LMethods.IsWindowOpen(windowType)) //CL_LMethods.IsWindowOpen<HEAD_LST_FROOSH22>()
            {
                var constructor = windowType.GetConstructor(new[] { parameter.GetType() });
                if (constructor != null)
                {
                    var window = (Window)constructor.Invoke(new[] { parameter });
                    window.Show();
                }
            }
            else
            {
                new Msgwin(false, errorMessage).ShowDialog();
            }
        }

        #region _SfDataGrid_
        private readonly FilterService<FLP> filterService = new FilterService<FLP>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();
        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        private void SYNCFUSION_DG_CurrentCellActivated(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs e) // Event handler for when a cell is activated in the data grid
        {
            UpdateCurrentCellValue(e.CurrentRowColumnIndex);
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

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as FLP);
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
                e.Handled = true;
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

            var dataType = typeof(FLP);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                var propertyInfo = typeof(FLP).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "sumofmabl_k" || column.MappingName.ToLower() == "expr2"))
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
                else if (column.MappingName.ToLower() == "mm")
                {
                    // متنی یا غیر عددی → فقط Count
                    summaryColumns.Add(new GridSummaryColumn
                    {
                        Name = column.MappingName + "Count",
                        MappingName = column.MappingName,
                        SummaryType = Syncfusion.Data.SummaryType.CountAggregate,
                        Format = "تعداد: {Count:N0}"
                    });
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
    }

    public class StatCheckBoxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CheckedTemplate { get; set; }
        public DataTemplate UncheckedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var row = item as Prg_UI.Wins.WinMenus.KHARID_FORUSH.VISITORY.flist_porsant_factors.FLP; // یا مدل دیتای خودت
            if (row != null && row.STAT == true)
                return CheckedTemplate;
            return UncheckedTemplate;
        }
    }
}
