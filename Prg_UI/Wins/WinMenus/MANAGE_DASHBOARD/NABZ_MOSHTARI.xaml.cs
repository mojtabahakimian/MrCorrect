using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Wins.WinMenus.Taarif;
using Wins.WinMenus.KHARID_FORUSH.GOZARESHAT;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using static Prg_Proccessy.SQLMODELS.CTABLES;


namespace Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD
{
    public partial class NABZ_MOSHTARI : Window
    {
        private string CUST_NO;
        public NABZ_MOSHTARI(string custNo)
        {
            InitializeComponent();
            CUST_NO = custNo;
        }

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

        private readonly CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();
        public bool NowIsReady { get; private set; }
        public bool ChangeIsHappend { get; private set; }

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

                //TextBox.IsReadOnly = !ican;
                //ComboBox.IsEnabled = ican;
            }
        }
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

            // اگر کلیدی که باعث تغییر داده نمی‌شود فشرده شده، نادیده بگیرید
            var nonDataKeys = new[]
            {
                Key.Enter, Key.Tab, Key.LeftShift, Key.RightShift,
                Key.CapsLock, Key.Left, Key.Right, Key.Up, Key.Down,
                Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Escape, Key.Insert, Key.Home, Key.End,
                Key.PageUp, Key.PageDown
            };
            if (!nonDataKeys.Contains(e.Key))
            {
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused != null && (CL_LMethods.IsInside<TextBoxBase>(focused) || CL_LMethods.IsInside<ComboBox>(focused) || CL_LMethods.IsInside<CheckBox>(focused)))
                {
                    ChangeIsHappend = true;
                }
                else
                {
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is Xceed.Wpf.Toolkit.MaskedTextBox)
                    {
                        ChangeIsHappend = true;
                    }
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCustomerInfo();
            LoadCharts();
            LoadTabsData();
        }
        private void LoadCustomerInfo()
        {
            if (string.IsNullOrEmpty(CUST_NO)) return;

            string customerName = CL_HESABDARI.GETHESNAME(CUST_NO);
            txtCustomerName.Text = $"مشتری: {CUST_NO} - {customerName}";

            int grade = CL_HESABDARI.GetGrade(CUST_NO);
            string gradeName = CL_HESABDARI.GetGradeName(grade);
            txtGrade.Text = $"گرید مشتری: {gradeName}";

            double balance = CL_HESABDARI.GetMhesab(CUST_NO);
            if (balance == 0)
                txtBalance.Text = "مانده حساب: 0 ریال";
            else
                txtBalance.Text = $"مانده حساب: {(balance > 0 ? balance.ToString("#,### ریال بدهکار") : (balance * -1).ToString("#,### ریال بستانکار"))}";

            double pishBalance = CL_HESABDARI.GetMPishf(CUST_NO);
            txtPreInvoiceBalance.Text = $"مانده پیش فاکتورهای تایید شده: {pishBalance:#,###} ریال";

            double havaleBalance = CL_HESABDARI.GetMHavl(CUST_NO);
            txtHavaleBalance.Text = $"جمع حواله های بارگیری نشده: {havaleBalance:#,###} ریال";

            double docBalance = CL_HESABDARI.GetMAsnad(CUST_NO);
            txtDocBalance.Text = $"مانده اسناد پاس نشده: {docBalance:#,###} ریال";


            if (pishBalance == 0)
            {
                txtPreInvoiceBalance.Text = "0 ریال";
            }
            if (havaleBalance == 0)
            {
                txtHavaleBalance.Text = "0 ریال";
            }
            if (docBalance == 0)
            {
                txtDocBalance.Text = "0 ریال";
            }
        }

        private void LoadCharts()
        {
            try
            {
                var query = @"SELECT SUM(INVO_LST.MEGHk) AS MEGHT, dbo.Umonth(HEAD_LST.DATE_N) AS MON, MON.MON AS MN, 
                             SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN) AS MABL_K
                             FROM HEAD_LST 
                             INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG 
                             INNER JOIN STUF_DEF ON INVO_LST.CODE = STUF_DEF.CODE 
                             INNER JOIN TCOD_STUFGROUP ON STUF_DEF.RADAH = TCOD_STUFGROUP.CODE 
                             INNER JOIN TCOD_VAHEDS ON STUF_DEF.VAHED = TCOD_VAHEDS.CODE 
                             RIGHT OUTER JOIN MON ON dbo.Umonth(HEAD_LST.DATE_N) = MON.MON_ID 
                             WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = -1) AND (HEAD_LST.CUST_NO = @CUST_NO)
                             GROUP BY dbo.Umonth(HEAD_LST.DATE_N), MON.MON 
                             ORDER BY dbo.Umonth(HEAD_LST.DATE_N)";

                var data = dbms.DoGetDataSQL<SalesReportByMonth>(query, new { CUST_NO = CUST_NO }).ToList();
                salesChart.DataContext = new { ChartData = data };
            }
            catch (Exception ex)
            {
                // Handle chart loading error gracefully
                // MessageBox.Show("Error loading chart: " + ex.Message);
            }
        }

        private void LoadTabsData()
        {
            // Pre-invoices
            var pishQuery = @"SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR 
                               FROM HEAD_LST 
                               INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG 
                               WHERE (HEAD_LST.TAG = 20) AND (HEAD_LST.TAMIR <> 2) AND (HEAD_LST.TAMIR <> 3) AND (HEAD_LST.SGN1 = 1) and (HEAD_LST.CUST_NO = @CUST_NO) 
                               GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR";
            grade_pishDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_pish_model>(pishQuery, new { CUST_NO = CUST_NO }).ToList();

            // Havaleh
            var havaleQuery = @"SELECT SUM(INVO_LST.MABL_K - INVO_LST.N_MOIN + INVO_LST.IMBAA) AS GMAB, HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR 
                                 FROM HEAD_LST 
                                 INNER JOIN INVO_LST ON HEAD_LST.NUMBER = INVO_LST.NUMBER AND HEAD_LST.TAG = INVO_LST.TAG 
                                 WHERE (HEAD_LST.TAG = 2) AND (HEAD_LST.TAMIR = 0) and (HEAD_LST.CUST_NO = @CUST_NO) 
                                 GROUP BY HEAD_LST.NUMBER, HEAD_LST.DATE_N, HEAD_LST.CUST_NO, HEAD_LST.MOLAH, HEAD_LST.DEPATMAN, HEAD_LST.SHIFT, HEAD_LST.CUST_KIND, HEAD_LST.USER_NAME, HEAD_LST.TAMIR";
            grade_havalaDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_havala_model>(havaleQuery, new { CUST_NO = CUST_NO }).ToList();

            // Returned Checks
            var checkQuery = @"SELECT N_SERI, vaz, BANK, DATE_S, DATE, SHOBEH, MABL, NAME_TAH, N_HESAB, HES1, HES2, HES3, VAZ 
                                FROM PAY_GETD 
                                WHERE (VAZ = 5 OR VAZ = 6) AND (CUST_NO = @CUST_NO)";
            grade_chek_bargashtiDataGrid.ItemsSource = dbms.DoGetDataSQL<grade_chek_bargashti_model>(checkQuery, new { CUST_NO = CUST_NO }).ToList();
        }

        #region Button Handlers

        private void btnValidation_Click(object sender, RoutedEventArgs e)
        {
            // DoCmd.OpenForm "AZAE", , , "HES = '" & Me.OpenArgs & "'", , acDialog
            var azaeWin = new AZAE_WIN();
            // Pass HES filtering if supported or manually filter.
            // For now just open it.
            azaeWin.ShowDialog();
        }

        private void btnPaymentStatus_Click(object sender, RoutedEventArgs e)
        {
            // DoCmd.OpenForm "NABZ_MOSHTARI_PAYMENT", , , "cust_no = '" & Me.OpenArgs & "'", , acDialog
            //new NABZ_MOSHTARI_PAYMENT().ShowDialog();
        }

        private void btnArchive_Click(object sender, RoutedEventArgs e)
        {
            // DoCmd.OpenForm "tasks_company", acFormDS, , , , acDialog, Me.OpenArgs
            new TASKS_COMPANY(CUST_NO).ShowDialog();
        }

        private void btnUnsoldGoods_Click(object sender, RoutedEventArgs e)
        {
            // DoCmd.OpenForm "froosh_naraftah_PERS", acFormDS, , , , acDialog, Me.OpenArgs
            new FROOSH_NARAFTAH_PERS(CUST_NO).ShowDialog();
        }
        #endregion

        // Helper class for Chart
        public class SalesReportByMonth
        {
            public double MEGHT { get; set; }
            public int MON { get; set; }
            public string MN { get; set; }
            public double MABL_K { get; set; }
        }

    }
}
