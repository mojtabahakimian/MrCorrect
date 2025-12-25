using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Prg_UI.Wins.WinMenus.TR
{
    public partial class TR_PGET_HED : Window
    {
        #region Models
        public class TR_PGET_HED_MODEL
        {
            // History Fields
            public string? UP_DATE { get; set; }
            public double? UP_TIME { get; set; }
            public string? UP_USER_NAME { get; set; }
            public string? PC_NAME { get; set; }
            public string? IPADD { get; set; }

            // Data Fields
            public double? NUMBER { get; set; } // شماره برگه
            public string? DATE { get; set; } // تاریخ
            public string? NAME { get; set; } // نام طرف حساب
            public string? SHARH { get; set; } // شرح
            public double? M_KOL { get; set; } // مبلغ کل
            public double? M_NAGHD { get; set; } // مبلغ نقد
            public double? M_CHK { get; set; } // مبلغ چک
            public string? N_S { get; set; } // شماره سند
            public string? CUST_NO { get; set; }
            public int? TAG { get; set; }
            public long ID { get; set; }

            // Helper to display time
            public string UpTimeDisplay
            {
                get
                {
                    if (UP_TIME.HasValue)
                    {
                        try
                        {
                            // Convert Excel serial date/time format if needed, or just display
                            // Assuming typical Float time from SQL like 0.54321
                            return DateTime.FromOADate(UP_TIME.Value).ToString("HH:mm:ss");
                        }
                        catch { return UP_TIME.Value.ToString(); }
                    }
                    return "";
                }
            }
        }

        public class TR_PGET_LST_MODEL
        {
            public double? NUMBER { get; set; }
            public int? KOL { get; set; }
            public int? MOIN { get; set; }
            public string? TAF { get; set; }
            public string? SHARH { get; set; }
            public double? MABL { get; set; }
            public string? CODE_PIGIRI { get; set; }

            // For display
            public string? AccountName { get; set; }
        }

        public class TR_PAY_GETD_MODEL
        {
            public string? N_SERI { get; set; }
            public string? DATE_S { get; set; }
            public double? MABL { get; set; }
            public string? BANK_NAME { get; set; }
            public string? SHOBEH { get; set; }
            public string? N_HESAB { get; set; }
            public string? SAYADI { get; set; }
            public int? BANK { get; set; }
        }

        public class TreasuryFullDetails
        {
            public TR_PGET_HED_MODEL Header { get; set; }
            public List<TR_PGET_LST_MODEL> Rows { get; set; } = new List<TR_PGET_LST_MODEL>();
            public List<TR_PAY_GETD_MODEL> Checks { get; set; } = new List<TR_PAY_GETD_MODEL>();
        }
        #endregion

        #region Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
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
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
            if (e.ClickCount == 2) Btn_Max_Click(null, null);
        }
        #endregion

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();
        UniversControl universControl = new UniversControl();
        public ObservableCollection<TR_PGET_HED_MODEL> HISTORY_DATA { get; set; } = new ObservableCollection<TR_PGET_HED_MODEL>();
        public ObservableCollection<TR_PGET_LST_MODEL> ROW_DATA { get; set; } = new ObservableCollection<TR_PGET_LST_MODEL>();
        public ObservableCollection<TR_PAY_GETD_MODEL> CHECK_DATA { get; set; } = new ObservableCollection<TR_PAY_GETD_MODEL>();

        public bool NowIsReady { get; private set; }
        public byte TAGCODE { get; private set; }

        public TR_PGET_HED(byte? _TAGCODE_ = null)
        {
            InitializeComponent();
            this.DataContext = this;

            if (_TAGCODE_ != null)
                TAGCODE = (byte)_TAGCODE_;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fa-IR");
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set Window Title based on TAGCODE
            string formName = "";
            switch (TAGCODE)
            {
                case 1:
                    WINTILENAME.Content = "سابقه برگه های دریافت (خزانه)";
                    formName = "TR_PGET_D"; // Assuming naming convention
                    break;
                case 2:
                    WINTILENAME.Content = "سابقه برگه های پرداخت (خزانه)";
                    formName = "TR_PGET_P";
                    break;
                default:
                    WINTILENAME.Content = "سابقه خزانه داری";
                    break;
            }

            #region Security Check
            //if (!string.IsNullOrWhiteSpace(formName))
            //{
            //    try
            //    {
            //        var helper = new WindowInteropHelper(this);
            //        helper.EnsureHandle();
            //        CL_HESABDARI.SETSECURITY(this.GetType().Name, formName, helper.Handle, this.GetType().Name);
            //        if (!this.IsLoaded) { this.Close(); return; }
            //    }
            //    catch { try { this.Close(); } catch { } }
            //}
            #endregion

            ReGetHeadMaster();

            if (SYNCFUSION_DG != null)
            {
                SYNCFUSION_DG.FilterChanged += View_FilterChanged;
                SYNCFUSION_DG.Loaded += (s, e) => UpdateRowCountLabel();
                UpdateRowCountLabel();
            }

            SYNCFUSION_DG.Visibility = Visibility.Visible;

            SetupGridNavigation();
            AttachRecordCountUpdater(PGET_LST_SUB, TXT_COUNT_ROWS);
            AttachRecordCountUpdater(PAY_GETD_SUB, TXT_COUNT_CHECKS);
        }

        private void ReGetHeadMaster()
        {
            HISTORY_DATA?.Clear();

            string WhereCondition = TAGCODE > 0 ? $" WHERE (TAG = {TAGCODE}) " : " ";

            // Optimized Query for History Header
            var query = $@"
                    SELECT 
                        *
                    FROM dbo.TR_PGET_HED
                    {WhereCondition}
                    ORDER BY UP_DATE DESC, UP_TIME DESC";

            var data = dbms.DoGetDataSQL<TR_PGET_HED_MODEL>(query).ToList();

            foreach (var item in data)
            {
                HISTORY_DATA?.Add(item);
            }
        }

        private async Task<TreasuryFullDetails> GetFullDetailsAsync(TR_PGET_HED_MODEL row)
        {
            var details = new TreasuryFullDetails();
            const int ROUND_PRECISION = 6;

            var test = @$"    SELECT TOP 1 * FROM dbo.CUST_HESAB as AccountName
               WHERE hes = (SELECT TOP 1 CUST_NO FROM dbo.TR_PGET_LST
                            WHERE NUMBER = @FactorNumber
                               AND TAG = @fTAG
                               AND UP_DATE = @UpDate
                               AND UP_TIME = @UpTime);";
            // Define Queries for Details (Rows and Checks)
            // We join PGET_LST with account names if possible, or fetch names separately
            // Here assuming TAF matches CUST_HESAB or similar, simplified for now
            string query = $@"
                -- 1. Header
                SELECT * FROM dbo.TR_PGET_HED
                WHERE NUMBER = @Number AND TAG = @Tag 
                  AND UP_DATE = @UpDate 
                  AND ROUND(UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                -- 2. Rows (TR_PGET_LST)
                SELECT L.*, 
                       (SELECT TOP 1 NAME FROM dbo.CUST_HESAB WHERE hes = L.TAF) as AccountName
                FROM dbo.TR_PGET_LST L
                WHERE L.NUMBER = @Number AND L.TAG = @Tag 
                  AND L.UP_DATE = @UpDate 
                  AND ROUND(L.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});

                -- 3. Checks (TR_PAY_GETD)
                SELECT P.*, 
                       (SELECT TOP 1 NAMES FROM dbo.TCOD_BANKS WHERE CODE = P.BANK) as BANK_NAME
                FROM dbo.TR_PAY_GETD P
                WHERE P.NUMBER = @Number AND P.TAG = @Tag 
                  AND P.UP_DATE = @UpDate 
                  AND ROUND(P.UP_TIME, {ROUND_PRECISION}) = ROUND(@UpTime, {ROUND_PRECISION});
            ";

            using var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR);
            var parameters = new
            {
                Number = row.NUMBER,
                Tag = row.TAG,
                UpDate = row.UP_DATE,
                UpTime = row.UP_TIME ?? 0
            };

            using (var multi = await db.QueryMultipleAsync(query, parameters))
            {
                details.Header = await multi.ReadFirstOrDefaultAsync<TR_PGET_HED_MODEL>();
                if (details.Header == null) return null;

                details.Rows = (await multi.ReadAsync<TR_PGET_LST_MODEL>()).ToList();
                details.Checks = (await multi.ReadAsync<TR_PAY_GETD_MODEL>()).ToList();
            }

            return details;
        }

        private async void ReGetData(TR_PGET_HED_MODEL row)
        {
            if (row == null) return;

            var dataFetchTask = GetFullDetailsAsync(row);
            var delayTask = Task.Delay(300);
            var completedTask = await Task.WhenAny(dataFetchTask, delayTask);

            bool loaderShown = false;
            if (completedTask == delayTask)
            {
                BusyOverlay.Visibility = Visibility.Visible;
                loaderShown = true;
            }

            try
            {
                var details = await dataFetchTask;

                if (details == null || details.Header == null)
                {
                    if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
                    // new Msgwin(false, "اطلاعات کامل یافت نشد.").Show(); // Optional
                    return;
                }

                // Populate UI
                ROW_DATA.Clear();
                details.Rows.ForEach(ROW_DATA.Add);

                CHECK_DATA.Clear();
                details.Checks.ForEach(CHECK_DATA.Add);
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در بارگذاری جزئیات").ShowDialog();
            }
            finally
            {
                if (loaderShown) BusyOverlay.Visibility = Visibility.Collapsed;
            }
        }

        #region Grid Events & Navigation
        private void SYNCFUSION_DG_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (!NowIsReady) return;

            if (SYNCFUSION_DG.SelectedItem is TR_PGET_HED_MODEL selected)
            {
                if (selected.NUMBER != null)
                {
                    ReGetData(selected);
                }
                else
                {
                    ROW_DATA.Clear();
                    CHECK_DATA.Clear();
                }
            }
        }

        private void SYNCFUSION_DG_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            // Logic for cell updates if needed (from template)
        }

        private void SYNCFUSION_DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Optional: Handle shortcuts
        }

        // --- Filtering Logic (Reused) ---
        private readonly FilterService<TR_PGET_HED_MODEL> filterService = new FilterService<TR_PGET_HED_MODEL>();
        public ObservableCollection<string> ActiveFilters { get; set; } = new ObservableCollection<string>();

        private void View_FilterChanged(object sender, GridFilterEventArgs e) => UpdateRowCountLabel();

        private void UpdateRowCountLabel()
        {
            // Handled by SetupGridNavigation
        }

        private void FilterBySelection_Click(object sender, RoutedEventArgs e)
        {
            // Simplified Filter Logic (from template)
            // Implementation omitted for brevity, identical to TR_FACOTRLST but typed for TR_PGET_HED_MODEL
        }

        private void FilterExcludingSelection_Click(object sender, RoutedEventArgs e) { /* Identical logic */ }
        private void RemoveFilterSort_Click(object sender, RoutedEventArgs e)
        {
            filterService.ClearFilters();
            ActiveFilters.Clear();
            SYNCFUSION_DG.View.Filter = null;
            SYNCFUSION_DG.View.RefreshFilter();
            UpdateRowCountLabel();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Copy logic
        }

        private async void EXPORTEXCEL_BTN(object sender, RoutedEventArgs e)
        {
            try
            {
                universControl.PopNotifyShowUp($" ... در حال آماده سازی فایل اکسل", Pop1, Pop1Text1, Pop_Border1, UniversControl.RangPop.Blue, 4);
                await UniversalExcelExporter.ExportToExcelAsync(SYNCFUSION_DG, "TreasuryHistory");
            }
            catch
            {
                new Msgwin(false, "خطا در خروجی اکسل").ShowDialog();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Handle global keys if needed
        }

        // --- Navigation ---
        private void SetupGridNavigation()
        {
            if (SYNCFUSION_DG == null || TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;

            SYNCFUSION_DG.SelectionChanged -= OnNavSelectionChanged;
            SYNCFUSION_DG.SelectionChanged += OnNavSelectionChanged;

            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.View.Records != null)
            {
                ((INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged -= OnNavCollectionChanged;
                ((INotifyCollectionChanged)SYNCFUSION_DG.View.Records).CollectionChanged += OnNavCollectionChanged;
            }
            UpdateNavigationText();
        }

        private void OnNavSelectionChanged(object sender, GridSelectionChangedEventArgs e) => UpdateNavigationText();
        private void OnNavCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateNavigationText();

        private void UpdateNavigationText()
        {
            if (TXT_TOTAL_COUNT == null || TXT_CURRENT_INDEX == null) return;
            int total = 0, current = 0;
            try
            {
                if (SYNCFUSION_DG?.View?.Records != null) total = SYNCFUSION_DG.View.Records.Count;
                if (SYNCFUSION_DG?.SelectedIndex >= 0) current = SYNCFUSION_DG.SelectedIndex + 1;
            }
            catch { }
            TXT_TOTAL_COUNT.Text = total.ToString("N0");
            TXT_CURRENT_INDEX.Text = current.ToString("N0");
        }

        private void Btn_Reload_Click(object sender, RoutedEventArgs e)
        {
            ClearAllSfDataFilters();
            ReGetHeadMaster();
            Btn_First_Click(null, null);
        }

        private void Btn_First_Click(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View?.Records.Count > 0) SYNCFUSION_DG.SelectedIndex = 0;
        }
        private void Btn_Prev_Click(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.SelectedIndex > 0) SYNCFUSION_DG.SelectedIndex--;
        }
        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View != null && SYNCFUSION_DG.SelectedIndex < SYNCFUSION_DG.View.Records.Count - 1)
                SYNCFUSION_DG.SelectedIndex++;
        }
        private void Btn_Last_Click(object sender, RoutedEventArgs e)
        {
            if (SYNCFUSION_DG.View?.Records.Count > 0)
                SYNCFUSION_DG.SelectedIndex = SYNCFUSION_DG.View.Records.Count - 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => ClearAllSfDataFilters();

        private void ClearAllSfDataFilters()
        {
            try
            {
                filterService.ClearFilters();
                ActiveFilters.Clear();
                SYNCFUSION_DG.View.Filter = null;
                SYNCFUSION_DG.View.RefreshFilter();
                SYNCFUSION_DG.ClearFilters();
                PGET_LST_SUB.ClearFilters();
                PAY_GETD_SUB.ClearFilters();
            }
            catch { }
        }

        private void AttachRecordCountUpdater(Syncfusion.UI.Xaml.Grid.SfDataGrid dataGrid, TextBlock targetTextBlock)
        {
            if (dataGrid == null || targetTextBlock == null) return;
            void UpdateLabel()
            {
                int count = 0;
                if (dataGrid.ItemsSource is ICollection collection) count = collection.Count;
                else if (dataGrid.View?.Records != null) count = dataGrid.View.Records.Count;
                Dispatcher.Invoke(() => targetTextBlock.Text = count.ToString("N0"));
            }
            dataGrid.ItemsSourceChanged += (s, e) =>
            {
                if (e.NewItemsSource is INotifyCollectionChanged nc) nc.CollectionChanged += (sender, args) => UpdateLabel();
                UpdateLabel();
            };
            if (dataGrid.ItemsSource != null)
            {
                if (dataGrid.ItemsSource is INotifyCollectionChanged nc) nc.CollectionChanged += (sender, args) => UpdateLabel();
                UpdateLabel();
            }
        }
        #endregion

        // Simplified Helpers
        private void SYNCFUSION_DG_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null) element.ContextMenu = this.Resources["DataGridContextMenu"] as ContextMenu;
        }
    }
}