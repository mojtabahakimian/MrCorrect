using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
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
using Wins.WinMenus.Checkha.NAZDESANDOGH;
using static Wins.WinMenus.Checkha.NAZDESANDOGH.VOSULDIALOG;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.Checkha
{
    public partial class CHEK_VLISTALL : Window
    {

        public CHEK_VLISTALL(string _DT1_, string _DT2_, string _MMOIN_)
        {
            InitializeComponent();

            this.DataContext = this;

            DT1 = _DT1_;
            DT2 = _DT2_;
            MMOIN = _MMOIN_;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
            GridResourceWrapper.SetResources(Assembly.Load("MrCorrect"), "Prg_UI");
        }
        #region Header Window Begin
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
        #endregion

        public ObservableCollection<NAZDBANK_D_MODEL> SFDATAGRID_DATA { get; set; } = new ObservableCollection<NAZDBANK_D_MODEL>();
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
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
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
                //اعلام وصول تک سطر
                if ((e.Key is Key.OemComma && Keyboard.Modifiers == ModifierKeys.None) || (e.Key is Key.Oem5 && Keyboard.Modifiers == ModifierKeys.None)) //پ
                {
                    DCheckManagement(BTN_SINGLE_PASS_CHECK);
                }
                //انتقال چک به صندوق
                if (e.Key is Key.E && Keyboard.Modifiers == ModifierKeys.None) //ث
                {
                    DCheckManagement(BTN_TRANSFER_SANDOGH);
                }
                //اعلام وصول گروهی
                if (e.Key is Key.G && Keyboard.Modifiers == ModifierKeys.None) //ل
                {
                    DCheckManagement(BTN_GROUP_PASS);
                }
                //به حساب گذاشتن
                if (e.Key is Key.F && Keyboard.Modifiers == ModifierKeys.None) //ب
                {
                    DCheckManagement(BTN_BEHESAB);
                }
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

            I_AM_CHEK_VLISTALL = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

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
            VAZ_COLUMN.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "نزد صندوق" },
                new COMBOYMODEL { ID = 2, NAME = "نزد بانك" },
                new COMBOYMODEL { ID = 3, NAME = "وصول شده" },
                new COMBOYMODEL { ID = 4, NAME = "واگذار شده" },
                new COMBOYMODEL { ID = 5, NAME = "برگشت شده" },
                new COMBOYMODEL { ID = 6, NAME = "مسترد شده" },
                new COMBOYMODEL { ID = 7, NAME = "حذف شده" }
            };

            //MappingName="VAZ" SelectedValuePath="TNUMBER" DisplayMemberPath="NAME" 
            //VAZ_COLUMN.ItemsSource = dbms.DoGetDataSQL<TDETA_HES>($"SELECT TNUMBER, NAME FROM TDETA_HES WHERE (N_KOL = {CL_HESABDARI.GETKOL(Baseknow.ADA)}) AND (NUMBER = {CL_HESABDARI.GETMOIN(Baseknow.ADA)})").ToList();


            //موقعیت چک
            SANDUGH_COLUMN.ItemsSource = dbms.DoGetDataSQL<TDETA_HES>($"SELECT * FROM TDETA_HES WHERE(N_KOL = {CL_HESABDARI.GETKOL(Baseknow.ADA)}) AND(NUMBER = 1)").ToList();
        }

        private void ReGetData()
        {
            string RecordSource = "";
            //string SpecialCondition = $" AND (dbo.PGET_LST.NAHVA IN (2))  "; //شرط برای اینکه فقط چک های دریافتی باشه  : جلوگیری از تکراری شدن به خاطر چک های پرداختی یا واگذار شده در خزانه
            string SpecialCondition = $" AND (dbo.PAY_GETD.KIND = 1 OR dbo.PAY_GETD.KIND IS NULL)  "; //فقط چک های دریافتی

            SpecialCondition = ""; //Reset it
            if (string.IsNullOrEmpty(MMOIN))
            {
                RecordSource = "SELECT PAY_GETD.ID, PAY_GETD.N_SERI, PAY_GETD.BANK, PAY_GETD.DATE_S, PAY_GETD.DATE, PAY_GETD.SHOBEH, PAY_GETD.MABL, PAY_GETD.NAME_TAH, PAY_GETD.N_HESAB, PAY_GETD.N_S, TCOD_BANKS.NAMES, PAY_GETD.RADIF, PAY_GETD.N_KOL, PAY_GETD.N_MOIN, PAY_GETD.N_KOL2,  PAY_GETD.N_MOIN2, PAY_GETD.N_KOL3, PAY_GETD.N_MOIN3, COD_HESAB.BANK AS BKK, PAY_GETD.N_TAF, PAY_GETD.N_TAF2,  PAY_GETD.N_TAF3, PAY_GETD.VAZ, CUST_HESAB.NAME AS Expr1, PAY_GETD.SANDUGH, ISNULL(HEAD_LST.CUST_KIND,  ISNULL(PGET_HED.CUST_KIND, NULL)) AS CUST_KIND, ISNULL(HEAD_LST.USER_NAME, ISNULL(PGET_HED.USER_NAME, NULL)) AS USER_NAME,  ISNULL(HEAD_LST.DEPATMAN, ISNULL(PGET_HED.DEPATMAN, NULL)) AS DEPATMAN, ISNULL(HEAD_LST.SHIFT, ISNULL(PGET_HED.SHIFT, NULL))  AS SHIFT, PAY_GETD.KIND, PAY_GETD.HES1, PAY_GETD.HES2, PAY_GETD.HES3, PAY_GETD.SAYADI " + "  FROM         TCOD_BANKS INNER JOIN    PAY_GETD LEFT OUTER JOIN   COD_HESAB ON PAY_GETD.N_TAF = COD_HESAB.MOIN ON TCOD_BANKS.CODE = PAY_GETD.BANK LEFT OUTER JOIN   PGET_HED INNER JOIN   PGET_LST ON PGET_HED.ID = PGET_LST.ID AND PGET_HED.DATE = PGET_LST.DATE ON PAY_GETD.N_SERI = PGET_LST.N_SERI AND   PAY_GETD.BANK = PGET_LST.BANK AND PAY_GETD.MABL = PGET_LST.MABL LEFT OUTER JOIN  HEAD_LST ON PAY_GETD.NUMBER = HEAD_LST.NUMBER AND PAY_GETD.TAG = HEAD_LST.TAG LEFT OUTER JOIN   CUST_HESAB ON RTRIM(CAST(PAY_GETD.N_KOL AS nvarchar)) + '-' + RTRIM(CAST(PAY_GETD.N_MOIN AS nvarchar))  + '-' + RTRIM(CAST(PAY_GETD.N_TAF AS nvarchar)) = CUST_HESAB.hes  " +
                    " WHERE     (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.N_KOL3 IS NULL) AND (dbo.PAY_GETD.N_KOL =" + Baseknow.BANKHA + " OR  dbo.PAY_GETD.N_KOL IS NULL)   AND  DATE_S >= " + DT1 + " AND DATE_S <= " + DT2 + $" {SpecialCondition} ORDER BY PAY_GETD.DATE_S ";
            }
            else
            {
                RecordSource = "SELECT PAY_GETD.ID, PAY_GETD.N_SERI, PAY_GETD.BANK, PAY_GETD.DATE_S, PAY_GETD.DATE, PAY_GETD.SHOBEH, PAY_GETD.MABL, PAY_GETD.NAME_TAH, PAY_GETD.N_HESAB, PAY_GETD.N_S, TCOD_BANKS.NAMES, PAY_GETD.RADIF, PAY_GETD.N_KOL, PAY_GETD.N_MOIN, PAY_GETD.N_KOL2,  PAY_GETD.N_MOIN2, PAY_GETD.N_KOL3, PAY_GETD.N_MOIN3, COD_HESAB.BANK AS BKK, PAY_GETD.N_TAF, PAY_GETD.N_TAF2,  PAY_GETD.N_TAF3, PAY_GETD.VAZ, CUST_HESAB.NAME AS Expr1, PAY_GETD.SANDUGH, ISNULL(HEAD_LST.CUST_KIND,  ISNULL(PGET_HED.CUST_KIND, NULL)) AS CUST_KIND, ISNULL(HEAD_LST.USER_NAME, ISNULL(PGET_HED.USER_NAME, NULL)) AS USER_NAME,  ISNULL(HEAD_LST.DEPATMAN, ISNULL(PGET_HED.DEPATMAN, NULL)) AS DEPATMAN, ISNULL(HEAD_LST.SHIFT, ISNULL(PGET_HED.SHIFT, NULL))  AS SHIFT, PAY_GETD.KIND, PAY_GETD.HES1, PAY_GETD.HES2, PAY_GETD.HES3 , PAY_GETD.SAYADI " + "  FROM         TCOD_BANKS INNER JOIN    PAY_GETD LEFT OUTER JOIN   COD_HESAB ON PAY_GETD.N_TAF = COD_HESAB.MOIN ON TCOD_BANKS.CODE = PAY_GETD.BANK LEFT OUTER JOIN   PGET_HED INNER JOIN   PGET_LST ON PGET_HED.ID = PGET_LST.ID AND PGET_HED.DATE = PGET_LST.DATE ON PAY_GETD.N_SERI = PGET_LST.N_SERI AND   PAY_GETD.BANK = PGET_LST.BANK AND PAY_GETD.MABL = PGET_LST.MABL LEFT OUTER JOIN  HEAD_LST ON PAY_GETD.NUMBER = HEAD_LST.NUMBER AND PAY_GETD.TAG = HEAD_LST.TAG LEFT OUTER JOIN   CUST_HESAB ON RTRIM(CAST(PAY_GETD.N_KOL AS nvarchar)) + '-' + RTRIM(CAST(PAY_GETD.N_MOIN AS nvarchar))  + '-' + RTRIM(CAST(PAY_GETD.N_TAF AS nvarchar)) = CUST_HESAB.hes  " +
                    " WHERE     (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.N_KOL3 IS NULL) AND (dbo.PAY_GETD.N_KOL =" + Baseknow.BANKHA + " OR  dbo.PAY_GETD.N_KOL IS NULL)   AND  DATE_S >= " + DT1 + " AND DATE_S <= " + DT2 + "  AND NAME_TAH LIKE '%" + MMOIN + "%'" + $" {SpecialCondition} ORDER BY PAY_GETD.DATE_S ";
            }

            SFDATAGRID_DATA?.Clear();
            var RST = dbms.DoGetDataSQL<NAZDBANK_D_MODEL>(RecordSource).ToList();
            foreach (var item in RST)
            {
                SFDATAGRID_DATA.Add(item);
            }

            ROWCOUNT_LABEL.Content = SFDATAGRID_DATA.Count;

        }

        #region _SfDataGrid_
        private readonly FilterService<NAZDBANK_D_MODEL> filterService = new FilterService<NAZDBANK_D_MODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private string? CurrentCellValue = null;
        private RowColumnIndex CurrentCellIndex;
        public string SelectedSfDgTextCell { get; private set; }
        public string DT1 { get; private set; } = "10000101";
        public string DT2 { get; private set; } = "99991230";
        public string MMOIN { get; private set; } = null;

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
            SFDATAGRID_SUB.View.Filter = item => filterService.ApplyFilter(item as NAZDBANK_D_MODEL);
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

            var dataType = typeof(NAZDBANK_D_MODEL);

            //foreach (var column in SFDATAGRID_SUB.Columns)
            foreach (var column in _DG_.Columns.OfType<GridTextColumn>())
            {
                if (column.MappingName == "MABL")
                {
                    var propertyInfo = typeof(NAZDBANK_D_MODEL).GetProperty(column.MappingName);
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


        public COME_FROM_VOSULDIALOG2 ITEM_SELECTED_VOSUL { get; set; }
        /// <summary>
        /// وضعیت چک انتخاب شده توسط پنجره انتقال به صندوق
        /// </summary>
        public int? VAZ_USER_CHOOSED { get; set; }
        public Visual I_AM_CHEK_VLISTALL { get; private set; }

        private void DCheckManagement(object BTNCALLED)
        {
            if (SFDATAGRID_SUB.SelectedItems.Count == 0)
            {
                universControl.PopNotifyShowUp("رکوردی انتخاب نشده!", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Red);
                return;
            }

            if (!string.IsNullOrEmpty(GetSelectedText())) //اگر درحالت ادیت بود برگرد
            {
                return;
            }

            List<NAZDBANK_D_MODEL> SelectedRows = SFDATAGRID_SUB.SelectedItems.OfType<NAZDBANK_D_MODEL>().ToList();

            ITEM_SELECTED_VOSUL = new COME_FROM_VOSULDIALOG2();
            VAZ_USER_CHOOSED = null;

            List<MsgModel> MyMessages = new List<MsgModel>();

            string HADA;
            var mab = default(double);
            double max_ns, MABL_CHK;
            string SHART;
            double? CKOLV = default, CMOINV = default, CTAFV = default, CTAF2V = default, CTAF3V = default, CTAF4V = default, CKOL = default, CMOIN = default, CTAF = default, CTAF2 = default, CTAF3 = default, CTAF4 = default, CKOLD = default, CMOIND = default, CTAFD = default, CTAF2D = default, CTAF3D = default, CTAF4D = default;
            var NNS = default(long);
            var NB = default(long);
            if (SelectedRows.FirstOrDefault()?.KIND == 1 || SelectedRows.FirstOrDefault()?.KIND is null)
            {
                HADA = Baseknow.ADA;
            }
            else
            {
                HADA = Baseknow.ADV;
            }

            //SFDATAGRID_DATA

            //اعلام وصول چک
            if (BTNCALLED == BTN_SINGLE_PASS_CHECK) // if (KeyAscii == 96 || KeyAscii == 129 || KeyAscii == 1662)
            {
                //DoCmd.OpenForm("VOSULDIALOG", default, default, default, default, acDialog, Interaction.IIf(IsNull(this.N_TAF), "", this.N_KOL + "-" + this.N_MOIN + "-" + this.N_TAF));
                var _HES_ = SelectedRows.FirstOrDefault().N_KOL + "-" + SelectedRows.FirstOrDefault().N_MOIN + "-" + SelectedRows.FirstOrDefault().N_TAF;

                VOSULDIALOG WIN_VOSUL = new VOSULDIALOG(I_AM_CHEK_VLISTALL, SelectedRows, "CHEK_VLISTALL", _HES_, SelectedRows.FirstOrDefault().N_SERI);
                WIN_VOSUL.ShowDialog();

                if (ITEM_SELECTED_VOSUL.HHMOIN_VOSUL == null)
                {
                    return;
                }

                var rstfirst = dbms.DoGetDataSQL<double?>("SELECT Max(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED").FirstOrDefault();
                if (rstfirst == null)
                {
                    max_ns = 1d;
                }
                else
                {
                    max_ns = (double)(rstfirst + 1);
                }
                //SHRST.Open("deed_hed", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM dbo.DEED_HED WHERE NO_S = 6 AND DATE_S = " + ITEM_SELECTED_VOSUL.DTS_DATE).FirstOrDefault();
                if (SHRST == null)
                {
                    var _N_S_ = max_ns;
                    var _DATE_S_ = ITEM_SELECTED_VOSUL.DTS_DATE;
                    var _SHARH_S_ = "اعلام وصول چكهاي دريافتي";
                    var _GHATEI_ = 0;
                    var _NO_S_ = 6;
                    var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                    var _UID_ = Baseknow.USERCOD;
                    var _QRE_base_ = dbms.DoGetDataSQL<int?>($@"INSERT INTO dbo.DEED_HED(N_S, DATE_S, SHARH_S,GHATEI, NO_S, USER_NAME,UID)
                                                       OUTPUT INSERTED.base
                                                       VALUES({_N_S_},
                                                       {_DATE_S_} ,
                                                       N'{_SHARH_S_}' ,
                                                       {_GHATEI_},
                                                       {_NO_S_} ,
                                                       N'{_USER_NAME_}' ,
                                                       {_UID_} )").FirstOrDefault();
                    NB = (int)_QRE_base_;

                }
                else
                {
                    max_ns = SHRST.N_S;
                    NB = SHRST.@base;
                }
                NNS = (long)Math.Round(max_ns);
                //rst.Open("CHKREC_H", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                {
                    var rst = dbms.DoGetDataSQL<CHKREC_H>("SELECT * FROM CHKREC_H WHERE [DATE] = " + ITEM_SELECTED_VOSUL.DTS_DATE).FirstOrDefault();
                    if (rst == null)
                    {
                        //rst.AddNew();
                        var _DATE_ = ITEM_SELECTED_VOSUL.DTS_DATE;
                        var _MOLAH_ = "وصولي ثبت شده توسط گزارش";
                        var _N_S_ = max_ns;
                        //rst.update();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.CHKREC_H(DATE, MOLAH, N_S)
                                                     VALUES({_DATE_}, N'{_MOLAH_}' , {_N_S_} )");
                    }
                }
                {
                    var rst = dbms.DoGetDataSQL<CHRE_LST>("SELECT * FROM CHRE_LST WHERE N_SERI =" + SelectedRows.FirstOrDefault().N_SERI + " AND BANK = " + SelectedRows.FirstOrDefault().BANK + " AND DATE_S = " + SelectedRows.FirstOrDefault().DATE_S).FirstOrDefault();
                    if (rst == null)
                    {
                        //rst.AddNew();
                        var _N_SERI_ = SelectedRows.FirstOrDefault().N_SERI;
                        var _BANK_ = SelectedRows.FirstOrDefault().BANK;
                        var _DATE_S_ = SelectedRows.FirstOrDefault().DATE_S;
                        var _DATE_ = ITEM_SELECTED_VOSUL.DTS_DATE;

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.CHRE_LST(N_SERI, BANK, DATE_S, DATE)
                                                     VALUES({_N_SERI_},
                                                     {_BANK_} ,
                                                     {_DATE_S_} ,
                                                     {_DATE_} )");
                        //rst.update();
                    }
                }

                {
                    string _WHERE_ = " WHERE N_SERI =" + SelectedRows.FirstOrDefault().N_SERI + " AND BANK = " + SelectedRows.FirstOrDefault().BANK + " AND DATE_S = " + SelectedRows.FirstOrDefault().DATE_S;
                    var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD " + _WHERE_).FirstOrDefault();
                    if (rst != null)
                    {
                        var _N_KOL3_ = Baseknow.BANKHA;
                        var _N_MOIN3_ = CL_HESABDARI.GETMOIN(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                        var _N_TAF3_ = CL_HESABDARI.GETTAF(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                        var _N_KOL_ = Baseknow.BANKHA;
                        var _N_MOIN_ = CL_HESABDARI.GETMOIN(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                        var _N_TAF_ = CL_HESABDARI.GETTAF(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                        var _HES1_ = ITEM_SELECTED_VOSUL.HHMOIN_VOSUL;
                        var _HES3_ = ITEM_SELECTED_VOSUL.HHMOIN_VOSUL;
                        var _N_S_ = NNS;
                        //rst.update();
                        dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET 
                                                     N_KOL3 = {_N_KOL3_} , N_MOIN3 = {_N_MOIN3_} , N_TAF3 = {_N_TAF3_} ,  N_KOL = {_N_KOL_},
                                                     N_MOIN = {_N_MOIN_} , N_TAF = {_N_TAF_} , HES1 = N'{_HES1_}' , HES3 = N'{_HES3_}' , N_S = {_N_S_} , 
                                                     VAZ = 3
                                                     {_WHERE_}");
                        mab = (double)rst.MABL;
                    }
                }

                CL_HESABDARI.GETDLOG(3, SelectedRows.FirstOrDefault().N_SERI.ToString(), (int)SelectedRows.FirstOrDefault().BANK, (long)SelectedRows.FirstOrDefault().DATE_S, (int)SelectedRows.FirstOrDefault().SANDUGH);
                //rst.Open("PAY_GETD_LOG", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //rst.AddNew();
                //rst.Fields("N_SERI") = this.N_SERI;
                //rst.Fields("BANK") = this.BANK;
                //rst.Fields("DATE_S") = this.DATE_S;
                //rst.Fields("DATE_V") = FARSIDATE(DateTime.Now);
                //rst.Fields("DATETIM") = DateTime.Now;
                //rst.Fields("VAZ") = 3;
                //rst.Fields("SANDUGH") = this.SANDUGH;
                //rst.Fields("USER_NAME") = UCurrentUser();
                //rst.update();

                dbms.DoExecuteSQL("DELETE FROM DEED_DTL WHERE (((DEED_DTL.N_S)= " + max_ns + " ))");

                if (!string.IsNullOrEmpty(Baseknow.ADA))
                {
                    CL_HESABDARI.GETTAF3(Baseknow.ADA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                }
                if (!string.IsNullOrEmpty(Baseknow.ADV))
                {
                    CL_HESABDARI.GETTAF3(Baseknow.ADV, ref CKOLV, ref CMOINV, ref CTAFV, ref CTAF2V, ref CTAF3V, ref CTAF4V);
                }

                {
                    var rst = dbms.DoGetDataSQL<VOSUL_MODEL_QRE1>("SELECT dbo.CHKREC_H.*, dbo.CHRE_LST.*, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH,dbo.PAY_GETD.N_S, dbo.PAY_GETD.MABL, dbo.PAY_GETD.N_KOL,  dbo.PAY_GETD.N_MOIN, dbo.PAY_GETD.N_TAF, dbo.PAY_GETD.KIND, dbo.PAY_GETD.HES1    FROM         dbo.CHKREC_H INNER JOIN dbo.CHRE_LST ON dbo.CHKREC_H.DATE = dbo.CHRE_LST.DATE INNER JOIN  dbo.PAY_GETD ON dbo.CHRE_LST.N_SERI = dbo.PAY_GETD.N_SERI AND dbo.CHRE_LST.BANK = dbo.PAY_GETD.BANK AND  dbo.CHRE_LST.DATE_S = dbo.PAY_GETD.DATE_S INNER JOIN dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE WHERE     (dbo.CHKREC_H.DATE = " + ITEM_SELECTED_VOSUL.DTS_DATE + ")").ToList();
                    foreach (var item in rst) //while (!rst.EOF)
                    {
                        //SDRST.AddNew(); //INSERT
                        var _N_S_ = NNS;

                        string? HES_K = null;
                        string? HES_M = null;
                        string? HES_T = null;
                        string? HES_T2 = null;
                        string? HES_T3 = null;
                        string? HES_T4 = null;
                        string? hes = null;

                        if (item.KIND == 1 || item.KIND == null)
                        {
                            HES_K = CKOL.ToStringNullSafe();
                            HES_M = CMOIN.ToStringNullSafe();
                            HES_T = CTAF.ToStringNullSafe();
                            HES_T2 = CTAF2.ToStringNullSafe();
                            HES_T3 = CTAF3.ToStringNullSafe();
                            HES_T4 = CTAF4.ToStringNullSafe();
                            hes = Baseknow.ADA;
                        }
                        else
                        {
                            HES_K = CKOLV.ToStringNullSafe();
                            HES_M = CMOINV.ToStringNullSafe();
                            HES_T = CTAFV.ToStringNullSafe();
                            HES_T2 = CTAF2V.ToStringNullSafe();
                            HES_T3 = CTAF3V.ToStringNullSafe();
                            HES_T4 = CTAF4V.ToStringNullSafe();
                            hes = Baseknow.ADV;
                        }
                        var N_SERI = item.N_SERI;
                        var BANK = item.BANK;
                        var BES = item.MABL;
                        var SHARH = Strings.Left(" چك " + item.N_SERI + " بانك " + item.NAMES + " " + item.SHOBEH + " مورخ " + Strings.Format(item.DATE_S, "####/##/##"), 255);
                        //SDRST.update();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES, HES_T2, HES_T3, HES_T4)
                                                 VALUES({_N_S_},
                                                 {HES_K},
                                                 {HES_M} ,
                                                 {HES_T} ,
                                                 N'{SHARH}' ,
                                                 0 ,
                                                 {BES} ,
                                                 {N_SERI} ,
                                                 {BANK} ,
                                                 N'{hes}' ,
                                                 {(string.IsNullOrEmpty(HES_T2) ? "NULL" : HES_T2)} ,
                                                 {(string.IsNullOrEmpty(HES_T3) ? "NULL" : HES_T3)} ,
                                                 {(string.IsNullOrEmpty(HES_T4) ? "NULL" : HES_T4)} )");

                        CL_HESABDARI.GETTAF3(item.HES1, ref CKOLD, ref CMOIND, ref CTAFD, ref CTAF2D, ref CTAF3D, ref CTAF4D);

                        //SDRST.AddNew();
                        //SDRST.Fields("N_S") = NNS;
                        //SDRST.Fields("HES_K") = CKOLD;
                        //SDRST.Fields("HES_M") = CMOIND;
                        //SDRST.Fields("HES_T") = CTAFD;
                        //SDRST.Fields("HES_T2") = CTAF2D;
                        //SDRST.Fields("HES_T3") = CTAF3D;
                        //SDRST.Fields("HES_T4") = CTAF4D;
                        //SDRST.Fields("hes") = item.HES1;
                        //SDRST.Fields("N_SERI") = item.N_SERI;
                        //SDRST.Fields("BANK") = item.BANK;
                        //SDRST.Fields("BED") = item.MABL;

                        var SHARH_ = Strings.Left(" چك " + item.N_SERI + " بانك " + item.NAMES + " " + item.SHOBEH + " مورخ " + Strings.Format(item.DATE_S, "####/##/##"), 255);
                        //SDRST.update();

                        dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES, HES_T2, HES_T3, HES_T4)
                                                 VALUES({NNS},
                                                 {CKOLD},
                                                 {CMOIND} ,
                                                 {CTAFD} ,
                                                 N'{SHARH_}' ,
                                                 {item.MABL} ,
                                                 0 ,
                                                 {item.N_SERI} ,
                                                 {item.BANK} ,
                                                 N'{item.HES1}' ,
                                                 {(string.IsNullOrEmpty(CTAF2D.ToStringNullSafe()) ? "NULL" : CTAF2D)} ,
                                                 {(string.IsNullOrEmpty(CTAF3D.ToStringNullSafe()) ? "NULL" : CTAF3D)} ,
                                                 {(string.IsNullOrEmpty(CTAF4D.ToStringNullSafe()) ? "NULL" : CTAF4D)} )");

                        item.N_S = NNS;
                        mab = (double)item.MABL;
                        //rst.update();
                    }
                    new Msgwin(false, "ثبت در سند شماره :" + NNS + "  شماره مبناي سند : " + NB /*+ "  به مبلغ : " + Strings.Format(Convert.ToInt64(mab), "#,###")*/).ShowDialog();
                }

                foreach (var row in SFDATAGRID_DATA.OfType<NAZDBANK_D_MODEL>().ToList())
                {
                    if (row.ID == SelectedRows?.FirstOrDefault()?.ID)
                    {
                        SFDATAGRID_DATA.Remove(row);
                    }
                }

            }

            // تغيير صندوق
            if (BTNCALLED == BTN_TRANSFER_SANDOGH) //if (KeyAscii == 69 | KeyAscii == 101 | KeyAscii == 1579)
            {
                new VSANDUGHDIALOG(I_AM_CHEK_VLISTALL, (int)SelectedRows.FirstOrDefault().VAZ).ShowDialog();

                if (VAZ_USER_CHOOSED == null)
                {
                    return;
                }

                foreach (var item in SelectedRows)
                {
                    string _WHERE_ = " WHERE N_SERI =" + item.N_SERI + " AND BANK = " + item.BANK + " AND DATE_S = " + item.DATE_S;
                    var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD" + _WHERE_).FirstOrDefault();
                    if (rst != null)
                    {
                        //rst.Fields("VAZ") = Forms["VSANDUGHDIALOG"]["VAZ"];
                        dbms.DoExecuteSQL($"UPDATE dbo.PAY_GETD SET VAZ = {VAZ_USER_CHOOSED} {_WHERE_} ");
                        item.VAZ = VAZ_USER_CHOOSED;
                        //rst.update();
                    }
                }
                SFDATAGRID_SUB.View.Refresh();

                universControl.PopNotifyShowUp("انتقال به صندوق مورد نظر انجام شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }

            // وصولي گروهي
            if (BTNCALLED == BTN_GROUP_PASS) //if (KeyAscii == 71 | KeyAscii == 103 | KeyAscii == 1604)
            {
                //SHRST.Open("deed_hed", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                var _HES_ = SelectedRows.FirstOrDefault().N_KOL + "-" + SelectedRows.FirstOrDefault().N_MOIN + "-" + SelectedRows.FirstOrDefault().N_TAF;
                new VOSULDIALOG2(I_AM_CHEK_VLISTALL, "CHEK_VLISTALL", _HES_).ShowDialog();
                //DoCmd.OpenForm("VOSULDIALOG2", default, default, default, default, acDialog, Interaction.IIf(IsNull(this.N_TAF), "", this.N_KOL + "-" + this.N_MOIN + "-" + this.N_TAF));

                if (ITEM_SELECTED_VOSUL.HHMOIN_VOSUL == null)
                {
                    return;
                }

                long JAM = 0; //برای روز وصول چک

                foreach (var item in SelectedRows)
                {
                    var rstfirst = dbms.DoGetDataSQL<double?>("SELECT Max(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED").FirstOrDefault();
                    if (rstfirst == null)
                    {
                        max_ns = 1d;
                    }
                    else
                    {
                        max_ns = (double)(rstfirst + 1);
                    }
                    switch (ITEM_SELECTED_VOSUL.VCLC)
                    {
                        case 1:
                            {
                                JAM = (long)item.DATE_S;
                                break;
                            }
                        case 2:
                            {
                                JAM = CL_HESABDARI.ADDDAY((long)item.DATE_S, ITEM_SELECTED_VOSUL.NUMBERIC_DTC_DAY_2);
                                break;
                            }
                        case 3:
                            {
                                JAM = Convert.ToInt64(ITEM_SELECTED_VOSUL.DTS_DATE);
                                break;
                            }
                    }
                    var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM dbo.DEED_HED WHERE NO_S = 6 AND DATE_S = " + JAM).FirstOrDefault();
                    if (SHRST == null)
                    {
                        var _N_S_ = max_ns;
                        var _DATE_S_ = JAM;
                        var _SHARH_S_ = "اعلام وصول چكهاي دريافتي";
                        var _GHATEI_ = 0;
                        var _NO_S_ = 6;
                        var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                        var _UID_ = Baseknow.USERCOD;
                        var _QRE_base_ = dbms.DoGetDataSQL<int?>($@"INSERT INTO dbo.DEED_HED(N_S, DATE_S, SHARH_S,GHATEI, NO_S, USER_NAME,UID)
                                                       OUTPUT INSERTED.base
                                                       VALUES({_N_S_},
                                                       {_DATE_S_} ,
                                                       N'{_SHARH_S_}' ,
                                                       {_GHATEI_},
                                                       {_NO_S_} ,
                                                       N'{_USER_NAME_}' ,
                                                       {_UID_} )").FirstOrDefault();
                        NB = (int)_QRE_base_;
                    }
                    else
                    {
                        max_ns = (int)SHRST.N_S;
                        NB = SHRST.@base;
                    }
                    NNS = (long)Math.Round(max_ns);

                    {
                        var rst = dbms.DoGetDataSQL<CHKREC_H>("SELECT * FROM CHKREC_H WHERE [DATE] = " + JAM).FirstOrDefault();
                        if (rst == null)
                        {
                            var _DATE_ = JAM;
                            var _MOLAH_ = "وصولي ثبت شده توسط گزارش";
                            var _N_S_ = max_ns;
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.CHKREC_H(DATE, MOLAH, N_S)
                                                     VALUES({_DATE_}, N'{_MOLAH_}' , {_N_S_} )");
                        }
                    }

                    {
                        var rst = dbms.DoGetDataSQL<CHRE_LST>("SELECT * FROM CHRE_LST WHERE N_SERI =" + item.N_SERI + " AND BANK = " + item.BANK + " AND DATE_S = " + item.DATE_S).FirstOrDefault();
                        if (rst == null)
                        {
                            var _N_SERI_ = item.N_SERI;
                            var _BANK_ = item.BANK;
                            var _DATE_S_ = item.DATE_S;
                            var _DATE_ = JAM;

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.CHRE_LST(N_SERI, BANK, DATE_S, DATE)
                                                     VALUES({_N_SERI_},
                                                     {_BANK_} ,
                                                     {_DATE_S_} ,
                                                     {_DATE_} )");
                        }
                    }

                    {
                        string _WHERE_ = " WHERE N_SERI =" + item.N_SERI + " AND BANK = " + item.BANK + " AND DATE_S = " + item.DATE_S;
                        var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD " + _WHERE_).FirstOrDefault();
                        if (rst != null)
                        {
                            var _N_KOL3_ = Baseknow.BANKHA;
                            var _N_MOIN3_ = CL_HESABDARI.GETMOIN(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                            var _N_TAF3_ = CL_HESABDARI.GETTAF(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                            var _N_KOL_ = Baseknow.BANKHA;
                            var _N_MOIN_ = CL_HESABDARI.GETMOIN(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                            var _N_TAF_ = CL_HESABDARI.GETTAF(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                            var _HES1_ = ITEM_SELECTED_VOSUL.HHMOIN_VOSUL;
                            var _HES3_ = ITEM_SELECTED_VOSUL.HHMOIN_VOSUL;
                            var _N_S_ = NNS;

                            dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET 
                                                     N_KOL3 = {_N_KOL3_} , N_MOIN3 = {_N_MOIN3_} , N_TAF3 = {_N_TAF3_} ,  N_KOL = {_N_KOL_},
                                                     N_MOIN = {_N_MOIN_} , N_TAF = {_N_TAF_} , HES1 = N'{_HES1_}' , HES3 = N'{_HES3_}' , N_S = {_N_S_} ,
                                                     VAZ = 3        
                                                     {_WHERE_}");
                        }
                    }
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                         VALUES({item.N_SERI}, {item.BANK} , {item.DATE_S} , {CL_HESABDARI.FARSIDATE()} , GETDATE(),3 , {item.SANDUGH} , N'{CL_HESABDARI.UCurrentUser()}' )");

                    dbms.DoExecuteSQL("DELETE FROM DEED_DTL WHERE (((DEED_DTL.N_S)= " + max_ns + " ))");

                    if (!string.IsNullOrEmpty(Baseknow.ADA))
                    {
                        CL_HESABDARI.GETTAF3(Baseknow.ADA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    }
                    if (!string.IsNullOrEmpty(Baseknow.ADV))
                    {
                        CL_HESABDARI.GETTAF3(Baseknow.ADV, ref CKOLV, ref CMOINV, ref CTAFV, ref CTAF2V, ref CTAF3V, ref CTAF4V);
                    }

                    {
                        var rst = dbms.DoGetDataSQL<VOSUL_MODEL_QRE1>("SELECT dbo.CHKREC_H.*, dbo.CHRE_LST.*, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH,dbo.PAY_GETD.N_S, dbo.PAY_GETD.MABL, dbo.PAY_GETD.N_KOL,  dbo.PAY_GETD.N_MOIN, dbo.PAY_GETD.N_TAF, dbo.PAY_GETD.KIND, dbo.PAY_GETD.HES1    FROM         dbo.CHKREC_H INNER JOIN dbo.CHRE_LST ON dbo.CHKREC_H.DATE = dbo.CHRE_LST.DATE INNER JOIN  dbo.PAY_GETD ON dbo.CHRE_LST.N_SERI = dbo.PAY_GETD.N_SERI AND dbo.CHRE_LST.BANK = dbo.PAY_GETD.BANK AND  dbo.CHRE_LST.DATE_S = dbo.PAY_GETD.DATE_S INNER JOIN dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE WHERE     (dbo.CHKREC_H.DATE = " + JAM + ")").ToList();
                        foreach (var rst_field in rst)
                        {
                            var _N_S_ = NNS;

                            string? HES_K = null;
                            string? HES_M = null;
                            string? HES_T = null;
                            string? HES_T2 = null;
                            string? HES_T3 = null;
                            string? HES_T4 = null;
                            string? hes = null;

                            if (rst_field.KIND == 1 || rst_field.KIND == null)
                            {
                                HES_K = CKOL.ToStringNullSafe();
                                HES_M = CMOIN.ToStringNullSafe();
                                HES_T = CTAF.ToStringNullSafe();
                                HES_T2 = CTAF2.ToStringNullSafe();
                                HES_T3 = CTAF3.ToStringNullSafe();
                                HES_T4 = CTAF4.ToStringNullSafe();
                                hes = Baseknow.ADA;
                            }
                            else
                            {
                                HES_K = CKOLV.ToStringNullSafe();
                                HES_M = CMOINV.ToStringNullSafe();
                                HES_T = CTAFV.ToStringNullSafe();
                                HES_T2 = CTAF2V.ToStringNullSafe();
                                HES_T3 = CTAF3V.ToStringNullSafe();
                                HES_T4 = CTAF4V.ToStringNullSafe();
                                hes = Baseknow.ADV;
                            }
                            var N_SERI = rst_field.N_SERI;
                            var BANK = rst_field.BANK;
                            var BES = rst_field.MABL;
                            var SHARH = Strings.Left(" چك " + rst_field.N_SERI + " بانك " + rst_field.NAMES + " " + rst_field.SHOBEH + " مورخ " + Strings.Format(rst_field.DATE_S, "####/##/##"), 255);

                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES, HES_T2, HES_T3, HES_T4)
                           VALUES({_N_S_},
                           {HES_K},
                           {HES_M} ,
                           {HES_T} ,
                           N'{SHARH}' ,
                           0 ,
                           {BES} ,
                           {N_SERI} ,
                           {BANK} ,
                           N'{hes}' ,
                           {(string.IsNullOrEmpty(HES_T2) ? "NULL" : HES_T2)} ,
                           {(string.IsNullOrEmpty(HES_T3) ? "NULL" : HES_T3)} ,
                           {(string.IsNullOrEmpty(HES_T4) ? "NULL" : HES_T4)} )");

                            CL_HESABDARI.GETTAF3(rst_field.HES1, ref CKOLD, ref CMOIND, ref CTAFD, ref CTAF2D, ref CTAF3D, ref CTAF4D);

                            var SHARH_ = Strings.Left(" چك " + rst_field.N_SERI + " بانك " + rst_field.NAMES + " " + rst_field.SHOBEH + " مورخ " + Strings.Format(rst_field.DATE_S, "####/##/##"), 255);
                            dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES, HES_T2, HES_T3, HES_T4)
                                                 VALUES({NNS},
                                                 {CKOLD},
                                                 {CMOIND} ,
                                                 {CTAFD} ,
                                                 N'{SHARH_}' ,
                                                 {rst_field.MABL} ,
                                                 0 ,
                                                 {rst_field.N_SERI} ,
                                                 {rst_field.BANK} ,
                                                 N'{rst_field.HES1}' ,
                                                 {(string.IsNullOrEmpty(CTAF2D.ToStringNullSafe()) ? "NULL" : CTAF2D)} ,
                                                 {(string.IsNullOrEmpty(CTAF3D.ToStringNullSafe()) ? "NULL" : CTAF3D)} ,
                                                 {(string.IsNullOrEmpty(CTAF4D.ToStringNullSafe()) ? "NULL" : CTAF4D)} )");

                            rst_field.N_S = NNS;
                        }
                    }

                    MyMessages.Add(new MsgModel { MessageText_U = "ثبت در سند شماره :" + NNS + "  شماره مبناي سند : " + NB /*+ "  به مبلغ : " + Strings.Format(Convert.ToInt64(mab), "#,###")*/ });
                }

                if (MyMessages.Any())
                {
                    MyMessages = MyMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, MyMessages).ShowDialog();
                }

                foreach (var row in SFDATAGRID_DATA.OfType<NAZDBANK_D_MODEL>().ToList())
                {
                    foreach (var choosedrow in SelectedRows)
                    {
                        if (row.ID == choosedrow?.ID)
                        {
                            SFDATAGRID_DATA.Remove(row);
                        }
                    }
                }
            }

            //به حساب گذاشتن
            if (BTNCALLED == BTN_BEHESAB) //if (KeyAscii == 70 | KeyAscii == 102 | KeyAscii == 1576)
            {
                var _HES_ = SelectedRows.FirstOrDefault().N_KOL + "-" + SelectedRows.FirstOrDefault().N_MOIN + "-" + SelectedRows.FirstOrDefault().N_TAF;
                new BEHESABDIALOG(I_AM_CHEK_VLISTALL, "CHEK_VLISTALL", _HES_).ShowDialog();

                if (ITEM_SELECTED_VOSUL.DTS_DATE == null)
                {
                    return;
                }

                {
                    var rst = dbms.DoGetDataSQL<CHREC_HES>("SELECT * FROM CHREC_HES WHERE [DATE]= " + ITEM_SELECTED_VOSUL.DTS_DATE).FirstOrDefault();
                    if (rst == null)
                    {
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.CHREC_HES(DATE, MOLAH)
                                             VALUES({ITEM_SELECTED_VOSUL.DTS_DATE}, N'به حساب گذاشته شده توسط گزارش' ) ");
                    }
                }
                {
                    var rst = dbms.DoGetDataSQL<CHRE_LSPH>("SELECT * FROM CHRE_LSPH WHERE N_SERI =" + SelectedRows.FirstOrDefault().N_SERI + " AND BANK = " + SelectedRows.FirstOrDefault().BANK + " AND DATE = " + SelectedRows.FirstOrDefault().DATE_S).FirstOrDefault();
                    if (rst == null)
                    {
                        var _N_SERI_ = SelectedRows.FirstOrDefault().N_SERI;
                        var _BANK_ = SelectedRows.FirstOrDefault().BANK;
                        var _DATE_S_ = SelectedRows.FirstOrDefault().DATE_S;
                        var _DATE_ = ITEM_SELECTED_VOSUL.DTS_DATE;
                        var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                        dbms.DoExecuteSQL($@"INSERT INTO dbo.CHRE_LSPH(N_SERI, BANK, DATE_S, DATE, USER_NAME)
                                             VALUES({_N_SERI_},
                                             {_BANK_} ,
                                             {_DATE_S_} ,
                                             {_DATE_} ,
                                             N'{_USER_NAME_}' )");
                    }
                }
                {
                    string _WHERE_ = " WHERE N_SERI =" + SelectedRows.FirstOrDefault().N_SERI + " AND BANK = " + SelectedRows.FirstOrDefault().BANK + " AND DATE_S = " + SelectedRows.FirstOrDefault().DATE_S;
                    var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD " + _WHERE_).FirstOrDefault();
                    if (rst != null)
                    {
                        var _N_KOL_ = Baseknow.BANKHA;
                        var _N_MOIN_ = CL_HESABDARI.GETMOIN(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                        var _N_TAF_ = CL_HESABDARI.GETTAF(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                        var _hes1_ = ITEM_SELECTED_VOSUL.HHMOIN_VOSUL;
                        var _VAZ_ = 2;

                        dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET
                                             N_KOL = {_N_KOL_} , N_MOIN = {_N_MOIN_} , N_TAF = {_N_TAF_},
                                             HES1 = N'{_hes1_}' , VAZ = {_VAZ_} {_WHERE_}");
                    }
                }
                {
                    var _N_SERI_ = SelectedRows.FirstOrDefault().N_SERI;
                    var _BANK_ = SelectedRows.FirstOrDefault().BANK;
                    var _DATE_S_ = SelectedRows.FirstOrDefault().DATE_S;
                    var _DATE_V_ = CL_HESABDARI.FARSIDATE();
                    var _VAZ_ = 2;
                    var _SANDUGH_ = SelectedRows.FirstOrDefault().SANDUGH;
                    var _USER_NAME_ = CL_HESABDARI.UCurrentUser();

                    dbms.DoExecuteSQL($@"INSERT INTO dbo.PAY_GETD_LOG(N_SERI, BANK, DATE_S, DATE_V, DATETIM, VAZ, SANDUGH, USER_NAME)
                                             VALUES({_N_SERI_}, {_BANK_} , {_DATE_S_} , {_DATE_V_} , GETDATE(), {_VAZ_} , {_SANDUGH_} , N'{_USER_NAME_}' )");
                }

                //Updaete Ui too
                SelectedRows.FirstOrDefault().VAZ = 2;
                SelectedRows.FirstOrDefault().N_KOL = (int?)CL_HESABDARI.GETKOL(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                SelectedRows.FirstOrDefault().N_MOIN = (int?)CL_HESABDARI.GETMOIN(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                SelectedRows.FirstOrDefault().N_TAF = (int?)CL_HESABDARI.GETTAF(ITEM_SELECTED_VOSUL.HHMOIN_VOSUL);
                SelectedRows.FirstOrDefault().HES1 = ITEM_SELECTED_VOSUL.HHMOIN_VOSUL;
                SFDATAGRID_SUB.View.Refresh();

                universControl.PopNotifyShowUp("چک به حساب مورد نظر واگذار شد.", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Green);
            }

        }

        private void BTN_SINGLE_PASS_CHECK_Click(object sender, RoutedEventArgs e)
        {
            //اعلام وصول چک
            DCheckManagement(BTN_SINGLE_PASS_CHECK);
        }
        private void BTN_TRANSFER_SANDOGH_Click(object sender, RoutedEventArgs e)
        {
            // تغيير صندوق
            DCheckManagement(BTN_TRANSFER_SANDOGH);
        }
        private void BTN_GROUP_PASS_Click(object sender, RoutedEventArgs e)
        {
            //وصولي گروهي
            DCheckManagement(BTN_GROUP_PASS);
        }
        private void BTN_BEHESAB_Click(object sender, RoutedEventArgs e)
        {
            //به حساب گذاشتن
            DCheckManagement(BTN_BEHESAB);
        }
    }
}
