using MaterialDesignThemes.Wpf;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    /// <summary>
    /// Interaction logic for WIN_SANAD_EKHTETAMIYAH.xaml
    /// </summary>
    public partial class WIN_SANAD_EKHTETAMIYAH : Window
    {
        public WIN_SANAD_EKHTETAMIYAH()
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

            CL_HESABDARI.SETSECURITY(this.GetType().Name, "EKHTE", new WindowInteropHelper(this).Handle, this.GetType().Name);
            if (!this.IsLoaded)
            {
                this.Close();
                return;
            }

            FILL_ALL_COMBOBOXES();

            // Set default date
            DT.Text = Tarikh.SlashyFullDate;

            // Generate next Voucher Number (N_S)
            var result = dbms.DoGetDataSQL<long?>("SELECT MAX(N_S) AS MaxOfN_S FROM DEED_HED").FirstOrDefault();
            if (result.HasValue)
            {
                S2.Text = (result.Value + 1).ToString();
            }
            else
            {
                S2.Text = "1";
            }
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

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct().Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }

                return false;
            }
            return true;
        }
        private bool DATE_VALIDATE(bool DisplayMsg = true)
        {
            string date_n_val = DT.Text.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    DT.Text = null;
                    if (DisplayMsg)
                    {
                        universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    }
                    return false;
                }
            }
            else
            {
                DT.Text = null;
                if (DisplayMsg)
                {
                    universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                }
                return false;
            }

            return true;
        }
        private void BTN_GO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DATE_VALIDATE())
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(DT.Text) || string.IsNullOrWhiteSpace(S2.Text))
                {
                    new Msgwin(false, "پارامترها کافی نیست!").ShowDialog();
                    return;
                }


                Msgwin msgDup = new Msgwin(true, "آیا در مورد صدور سند اختتامیه اطمینان دارید ؟");
                msgDup.ShowDialog();
                if (msgDup.DialogResult != true) return;

                // Confirmation
                if (true)
                {
                    long n_s = long.Parse(S2.Text);
                    long date_s = long.Parse(DT.Text.ToRawTarikh());
                    string description = new string((SH.Text ?? "").Take(255).ToArray()); // Truncate to 255 chars

                    // Check duplicate
                    var existing = dbms.DoGetDataSQL<long?>($"SELECT N_S FROM DEED_HED WHERE N_S = {n_s}").FirstOrDefault();
                    if (existing.HasValue)
                    {
                        new Msgwin(false, "شماره سند تکراری می باشد.").ShowDialog();
                        return;
                    }

                    // Insert Header
                    dbms.DoExecuteSQL("INSERT INTO DEED_HED (N_S, DATE_S, SHARH_S, NO_S, OKF, USER_NAME) VALUES (@N_S, @DATE_S, @SHARH_S, 0, 1, @USER_NAME)",
                        new { N_S = n_s, DATE_S = date_s, SHARH_S = description, USER_NAME = Baseknow.UUSER });

                    string sql = "";

                    // Insert Details (Balances)
                    // Note: Using a CASE statement to replace the legacy UIIF function.
                    // Assuming [BED-BES] is a view or table available in the database.
                    if (Option29.IsChecked == true) // Option29 = All Accounts (No Join with TOTA_HES needed for filtering by M_D)
                    {
                        sql = @"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, BES, BED, SHARH) 
                                 SELECT @N_S AS N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, 
                                 CASE WHEN BEST > 0 THEN BEST ELSE 0 END AS BES, 
                                 CASE WHEN BEST < 0 THEN BEST * -1 ELSE 0 END AS BED, 
                                 'سند اختتامیه' AS SH 
                                 FROM dbo.[BED-BES] 
                                 WHERE (HES <> @ADA And HES <> @APA) 
                                 AND (HES <> @ADV And HES <> @APV)";
                    }
                    else // Option27 = Permanent Accounts Only (Join TOTA_HES and filter M_D = 1)
                    {
                        sql = @"INSERT INTO dbo.DEED_DTL (N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, BES, BED, SHARH) 
                                 SELECT @N_S AS N_S, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, 
                                 CASE WHEN BEST > 0 THEN BEST ELSE 0 END AS BES, 
                                 CASE WHEN BEST < 0 THEN BEST * -1 ELSE 0 END AS BED, 
                                 'سند اختتامیه' AS SH 
                                 FROM dbo.[BED-BES] INNER JOIN dbo.TOTA_HES ON dbo.[BED-BES].HES_K = dbo.TOTA_HES.NUMBER 
                                 WHERE (HES <> @ADA And HES <> @APA) 
                                 AND (HES <> @ADV And HES <> @APV) 
                                 And (dbo.TOTA_HES.M_D = 1)";
                    }

                    dbms.DoExecuteSQL(sql, new { N_S = n_s, ADA = Baseknow.ADA, APA = Baseknow.APA, ADV = Baseknow.ADV, APV = Baseknow.APV });

                    // --- Commercial Documents (Cheques) ---

                    double? CKOL = null, CMOIN = null, CTAF = null, CTAF2 = null, CTAF3 = null, CTAF4 = null;

                    // 1. Trade Receivables (ADA)
                    CL_HESABDARI.GETTAF3(Baseknow.ADA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    sql = @"INSERT INTO dbo.DEED_DTL (N_SERI, BANK, BES, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, N_S, SHARH) 
                             SELECT dbo.PAY_GETD.N_SERI, dbo.PAY_GETD.BANK, dbo.PAY_GETD.MABL, 
                             @KOL AS KOL, @MOIN AS MOIN, @TAF AS TAF, 
                             @TAF2 AS TAF2, 
                             @TAF3 AS TAF3, 
                             @TAF4 AS TAF4, 
                             @ADA AS Expr2, @NS AS NS, 
                             RIGHT(' چك' + CAST(CAST(dbo.PAY_GETD.N_SERI AS numeric) AS nvarchar(50))+ ' بانك ' + ISNULL(dbo.TCOD_BANKS.NAMES, '  ') + ' ' + ISNULL(dbo.PAY_GETD.SHOBEH, '  ') + ' مورخ ' + CAST(dbo.PAY_GETD.DATE_S AS NVARCHAR),60) AS Expr1 
                             FROM dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETD ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETD.BANK 
                             WHERE (dbo.PAY_GETD.N_KOL IS NULL OR dbo.PAY_GETD.N_KOL = @BANKHA) 
                             AND (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.N_KOL3 IS NULL) 
                             AND (KIND = 1 OR KIND IS NULL)";

                    dbms.DoExecuteSQL(sql, new
                    {
                        KOL = CKOL ?? 0,
                        MOIN = CMOIN ?? 0,
                        TAF = CTAF ?? 0,
                        TAF2 = CTAF2,
                        TAF3 = CTAF3,
                        TAF4 = CTAF4,
                        ADA = Baseknow.ADA,
                        NS = n_s,
                        BANKHA = Baseknow.BANKHA
                    });

                    // 2. Non-Trade Receivables (ADV)
                    CL_HESABDARI.GETTAF3(Baseknow.ADV, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    sql = @"INSERT INTO dbo.DEED_DTL (N_SERI, BANK, BES, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, N_S, SHARH) 
                             SELECT dbo.PAY_GETD.N_SERI, dbo.PAY_GETD.BANK, dbo.PAY_GETD.MABL, 
                             @KOL AS KOL, @MOIN AS MOIN, @TAF AS TAF, 
                             @TAF2 AS TAF2, 
                             @TAF3 AS TAF3, 
                             @TAF4 AS TAF4, 
                             @ADV AS Expr2, @NS AS NS, 
                             RIGHT(' چك' + CAST(CAST(dbo.PAY_GETD.N_SERI AS numeric) AS nvarchar(50))+ ' بانك ' + ISNULL(dbo.TCOD_BANKS.NAMES, '  ') + ' ' + ISNULL(dbo.PAY_GETD.SHOBEH, '  ') + ' مورخ ' + CAST(dbo.PAY_GETD.DATE_S AS NVARCHAR),60) AS Expr1 
                             FROM dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETD ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETD.BANK 
                             WHERE (dbo.PAY_GETD.N_KOL IS NULL OR dbo.PAY_GETD.N_KOL = @BANKHA) 
                             AND (dbo.PAY_GETD.N_KOL2 IS NULL) AND (dbo.PAY_GETD.N_KOL3 IS NULL) 
                             AND kind <> 1";

                    dbms.DoExecuteSQL(sql, new
                    {
                        KOL = CKOL ?? 0,
                        MOIN = CMOIN ?? 0,
                        TAF = CTAF ?? 0,
                        TAF2 = CTAF2,
                        TAF3 = CTAF3,
                        TAF4 = CTAF4,
                        ADV = Baseknow.ADV,
                        NS = n_s,
                        BANKHA = Baseknow.BANKHA
                    });

                    // 3. Trade Payables (APA)
                    CL_HESABDARI.GETTAF3(Baseknow.APA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    sql = @"INSERT INTO dbo.DEED_DTL (N_SERI, BANK, BED, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, N_S, SHARH) 
                             SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK, dbo.PAY_GETP.MABL, 
                             @KOL AS KOL, @MOIN AS MOIN, @TAF AS TAF, 
                             @TAF2 AS TAF2, 
                             @TAF3 AS TAF3, 
                             @TAF4 AS TAF4, 
                             @APA AS Expr2, @NS AS NS, 
                             RIGHT(' چك' + CAST(CAST(dbo.PAY_GETP.N_SERI AS numeric) AS nvarchar(50))+ ' بانك ' + dbo.TCOD_BANKS.NAMES + ' ' + ISNULL(dbo.PAY_GETP.SHOBEH,'  ') + ' مورخ ' + CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR),60) AS Expr1 
                             FROM dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETP ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETP.BANK 
                             WHERE (dbo.PAY_GETP.N_KOL2 IS NULL) AND (dbo.PAY_GETP.N_KOL3 IS NULL) 
                             AND kind = 1 AND dbo.PAY_GETP.N_KOL<> 911";

                    dbms.DoExecuteSQL(sql, new
                    {
                        KOL = CKOL ?? 0,
                        MOIN = CMOIN ?? 0,
                        TAF = CTAF ?? 0,
                        TAF2 = CTAF2,
                        TAF3 = CTAF3,
                        TAF4 = CTAF4,
                        APA = Baseknow.APA,
                        NS = n_s
                    });

                    // 4. Non-Trade Payables (APV)
                    CL_HESABDARI.GETTAF3(Baseknow.APV, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                    sql = @"INSERT INTO dbo.DEED_DTL (N_SERI, BANK, BED, HES_K, HES_M, HES_T, HES_T2, HES_T3, HES_T4, HES, N_S, SHARH) 
                             SELECT dbo.PAY_GETP.N_SERI, dbo.PAY_GETP.BANK, dbo.PAY_GETP.MABL, 
                             @KOL AS KOL, @MOIN AS MOIN, @TAF AS TAF, 
                             @TAF2 AS TAF2, 
                             @TAF3 AS TAF3, 
                             @TAF4 AS TAF4, 
                             @APV AS Expr2, @NS AS NS, 
                             RIGHT(' چك' + CAST(CAST(dbo.PAY_GETP.N_SERI AS numeric) AS nvarchar(50))+ ' بانك ' + dbo.TCOD_BANKS.NAMES + ' ' + ISNULL(dbo.PAY_GETP.SHOBEH, '  ') + ' مورخ ' + CAST(dbo.PAY_GETP.DATE_S AS NVARCHAR),60) AS Expr1 
                             FROM dbo.TCOD_BANKS INNER JOIN dbo.PAY_GETP ON dbo.TCOD_BANKS.CODE = dbo.PAY_GETP.BANK 
                             WHERE (dbo.PAY_GETP.N_KOL2 IS NULL) AND (dbo.PAY_GETP.N_KOL3 IS NULL) 
                             AND kind <> 1 AND dbo.PAY_GETP.N_KOL<> 911";

                    dbms.DoExecuteSQL(sql, new
                    {
                        KOL = CKOL ?? 0,
                        MOIN = CMOIN ?? 0,
                        TAF = CTAF ?? 0,
                        TAF2 = CTAF2,
                        TAF3 = CTAF3,
                        TAF4 = CTAF4,
                        APV = Baseknow.APV,
                        NS = n_s
                    });

                    new Msgwin(false, "سند اختتامیه با موفقیت صادر گردید").ShowDialog();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                new Msgwin(false, "خطا در صدور سند: " + ex.Message).ShowDialog();
            }
        }
    }
}
