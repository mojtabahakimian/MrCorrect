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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.HESABDARI
{
    public partial class BEDEHKARAN_BESTANKARAN : Window
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
        public BEDEHKARAN_BESTANKARAN()
        {
            InitializeComponent();

            SYNCFUSION_DG.SelectionController = new SafeGridSelectionController(SYNCFUSION_DG);

            this.DataContext = this;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }

        public bool IsCTRLF9 { get; set; } = false;

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public ObservableCollection<Q_BEDEHBESTANH_MAIN> FACTOR_DATA { get; set; } = new ObservableCollection<Q_BEDEHBESTANH_MAIN>();
        public bool NowIsReady { get; private set; }
        public byte TAGCODE { get; private set; }

        public int KOL_PASSED { get; set; }
        public int MOIN_PASSED { get; set; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region SecuritCheck
            try
            {
                string Formname = "BEDBES";  //لیست بدهکاران و بستانکاران              
                if (IsCTRLF9) //BEDBESM	لیست بدهکاران و بستانکاران محدود شده 
                {
                    Formname = "BEDBESM";
                }
                //
                var helper = new WindowInteropHelper(this); helper.EnsureHandle(); // Critical: Ensures handle exists before access
                // 2. Run Security:
                bool HasAccess = CL_HESABDARI.SETSECURITY(this.GetType().Name, Formname, helper.Handle, this.GetType().Name);
                if (!HasAccess)
                {
                    this?.Close();
                }
                // 3. Final State Check:
                if (!this.IsLoaded) { this?.Close(); return; }
            }
            catch { try { this?.Close(); } catch { } }
            if (!this.IsLoaded) { this?.Close(); return; }
            #endregion

            FACTOR_DATA?.Clear();
            System.Collections.Generic.List<Q_BEDEHBESTANH_MAIN> MasterHead = null;

            Process Prc = Prg_UI.Functions.CL_LMethods.ProcLoader.Start();

            if (IsCTRLF9)
            {
                WINTILENAME.Content = "لیست بدهکاران و بستانکاران محدود شده";

                var limitedQuery = $@"
                    SELECT *
                    FROM BEDBESMAH{Baseknow.USERCOD}
                    WHERE (HES_K = {KOL_PASSED}) AND (HES_M = {MOIN_PASSED})
                    ORDER BY HES_K, HES_M, HES_T";

                MasterHead = dbms.DoGetDataSQL<Q_BEDEHBESTANH_MAIN>(limitedQuery).ToList();

                var limitedTransQuery = $@"
                    SELECT d.HES_K, d.HES_M, d.HES_T, h.DATE_S, d.N_S, d.SHARH,
                           ISNULL(d.BED, 0) AS BED, ISNULL(d.BES, 0) AS BES
                    FROM DEED_DTL d
                    INNER JOIN DEED_HED h ON h.N_S = d.N_S
                    WHERE d.HES_K = {KOL_PASSED} AND d.HES_M = {MOIN_PASSED}
                    ORDER BY d.HES_K, d.HES_M, d.HES_T, h.DATE_S ASC, d.N_S ASC";

                var limitedTrans = dbms.DoGetDataSQL<DebtCalcRow>(limitedTransQuery).ToList();
                var limitedLookup = limitedTrans
                    .GroupBy(r => (r.HES_K, r.HES_M, r.HES_T))
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in MasterHead)
                {
                    var key = (item.HES_K ?? 0, item.HES_M ?? 0, item.HES_T ?? 0);
                    if (limitedLookup.TryGetValue(key, out var accountRows))
                        item.LAST_DEED_DATE = ComputeLastDeedDateByCreditCoverage(accountRows);
                }
            }
            else
            {
                const string fullQuery = @"SELECT * FROM Q_BEDEHBESTANH_MAIN OPTION (FORCE ORDER, QUERYTRACEON 2312)";
                MasterHead = dbms.DoGetDataSQL<Q_BEDEHBESTANH_MAIN>(fullQuery).ToList();

                const string fullTransQuery = @"
                    SELECT d.HES_K, d.HES_M, d.HES_T, h.DATE_S, d.N_S, d.SHARH,
                           ISNULL(d.BED, 0) AS BED, ISNULL(d.BES, 0) AS BES
                    FROM DEED_DTL d
                    INNER JOIN DEED_HED h ON h.N_S = d.N_S
                    INNER JOIN Q_BEDEHBESTANH_MAIN main
                        ON main.HES_K = d.HES_K AND main.HES_M = d.HES_M AND main.HES_T = d.HES_T
                    ORDER BY d.HES_K, d.HES_M, d.HES_T, h.DATE_S ASC, d.N_S ASC";

                var fullTrans = dbms.DoGetDataSQL<DebtCalcRow>(fullTransQuery).ToList();
                var fullLookup = fullTrans
                    .GroupBy(r => (r.HES_K, r.HES_M, r.HES_T))
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in MasterHead)
                {
                    var key = (item.HES_K ?? 0, item.HES_M ?? 0, item.HES_T ?? 0);
                    if (fullLookup.TryGetValue(key, out var accountRows))
                        item.LAST_DEED_DATE = ComputeLastDeedDateByCreditCoverage(accountRows);
                }
            }

            foreach (var item in MasterHead)
            {
                FACTOR_DATA.Add(item);
            }

            Prg_UI.Functions.CL_LMethods.ProcLoader.Stop(Prc);

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }
            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as Q_BEDEHBESTANH_MAIN;

            }
        }

        #region SYNFUSION_DATA_GRID
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

        private readonly FilterService<Q_BEDEHBESTANH_MAIN> filterService = new FilterService<Q_BEDEHBESTANH_MAIN>();
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

            if (this.SYNCFUSION_DG?.Columns == null || this.SYNCFUSION_DG.Columns.Count == 0)
            {
                return;
            }

            int rowIndex = rowColumnIndex.RowIndex;
            int columnIndex = this.SYNCFUSION_DG.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            if (columnIndex < 0) return;
            if (columnIndex >= this.SYNCFUSION_DG.Columns.Count) return;

            var mappingName = this.SYNCFUSION_DG.Columns[columnIndex].MappingName; if (string.IsNullOrEmpty(mappingName)) return;
            var recordIndex = this.SYNCFUSION_DG.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0) return;

            if (recordIndex >= this.SYNCFUSION_DG.View.Records.Count)
            {
                return;
            }

            var record = this.SYNCFUSION_DG.View.Records.GetItemAt(recordIndex);

            if (record == null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                return;
            }
            var propertyInfo = record.GetType().GetProperty(mappingName);
            if (propertyInfo == null)
            {
                return;
            }
            var propertyValue = propertyInfo.GetValue(record);
            CurrentCellValue = propertyValue?.ToStringNullSafe() ?? string.Empty;

            //CurrentCellValue = record?.GetType()?.GetProperty(mappingName ?? string.Empty)?.GetValue(record)?.ToString();
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
                universControl.PopNotifyShowUp($" ... در حال اماده سازی فایل اکسل این عملیات مدتی طول خواهد کشید", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as Q_BEDEHBESTANH_MAIN);
            // Refresh the filter to update the view
            SYNCFUSION_DG.View.RefreshFilter();
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Q_BEDEHBESTANH_MAIN row })
            {
                if (row != null && row?.HES != null)
                {
                    var JAMFAC = dbms.DoGetDataSQL<JAMFACTPRS>($"SELECT TOP 1 NUMBER FROM dbo.JAMFACTPRS WHERE (CUST_NO = N'{row.HES}')").FirstOrDefault();
                    if (JAMFAC != null)
                    {
                        new WIN_JAMFACTPRS(row.HES).ShowDialog();
                    }
                }
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Q_BEDEHBESTANH_MAIN row })
            {
                if (row != null && row?.HES != null)
                {
                    new F_MENU_KOL_MOIN_TAFZIL(row?.HES.ToString());
                }
            }
        }

        #region LAST_DEED_DATE - FindNewDebtStartByCreditCoverage Algorithm

        private class DebtCalcRow
        {
            public int HES_K { get; set; }
            public int HES_M { get; set; }
            public int HES_T { get; set; }
            public long? DATE_S { get; set; }
            public double? N_S { get; set; }
            public string SHARH { get; set; }
            public double BED { get; set; }
            public double BES { get; set; }
        }

        /// <summary>
        /// محاسبه تاریخ ایجاد مانده بر اساس الگوریتم FindNewDebtStartByCreditCoverage
        /// (معادل C# متد همنام در R_DAFTAR_MOIN_LIST)
        /// سطرها باید از قبل بر اساس DATE_S ASC, N_S ASC مرتب باشند.
        /// </summary>
        private static long? ComputeLastDeedDateByCreditCoverage(System.Collections.Generic.List<DebtCalcRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return null;

            int targetIndex = rows.Count - 1;

            // Step 1: جمع کل بستانکارها
            decimal totalCredit = 0m;
            for (int i = 0; i <= targetIndex; i++)
                totalCredit += (decimal)rows[i].BES;

            // Step 2: اگر بستانکاری نداریم، اولین سطر غیر افتتاحیه
            if (totalCredit <= 0)
            {
                for (int i = 0; i <= targetIndex; i++)
                {
                    if (!IsOpeningRowCalc(rows[i].SHARH))
                        return rows[i].DATE_S;
                }
                return null;
            }

            // Step 3: جمع بدهکارها از اول — یافتن cover row
            decimal runningDebit = 0m;
            int coverRowIndex = -1;
            int nearestRowIndex = -1;
            decimal nearestDiff = decimal.MaxValue;

            for (int i = 0; i <= targetIndex; i++)
            {
                decimal bed = (decimal)rows[i].BED;
                if (bed <= 0)
                    continue;

                runningDebit += bed;

                decimal diff = Math.Abs(runningDebit - totalCredit);
                if (diff < nearestDiff)
                {
                    nearestDiff = diff;
                    nearestRowIndex = i;
                }

                if (runningDebit >= totalCredit)
                {
                    coverRowIndex = i;
                    break;
                }
            }

            // اگر دقیق/عبوری پیدا نشد، نزدیک‌ترین را بگیر
            if (coverRowIndex < 0)
                coverRowIndex = nearestRowIndex;

            if (coverRowIndex < 0)
                return null;

            // Step 4: اولین سطر غیر افتتاحیه بعد از cover row = شروع بدهی جدید
            int startIndex = coverRowIndex + 1;
            if (startIndex > targetIndex)
                startIndex = coverRowIndex;

            while (startIndex <= targetIndex)
            {
                if (!IsOpeningRowCalc(rows[startIndex].SHARH))
                    return rows[startIndex].DATE_S;
                startIndex++;
            }

            return rows[coverRowIndex].DATE_S;
        }

        private static bool IsOpeningRowCalc(string sharh)
        {
            return (sharh ?? string.Empty).Contains("افتتاح");
        }

        #endregion
    }

}
