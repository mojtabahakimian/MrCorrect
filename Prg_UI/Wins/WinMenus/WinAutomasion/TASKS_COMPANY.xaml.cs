using MaterialDesignThemes.Wpf;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Prg_UI.Functions.CL_LMethods;
using Functions;
using Prg_Proccessy.SQLMODELS;
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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using System.Diagnostics;
using static Prg_UI.Wins.WinMenus.WinAutomasion.MAIN;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;

namespace Prg_UI.Wins.WinMenus.WinAutomasion
{
    public partial class TASKS_COMPANY : Window
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

        public TASKS_COMPANY(string _Openargs_)
        {
            InitializeComponent();

            this.DataContext = this;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");

            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");

            OperArgs = _Openargs_;
        }

        UniversControl universControl = new UniversControl();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        public ObservableCollection<TASKS> FACTOR_DATA { get; set; } = new ObservableCollection<TASKS>();
        public bool NowIsReady { get; private set; }
        public string OperArgs { get; }

        public ObservableCollection<CutsomPeriority_Model> PERIORITY_COMBO_DATA { get; } = new();
        public ObservableCollection<CutsomStatus_Model> STATUS_COMBO_DATA { get; } = new();
        public ObservableCollection<COMBOPERSONEL> PERSONEL_COMBO_DATA { get; } = new();

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process Prc = ProcLoader.Start();

            FACTOR_DATA?.Clear();

            //string query = @$" SELECT TSK.IDNUM, CH.NAME, TSK.GR,
            //                       TSK.PERSONEL, TSK.TASK,
            //                       TSK.PERIORITY, TSK.STATUS,
            //                       TSK.STDATE, TSK.STTIME,
            //                       TSK.ENDATE, TSK.ENTIME,
            //                       TSK.USERNAME, TSK.COMP_COD,
            //                       TSK.SUMTIME, TSK.pic,
            //                       TSK.ss, TSK.skid,
            //                       TSK.num, TSK.tg,
            //                       TSK.CTIM, TSK.USERCO, TSK.SEE
            //                    FROM dbo.TASKS AS TSK WITH (INDEX(IX_TASKS_Status1))
            //                         LEFT HASH JOIN dbo.CUST_HESAB AS CH
            //                             ON CH.hes = TSK.COMP_COD
            //                    WHERE TSK.COMP_COD = N'{OperArgs}'
            //                    ORDER BY TSK.IDNUM";

            //var MasterHead = dbms.DoGetDataSQL<TASKS>(query);

            //foreach (var item in MasterHead)
            //{
            //    FACTOR_DATA.Add(item);
            //}

            FILL_ALL_COMBOBOXES();

            ReGetData();

            //SYNCFUSION_DG.ColumnSizer = GridLengthUnitType.Auto;


            //if (isSummed)
            //{
            //    GenerateAutomaticSummary(SYNCFUSION_DG);
            //}

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();

