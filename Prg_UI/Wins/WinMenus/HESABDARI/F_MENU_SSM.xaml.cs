using Dapper;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;


namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    public class SSM_CUST_ANALYS_MODEL
    {
        public long SCAID { get; set; }
        public string CUST_NO { get; set; }
        public long? SCADATE { get; set; }
        public double? SCAJAMF { get; set; }
        public double? SCARASFR { get; set; }
        public double? SCARASTAV { get; set; }
        public double? SCARASPAY { get; set; }
        public double? SCATAFAVOT { get; set; }
        public long? SCAMABNADATE { get; set; }
        public double? SCAMABLA { get; set; }
        public double? SCAMABP { get; set; }
        public DateTime? CTIM { get; set; }
        public int? USERCO { get; set; }
        public int? MONTH { get; set; }
        public string NAM { get; set; }
        public double? TAKHIR { get; set; }
        public double? TAGIL { get; set; }
    }
    public partial class F_MENU_SSM : Window
    {
        public F_MENU_SSM()
        {
            InitializeComponent();

            this.DataContext = this;
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
        public ObservableCollection<SSM_CUST_ANALYS_MODEL> SSM_DATA { get; set; } = new ObservableCollection<SSM_CUST_ANALYS_MODEL>();

        public bool NowIsReady { get; private set; }
        public double? NUMBER_TO_OPEN { get; set; }
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}

            FILL_ALL_COMBOBOXES();
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
        private void FILL_ALL_COMBOBOXES()
        {
            //COMBOHESAB.ItemsSource = dbms.DoGetDataSQL<HESAB_CMB_MODEL>($"SELECT hes, NAME FROM CUST_HESAB ORDER BY hes").ToList();
        }

        private void Command5_Click(object sender, RoutedEventArgs e)
        {
            if (MMO.SelectedItem == null)
            {
                new Msgwin(false, "ماه را انتخاب کنید...").ShowDialog();
                return;
            }

            int month = int.Parse(((ComboBoxItem)MMO.SelectedItem).Tag.ToString());

            // Check if records exist for this month
            var count = dbms.DoGetDataSQL<int>($"SELECT COUNT(*) FROM dbo.SSM_CUST_ANALYS WHERE MONTH = {month}").FirstOrDefault();
            if (count > 0)
            {
                new Msgwin(false, "اول محاسبات قبلی را حذف کنید سپس تایید را بزنید").ShowDialog();
                return;
            }

            int currentMonth = int.Parse(Tarikh.Current_Mah);
            int limit = Baseknow.SSMTBMON ?? 0;

            // Simplified date check logic based on legacy code intent (Month diff check)
            // Legacy: UMonth(FARSIDATE(DATE)) > Me.MMO + SSMTBMON
            // We'll check if current month is strictly greater than selected month + limit
            // Note: This logic assumes within same year or handles wrap around. Legacy code was simple.
            // A more robust check would involve years, but sticking to legacy logic:

            // Assuming we are calculating for the current year.
            if (currentMonth > month + limit)
            {
                try
                {
                    // Get customers with Sales in that month
                    // Legacy: SELECT CUST_NO FROM HEAD_LST WHERE (TAG = 13) AND (dbo.Umonth(DATE_N) = Me.MMO) GROUP BY CUST_NO
                    // Converted to SQL Server compatible
                    // Tag 13 is likely Sales Invoice.
                    var customers = dbms.DoGetDataSQL<string>($"SELECT CUST_NO FROM dbo.HEAD_LST WHERE (TAG = 13) AND (dbo.Umonth(DATE_N) = {month}) GROUP BY CUST_NO").ToList();

                    if (customers.Count > 0)
                    {
                        foreach (var custNo in customers)
                        {
                            HesAnalysis(custNo, month);
                        }
                        ReGetData(month);
                    }
                    else
                    {
                        new Msgwin(false, "برای این ماه فروشی ثبت نشده").ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در محاسبه: " + ex.Message).ShowDialog();
                }
            }
            else
            {
                new Msgwin(false, $"با توجه به تنظیمات برای ماه مورد نظر قابل محاسبه نیست حداقل باید {limit} ماه با تاخیر محاسبه گردد").ShowDialog();
            }
        }
        private void HesAnalysis(string custNo, int month)
        {
            //TODO
        }

        private void ReGetData(int month)
        {
            SSM_DATA.Clear();
            var list = dbms.DoGetDataSQL<SSM_CUST_ANALYS_MODEL>(
                @"SELECT s.*, 
                         dbo.UIIF(s.SCAMABLA, '>=', 0, s.SCAMABLA, 0) AS TAKHIR, 
                         dbo.UIIF(s.SCAMABLA, '<', 0, ABS(s.SCAMABLA), 0) AS TAGIL,
                         c.NAME AS NAM
                  FROM dbo.SSM_CUST_ANALYS s
                  LEFT JOIN dbo.CUST_HESAB c ON s.CUST_NO = c.hes
                  WHERE s.MONTH = " + month + " ORDER BY s.SCAJAMF DESC").ToList();

            foreach (var item in list)
            {
                SSM_DATA.Add(item);
            }
        }

        private void ESLAH_Click(object sender, RoutedEventArgs e)
        {
            if (MMO.SelectedItem == null) return;
            int month = int.Parse(((ComboBoxItem)MMO.SelectedItem).Tag.ToString());

            if (Baseknow.TRANSF == true)
            {
                // Call TR (Transaction Log) - assuming TR is a method in CL_HESABDARI or similar
                // CL_HESABDARI.TR("SSM_CUST_ANALYS", $"(MONTH = {month})", DateTime.Now, 1); 
                // Since TR is not clearly defined in provided files, we might skip or log simply
            }

            // Allow deletion - In WPF, we can just enable a "Delete" button or delete logic.
            // For now, we will just delete all records for this month to allow recalculation.
            if (new Msgwin(true, "آیا می خواهید محاسبات این ماه را حذف کنید؟").ShowDialog() == true)
            {
                dbms.DoExecuteSQL($"DELETE FROM SSM_CUST_ANALYS WHERE MONTH = {month}");
                ReGetData(month);
            }
        }

        private void Command24_Click(object sender, RoutedEventArgs e)
        {
            if (MMO.SelectedItem == null) return;
            int month = int.Parse(((ComboBoxItem)MMO.SelectedItem).Tag.ToString());

            try
            {
                // Fetch eligible records
                // ((Month = Me.MMO) AND (SCATAFAVOT > SSMTRTAKM)) OR ((MONTH = Me.MMO) AND (SCATAFAVOT < SSMTRTAGM * -1))
                int takm = Baseknow.SSMTRTAKM ?? 0;
                int tagm = Baseknow.SSMTRTAGM ?? 0;

                string sql = $@"SELECT * FROM SSM_CUST_ANALYS 
                                WHERE (MONTH = {month} AND SCATAFAVOT > {takm}) 
                                   OR (MONTH = {month} AND SCATAFAVOT < {tagm * -1})";

                var list = dbms.DoGetDataSQL<SSM_CUST_ANALYS_MODEL>(sql).ToList();

                if (list.Count > 0)
                {
                    long snd = CL_HESABDARI.Createsanad(
                        long.Parse($"{Baseknow.YEA}{month:00}30"),
                        $"تاخير و تعجيل در پرداخت مشتريان در ماه {month}",
                        0, // GHATEI = False
                        0, // NO_S (auto?)
                        1, // OKF = True (Converted to int)
                        Baseknow.UUSER);

                    // Insert details
                    double jamBed = 0;

                    using (var db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
                    {
                        db.Open();
                        foreach (var item in list)
                        {
                            double? kol = 0, moin = 0, taf = 0, taf2 = 0, taf3 = 0, taf4 = 0;
                            CL_HESABDARI.GETTAF3(item.CUST_NO, ref kol, ref moin, ref taf, ref taf2, ref taf3, ref taf4);

                            string sharh = "";
                            double bed = 0, bes = 0;

                            if (item.SCAMABLA > 0)
                            {
                                sharh = $"تاخير پرداخت به مدت {item.SCATAFAVOT} روز راس فاکتورها: {item.SCARASFR} راس پرداختها: {item.SCARASPAY} راس توافق شده: {item.SCARASTAV} تاریخ مبنا:{item.SCAMABNADATE} جمع فاکتورها:{item.SCAJAMF:#,##0} درصد کاهش تخفیف روزانه :{item.SCAMABP}";
                                bed = item.SCAMABLA ?? 0;
                            }
                            else
                            {
                                sharh = $"تعجيل پرداخت به مدت {Math.Abs(item.SCATAFAVOT ?? 0)} روز راس فاکتورها: {item.SCARASFR} راس پرداختها: {item.SCARASPAY} راس توافق شده: {item.SCARASTAV} تاریخ مبنا:{item.SCAMABNADATE} جمع فاکتورها:{item.SCAJAMF:#,##0} درصد افزایش تخفیف روزانه :{item.SCAMABP}";
                                bes = Math.Abs(item.SCAMABLA ?? 0);
                            }

                            string dtlSql = @"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, SHARH, BED, BES)
                                              VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @HES, @SHARH, @BED, @BES)";

                            db.Execute(dtlSql, new
                            {
                                N_S = snd,
                                HES_K = kol ?? 0,
                                HES_M = moin ?? 0,
                                HES_T = taf ?? 0,
                                HES_T2 = taf2 ?? 0,
                                HES_T3 = taf3 ?? 0,
                                HES_T4 = taf4 ?? 0,
                                HES = item.CUST_NO,
                                SHARH = sharh,
                                BED = bed,
                                BES = bes
                            });

                            jamBed += (bed - bes); // Net impact
                        }

                        // Balancing Entry
                        if (!string.IsNullOrEmpty(Baseknow.HDARKASRTAKHF))
                        {
                            double? bkol = 0, bmoin = 0, btaf = 0, btaf2 = 0, btaf3 = 0, btaf4 = 0;
                            CL_HESABDARI.GETTAF3(Baseknow.HDARKASRTAKHF, ref bkol, ref bmoin, ref btaf, ref btaf2, ref btaf3, ref btaf4);

                            double balBed = 0, balBes = 0;
                            string balSharh = "";

                            if (jamBed < 0) // Net Credit -> Need Debit
                            {
                                balSharh = $" تعجيل پرداخت ماه {month}";
                                balBed = Math.Abs(jamBed);
                            }
                            else // Net Debit -> Need Credit
                            {
                                balSharh = $" تاخير پرداخت ماه {month}";
                                balBes = Math.Abs(jamBed);
                            }

                            string balSql = @"INSERT INTO DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, SHARH, BED, BES)
                                              VALUES (@N_S, @HES_K, @HES_M, @HES_T, @HES_T2, @HES_T3, @HES_T4, @HES, @SHARH, @BED, @BES)";

                            db.Execute(balSql, new
                            {
                                N_S = snd,
                                HES_K = bkol ?? 0,
                                HES_M = bmoin ?? 0,
                                HES_T = btaf ?? 0,
                                HES_T2 = btaf2 ?? 0,
                                HES_T3 = btaf3 ?? 0,
                                HES_T4 = btaf4 ?? 0,
                                HES = Baseknow.HDARKASRTAKHF,
                                SHARH = balSharh,
                                BED = balBed,
                                BES = balBes
                            });
                        }
                        else
                        {
                            new Msgwin(false, "حساب کسری/اضافی در تنظیمات مشخص نشده است.").ShowDialog();
                        }
                    }

                    new Msgwin(false, "سند صادر شد").ShowDialog();
                    // Open Document Header - assuming form name
                    // new DEED_HEAD(snd).Show(); 
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در صدور سند: " + ex.Message).ShowDialog();
            }
        }

        private void SSM_CUST_ANALYS_form_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            var item = e.Record as SSM_CUST_ANALYS_MODEL;
            if (item != null && !string.IsNullOrEmpty(item.CUST_NO))
            {
                if (CL_HESABDARI.BLOCKEDMK(item.CUST_NO))
                {
                    new Msgwin(false, "حساب مورد نظر مسدود می باشد!").ShowDialog();
                    return;
                }

                try
                {
                    string tableName = "MOIN" + Baseknow.USERCOD;
                    dbms.DoExecuteSQL($"IF EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}') DROP TABLE {tableName}");

                    // Assuming QDAFTARTAFZIL2_H is a TVF
                    string sql = $@"SELECT N_S, base, DATE_S, HES_K, HES_M, HES_T, HES_T2, SHARH, BED, BES, 
                                    SUM(BED - BES) OVER (ORDER BY DATE_S, BED DESC ROWS UNBOUNDED PRECEDING) AS MAND, 
                                    id, NO_S, N_SERI, BANK, NUMBER, TAG, ARZD, HES_T3, HES_T4, TAFZILN, HES 
                                    INTO dbo.{tableName} 
                                    FROM dbo.QDAFTARTAFZIL2_H(1, 99999999, '{item.CUST_NO}') QDAFTARTAFZIL2_H 
                                    ORDER BY DATE_S, BED DESC";

                    dbms.DoExecuteSQL(sql);

                    // Open Report Window - using generic name as placeholder or known one
                    new R_DAFTAR_MOIN_LIST().ShowDialog();
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در نمایش گزارش: " + ex.Message).ShowDialog();
                }
            }
        }

        private void MMO_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (MMO.SelectedItem != null)
            {
                int month = int.Parse(((ComboBoxItem)MMO.SelectedItem).Tag.ToString());
                ReGetData(month);
            }
        }
    }
}
