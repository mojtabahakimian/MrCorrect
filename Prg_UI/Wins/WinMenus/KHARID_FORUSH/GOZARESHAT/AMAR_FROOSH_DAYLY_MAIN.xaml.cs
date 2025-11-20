using Functions;
using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.BulletGraph;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.KHARID_FORUSH.GOZARESHAT
{
    /// <summary>
    /// Interaction logic for AMAR_FROOSH_DAYLY_MAIN.xaml
    /// </summary>
    public partial class AMAR_FROOSH_DAYLY_MAIN : Window
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

        public AMAR_FROOSH_DAYLY_MAIN(string dT1, string dT2)
        {
            InitializeComponent();

            this.DataContext = this;

            DT1 = dT1;
            DT2 = dT2;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }

        string DT1;
        string DT2;

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public class AMAR_MODEL
        {
            public string Name { get; set; }

            [Column("1")]
            public double? Day1 { get; set; }

            [Column("2")]
            public double? Day2 { get; set; }

            [Column("3")]
            public double? Day3 { get; set; }

            [Column("4")]
            public double? Day4 { get; set; }

            [Column("5")]
            public double? Day5 { get; set; }

            [Column("6")]
            public double? Day6 { get; set; }

            [Column("7")]
            public double? Day7 { get; set; }

            [Column("8")]
            public double? Day8 { get; set; }

            [Column("9")]
            public double? Day9 { get; set; }

            [Column("10")]
            public double? Day10 { get; set; }

            [Column("11")]
            public double? Day11 { get; set; }

            [Column("12")]
            public double? Day12 { get; set; }

            [Column("13")]
            public double? Day13 { get; set; }

            [Column("14")]
            public double? Day14 { get; set; }

            [Column("15")]
            public double? Day15 { get; set; }

            [Column("16")]
            public double? Day16 { get; set; }

            [Column("17")]
            public double? Day17 { get; set; }

            [Column("18")]
            public double? Day18 { get; set; }

            [Column("19")]
            public double? Day19 { get; set; }

            [Column("20")]
            public double? Day20 { get; set; }

            [Column("21")]
            public double? Day21 { get; set; }

            [Column("22")]
            public double? Day22 { get; set; }

            [Column("23")]
            public double? Day23 { get; set; }

            [Column("24")]
            public double? Day24 { get; set; }

            [Column("25")]
            public double? Day25 { get; set; }

            [Column("26")]
            public double? Day26 { get; set; }

            [Column("27")]
            public double? Day27 { get; set; }

            [Column("28")]
            public double? Day28 { get; set; }

            [Column("29")]
            public double? Day29 { get; set; }

            [Column("30")]
            public double? Day30 { get; set; }

            [Column("31")]
            public double? Day31 { get; set; }

            public string VH { get; set; }
            public string GRP { get; set; }
            public string Vahed { get; set; }
            public string Names { get; set; }
        }

        public ObservableCollection<AMAR_MODEL> AMAR_FROOSH_DATA { get; set; } = new ObservableCollection<AMAR_MODEL>();
        public bool NowIsReady { get; private set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            AMAR_FROOSH_DATA?.Clear();

            var MasterHead = dbms.DoGetDataSQL<AMAR_MODEL>(@$"  SELECT     NAME, SUM([1]) AS [1], SUM([2]) AS [2], SUM([3]) AS [3], SUM([4]) AS [4], SUM([5]) AS [5], SUM([6]) AS [6], SUM([7]) AS [7], SUM([8]) AS [8], SUM([9])  AS [9], SUM([10]) AS [10], SUM([11]) AS [11], SUM([12]) AS [12], SUM([13]) AS [13], SUM([14]) AS [14], 
                                                                SUM([15]) AS [15], SUM([16]) AS [16], SUM([17])                       AS [17], SUM([18]) AS [18], SUM([19]) AS [19], SUM([20]) AS [20], SUM([21]) AS [21], SUM([22]) AS [22], SUM([23]) AS [23], SUM([24]) AS [24], SUM([25])   AS [25], SUM([26]) AS [26], SUM([27]) AS [27], 
                                                                SUM([28]) AS [28], SUM([29]) AS [29], SUM([30]) AS [30], SUM([31]) AS [31], VH, GRP, vahed,  NAMES FROM         (SELECT     NAME, CASE DAYY WHEN 1 THEN MEGHT ELSE 0 END AS [1], CASE DAYY WHEN 2 THEN MEGHT ELSE 0 END AS [2], CASE DAYY WHEN 3 THEN MEGHT ELSE 0 END AS [3], CASE DAYY WHEN 4 THEN MEGHT ELSE 0 END AS [4], CASE DAYY WHEN 5 THEN MEGHT ELSE 0 END AS [5], CASE DAYY WHEN 6 THEN MEGHT ELSE 0 END AS [6], CASE DAYY WHEN 7 THEN MEGHT ELSE 0 END AS [7], CASE DAYY WHEN 8 THEN MEGHT ELSE 0 END AS [8], CASE DAYY WHEN 9 THEN MEGHT ELSE 0 END AS [9], CASE DAYY WHEN 10 THEN MEGHT ELSE 0 END AS [10], CASE DAYY WHEN 11 THEN MEGHT ELSE 0 END AS [11], CASE DAYY WHEN 12 THEN MEGHT ELSE 0 END AS [12], CASE DAYY WHEN 13 THEN MEGHT ELSE 0 END AS [13], CASE DAYY WHEN 14 THEN MEGHT ELSE 0 END AS [14], CASE DAYY WHEN 15 THEN MEGHT ELSE 0 END AS [15], CASE DAYY WHEN 16 THEN MEGHT ELSE 0 END AS [16], CASE DAYY WHEN 17 THEN MEGHT ELSE 0 END AS [17], CASE DAYY WHEN 18 THEN MEGHT ELSE 0 END AS [18], CASE DAYY WHEN 19 THEN MEGHT ELSE 0 END AS [19], CASE DAYY WHEN 20 THEN MEGHT ELSE 0 END AS [20], CASE DAYY WHEN 21 THEN MEGHT ELSE 0 END AS [21], CASE DAYY WHEN 22 THEN MEGHT ELSE 0 END AS [22], CASE DAYY WHEN 23 THEN MEGHT ELSE 0 END AS [23], CASE DAYY WHEN 24 THEN MEGHT ELSE 0 END AS [24], CASE DAYY WHEN 25 THEN MEGHT ELSE 0 END AS [25], CASE DAYY WHEN 26 THEN MEGHT ELSE 0 END AS [26], CASE DAYY WHEN 27 THEN MEGHT ELSE 0 END AS [27], CASE DAYY WHEN 28 THEN MEGHT ELSE 0 END AS [28], CASE DAYY WHEN 29 THEN MEGHT ELSE 0 END AS [29], CASE DAYY WHEN 30 THEN MEGHT ELSE 0 END AS [30], CASE DAYY WHEN 31 THEN MEGHT ELSE 0 END AS [31], NAMES, vahed, GRP, VH, DAYY, date_n   FROM dbo.AMAR_FROOSH_DAYLY  WHERE     (date_n BETWEEN {DT1} AND {DT2} )) DERIVEDTBL GROUP BY NAME, VH, GRP, vahed, NAMES ").ToList();

            foreach (var item in MasterHead)
            {
                AMAR_FROOSH_DATA.Add(item);
            }

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            //{
            //    e.Handled = true;

            //    var currentRow = SYNCFUSION_DG.SelectedItem as BASKOOL_MODEL;

            //    if (currentRow?.NUMBER != null)
            //    {
            //        OpenWindow(typeof(ANBGRD_HEAD_WIN), (double)currentRow.NUMBER, "یک پنجره انبار گردانی از قبل باز شده ابتدا آنرا ببندید.");
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

        private readonly FilterService<HEAD_LST> filterService = new FilterService<HEAD_LST>();
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
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as HEAD_LST);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }
    }
}