                UpdateRowCountLabel();
            }

            //if (RestrictionMessages.Any())
            //{
            //    LBL_STATE.Content = "دسترسی شما با این شرایط محدود شده است: ");
            //    LBL_STATE.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    LBL_STATE.Visibility = Visibility.Collapsed;
            //}

            ProcLoader.Stop(Prc);
        }

        private void FILL_ALL_COMBOBOXES()
        {
            // وضعیت
            STATUS_COMBO_DATA.Clear();
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 1, STATUS_NAME = "انجام نشده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 2, STATUS_NAME = "انجام شده" });
            STATUS_COMBO_DATA.Add(new CutsomStatus_Model { STATUS = 3, STATUS_NAME = "لغو شده" });

            // اولویت
            PERIORITY_COMBO_DATA.Clear();
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 1, PERIORITY_NAME = "فوری" });
            PERIORITY_COMBO_DATA.Add(new CutsomPeriority_Model { PERIORITY = 2, PERIORITY_NAME = "معمولی" });

            ////زیر مجموعه کاربران مجاز برای محدود کردن پرونده دیدن
            //const string sqlSub = @"
            //     SELECT
            //          sd.SAL_NAME,
            //          sd.PSAL_NAME,
            //          sd.GRSAL,
            //          sd.ENABL,
            //          cs.SUBUSERCO     AS IDD,   
            //          cs.SUBUSERCO     AS USERCO 
            //     FROM dbo.CHARTSAZMANI        cs
            //     LEFT JOIN SALA_DTL           sd  ON cs.SUBUSERCO = sd.IDD
            //     LEFT JOIN USER_PERSONEL_ORDER uo  ON cs.SUBUSERCO = uo.PERSONEL_ID
            //                                      AND uo.USER_ID   = @UserId
            //     WHERE cs.USERCO  = @UserId
            //     ORDER BY
            //          CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,  -- اولويات كاربر
            //          uo.SORT_ORDER,
            //          sd.SAL_NAME;";
            //List<COMBOPERSONEL> sub_rst_personel = dbms.DoGetDataSQL<COMBOPERSONEL>(sqlSub, new { UserId = Baseknow.USERCOD }).ToList();

            // مجری (مرتب‌سازی بر اساس سفارش کاربر)

            const string sql = @"
                SELECT sd.SAL_NAME,sd.GRSAL, sd.ENABL, sd.IDD
                FROM SALA_DTL sd
                LEFT JOIN USER_PERSONEL_ORDER uo 
                     ON sd.IDD = uo.PERSONEL_ID AND uo.USER_ID = @UserId
                ORDER BY
                     CASE WHEN uo.SORT_ORDER IS NULL THEN 1 ELSE 0 END,
                     uo.SORT_ORDER, sd.SAL_NAME";

            //var rows = dbms.DoGetDataSQL<COMBOPERSONEL>(@"SELECT SAL_NAME, IDD FROM dbo.SALA_DTL").ToList();
            var rows = dbms.DoGetDataSQL<COMBOPERSONEL>(sql, new { UserId = Baseknow.USERCOD }).ToList();
            PERSONEL_COMBO_DATA.Clear();
            foreach (var item in rows)
            {
                item.SAL_NAME = CL_HESABDARI.DECODEUN(item.SAL_NAME);
                PERSONEL_COMBO_DATA.Add(item);
            }

        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && SYNCFUSION_DG.SelectedItem != null)
            {
                e.Handled = true;

                var currentRow = SYNCFUSION_DG.SelectedItem as TASKS;

                //switch (currentRow?.TAGCODE)
                //{
                //    case 1: // رسید خرید
                //        if (currentRow?.NUMBER != null)
                //        {
                //            //OpenWindow(typeof(HEAD_LST_RASID), (double)currentRow.NUMBER, "یک پنجره رسید خرید از قبل باز شده ابتدا آنرا ببندید.");

                //            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, this, (double)currentRow.NUMBER);
                //        }
                //        break;
                //}
            }
        }

        #region FilterBy
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

        private readonly FilterService<TASKS> filterService = new FilterService<TASKS>();
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
            SYNCFUSION_DG.View.Filter = item => filterService.ApplyFilter(item as TASKS);
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

            var dataType = typeof(TASKS);

            //foreach (var column in SYNCFUSION_DG.Columns)
            foreach (var column in _DG_.Columns)
            {
                var propertyInfo = typeof(TASKS).GetProperty(column.MappingName);
                if (propertyInfo == null)
                    continue;

                //var propertyInfo = dataType.GetProperty(column.MappingName);
                //if (propertyInfo == null)
                //    continue;

                //if (IsNumericType(propertyInfo.PropertyType) && (column.MappingName.ToLower() == "meghk" || column.MappingName.ToLower() == "mablk"))
                if (CheckField(column.MappingName.ToUpper()))
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

        private bool CheckField(string fieldName)
        {
            var numericFields = new[]
            {
                "MEGH", "MEGHk", "MABL", "MABL_K", "N_KOL", "N_MOIN", "IMBAA",
                "MEGH_MAR", "MAS", "N_RASID", "KHFR", "GHFR", "N_TAF", "TOTALARZ",
                "TAMIR", "MIN_M", "MAX_M", "N_SEF", "B_SEF", "MABL_F", "AVRAGE",
                "MABRIAL", "VAZN", "TKHN"
            };

            return numericFields.Contains(fieldName.ToUpper());
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

        private void ReGetData()
        {
            IEnumerable<TASKS> items;
            if (IsCompCod(OperArgs))
            {
                //items = dbms.DoGetDataSQL<TASKS>(
                //    "SELECT IDNUM, GR, PERSONEL, TASK, PERIORITY, STATUS, STDATE, STTIME, ENDATE, ENTIME, USERNAME, COMP_COD, SUMTIME, pic, ss, skid, num, tg, CTIM, USERCO, SEE, SEET, CRT, UID FROM dbo.TASKS WHERE COMP_COD = @Code ORDER BY IDNUM",
                //    new { Code = OperArgs }).ToList();

                const string query = @" SELECT  TSK.IDNUM, CH.NAME, TSK.GR,
                                                TSK.PERSONEL, TSK.TASK,
                                                TSK.PERIORITY, TSK.STATUS,
                                                TSK.STDATE, TSK.STTIME,
                                                TSK.ENDATE, TSK.ENTIME,
                                                TSK.USERNAME, TSK.COMP_COD,
                                                TSK.SUMTIME, TSK.pic,
                                                TSK.ss, TSK.skid,
                                                TSK.num, TSK.tg,
                                                TSK.CTIM, TSK.USERCO, TSK.SEE
                                        FROM dbo.TASKS AS TSK
                                        LEFT JOIN dbo.CUST_HESAB AS CH
                                          ON CH.hes = TSK.COMP_COD
                                        WHERE TSK.COMP_COD = @operargs
                                        ORDER BY TSK.IDNUM";

                items = dbms.DoGetDataSQL<TASKS>(query, new { operargs = OperArgs }).ToList();
            }
            else
            {
                items = dbms.DoGetDataSQL<TASKS>(
                    "SELECT IDNUM, GR, PERSONEL, TASK, PERIORITY, STATUS, STDATE, STTIME, ENDATE, ENTIME, USERNAME, COMP_COD, SUMTIME, pic, ss, skid, num, tg, CTIM, USERCO, SEE, SEET, CRT, UID FROM dbo.TASKS WHERE USERNAME = @User ORDER BY IDNUM",
                    new { User = OperArgs }).ToList();
            }

            FACTOR_DATA.Clear();
            foreach (var item in items)
            {
                FACTOR_DATA.Add(item);
            }
        }

        private static bool IsCompCod(string arg) => !string.IsNullOrEmpty(arg) && arg.Replace("-", "").All(char.IsDigit);
        private void BTN_ISEND_Click(object sender, RoutedEventArgs e)
        {
            var CurrentRow = SYNCFUSION_DG.SelectedItem as TASKS;

            //if (CurrentRow != null && CurrentRow?.NUMBER != null && CurrentRow?.NUMBER > 0)
            //{
            //    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.WIN_MOADIAN_SINGLE, this, Convert.ToDouble(CurrentRow.NUMBER));
            //}
        }


        private void BTN_ISEND_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (!(btn.Tag is null))
                {
                    if ((btn.Tag as TASKS)?.IDNUM is not null)
                    {
                        var Row = btn.Tag as TASKS;
                        if (Row != null && Row?.IDNUM > 0)
                        {
                            new WinEVENTS((long)Row.IDNUM).ShowDialog();
                        }
                    }
                }
            }

        }

        private void SYNCFUSION_DG_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
        }
        private void SYNCFUSION_DG_CurrentCellEndEdit(object sender, CurrentCellEndEditEventArgs e)
        {

        }

        private void ChangePersonelButton_Click(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.SelectedItem is not TASKS selectedTask || selectedTask.IDNUM is null)
            {
                universControl.PopNotifyShow("لطفاً یک ردیف را انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (PERSONEL_CHANGE_COMBO.SelectedValue is null || !int.TryParse(PERSONEL_CHANGE_COMBO.SelectedValue.ToString(), out var personelId))
            {
                universControl.PopNotifyShow("مجری جدید را انتخاب کنید", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            if (selectedTask.PERSONEL == personelId)
            {
                universControl.PopNotifyShow("مجری انتخاب‌شده با مجری فعلی یکسان است", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                return;
            }

            string wasPeronName = PERSONEL_COMBO_DATA.FirstOrDefault(p => p.IDD == selectedTask.PERSONEL)?.SAL_NAME ?? string.Empty;
            string selectedPersonName = PERSONEL_COMBO_DATA.FirstOrDefault(p => p.IDD == personelId)?.SAL_NAME ?? string.Empty;
            Msgwin msgwin = new Msgwin(true, $"آیا از تغییر این مجری [{wasPeronName}] به ← [{selectedPersonName}] برای شماره اتوماسیون {selectedTask.IDNUM} مطمئن هستید ؟ ");
            msgwin.ShowDialog();
            if (msgwin.DialogResult != true)
            {
                return;
            }

            try
            {
                DateTime dt = DateTime.MinValue;
                dt = DateTime.Now;
                if (Convert.ToBoolean(Baseknow.TRANSF))
                {
                    CL_HESABDARI.TR("TASKS", "(IDNUM = " + selectedTask.IDNUM + " )", dt, 1);
                    CL_HESABDARI.TR("EVENTS", "(IDNUM = " + selectedTask.IDNUM + " )", dt, 1);
                }

                const string updateSql = "UPDATE dbo.TASKS SET PERSONEL = @Personel WHERE IDNUM = @Id";
                dbms.DoExecuteSQL(updateSql, new { Personel = personelId, Id = selectedTask.IDNUM });

                try
                {
                    string eventDescription = $"از بخش مشاهده پرونده ارجاع شد به : تغییر مجری [{wasPeronName}] به ← [{selectedPersonName}]";
                    const string insertEvent = @"INSERT INTO events (IDNUM, USERNAME, EVENTS, STDATE, STTIME, SKID, NUM, TG)
                                          VALUES (@IdNum, @UserName, @EventDesc, @StDate, @StTime, @SkId, @Num, @Tg)";
                    dbms.DoExecuteSQL(insertEvent, new
                    {
                        IdNum = selectedTask.IDNUM,
                        UserName = Baseknow.UUSER ?? "Unknown",
                        EventDesc = eventDescription,
                        StDate = Convert.ToInt32(Tarikh.GoGetPersianDate(true)),
                        StTime = Convert.ToInt32(DateTime.Now.ToString("HHmm")),
                        SkId = selectedTask.skid,
                        Num = selectedTask.num,
                        Tg = selectedTask.tg
                    });
                }
                catch { }

                universControl.PopNotifyShowUp("مجری با موفقیت تغییر کرد", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green, 1);

                SYNCFUSION_DG.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                universControl.PopNotifyShow($"خطا در تغییر مجری", Pop1, Pop1Text1, Pop_Border1, "#E5EC2B2B");
                ReGetData();
            }

        }

        private void BTN_COLUMN_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not TASKS task || task.IDNUM is null || task.IDNUM <= 0)
            {
                return;
            }

            new WinEVENTS((long)task.IDNUM, isReadOnly: true).ShowDialog();
        }
    }
}
