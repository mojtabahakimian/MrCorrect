using Dapper;
using Functions;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Stimulsoft.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Functions.CL_LMethods;

namespace Wins.WinMenus.HESABDARI
{
    public partial class ZASESABBEESAB : Window
    {
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
        public ZASESABBEESAB()
        {
            InitializeComponent();

            this.DataContext = this;
        }

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
            }
        }

        public bool NowIsReady { get; private set; }
        public Visual I_AM_ZASESABBEESAB { get; private set; }
        ComboBox CHESAB;

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
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_ZASESABBEESAB = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            //CL_HESABDARI.SETSECURITY(this.GetType().Name, "VCHD", new WindowInteropHelper(this).Handle, this.GetType().Name);
            //if (!this.IsLoaded)
            //{
            //    this.Close();
            //    return;
            //}
        }


        private void AZHES_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SearchGetSetHesName(AZHES);
        }
        private void AZHES2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SearchGetSetHesName(AZHES2);
        }

        private void TOHES_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SearchGetSetHesName(TOHES);
        }
        private void TOHES2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            SearchGetSetHesName(TOHES2);
        }

        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();
        private void SearchGetSetHesName(TextBox _THE_TEXTBOX_)
        {
            if (!string.IsNullOrEmpty(_THE_TEXTBOX_.Tag.ToStringNullSafe()))
            {
                if (_THE_TEXTBOX_.Text.Trim().ToStringNullSafe() == _THE_TEXTBOX_.Tag.ToStringNullSafe().Trim()) //It's Diffrent with own last valid value (was)
                {
                    return;
                }
            }

            ComboBox CHESAB = new ComboBox() { IsEditable = true };
            CHESAB.ItemsSource = new List<Custom_CUST_HESAB>();
            CHESAB.DisplayMemberPath = "NAME";
            CHESAB.SelectedValuePath = "hes";

            if (string.IsNullOrEmpty(_THE_TEXTBOX_.Text) || string.IsNullOrWhiteSpace(_THE_TEXTBOX_.Text))
            {
                universControl.PopNotifyShow("مقدار وارد شده خالی است", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            CHESAB.Text = _THE_TEXTBOX_.Text;

            CL_LMethods.GetSearchedValueCustomer(CHESAB, "ZASESABBEESAB", default, dbms, I_AM_ZASESABBEESAB, false);

            if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES) || CHESAB.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
                {
                    HESAB_FROM_SEARCH.FULL_HES = (CHESAB.SelectedItem as Custom_CUST_HESAB).hes; //کد حساب
                    HESAB_FROM_SEARCH.NAME = (CHESAB.SelectedItem as Custom_CUST_HESAB).NAME; //نام حساب
                }

                if (_THE_TEXTBOX_ == AZHES || _THE_TEXTBOX_ == AZHES2)
                {
                    AZHES.Text = HESAB_FROM_SEARCH.FULL_HES; //کد حساب
                    AZHES.Tag = AZHES.Text; //Save as Last valid value

                    AZHES2.Text = HESAB_FROM_SEARCH.NAME; //نام حساب
                    AZHES2.Tag = AZHES2.Text;
                }
                else if (_THE_TEXTBOX_ == TOHES || _THE_TEXTBOX_ == TOHES2)
                {
                    TOHES.Text = HESAB_FROM_SEARCH.FULL_HES; //کد حساب
                    TOHES.Tag = TOHES.Text;

                    TOHES2.Text = HESAB_FROM_SEARCH.NAME; //نام حساب
                    TOHES2.Tag = TOHES2.Text;
                }
            }
            else
            {
                if (_THE_TEXTBOX_ == AZHES || _THE_TEXTBOX_ == AZHES2)
                {
                    AZHES.Text = null;  //کد حساب
                    AZHES2.Text = null; //نام حساب
                }
                else if (_THE_TEXTBOX_ == TOHES || _THE_TEXTBOX_ == TOHES2)
                {
                    TOHES.Text = null;  //کد حساب
                    TOHES2.Text = null; //نام حساب
                }

                universControl.PopNotifyShow("حسابی صحیح انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1);
            }

            CHESAB.Text = null;
            HESAB_FROM_SEARCH.DoClear();
        }
        public bool DATE_IS_VALID(string _DATE_)
        {
            bool Date_Is_Valid = true;

            var DATE = _DATE_.ToRawTarikh();
            string date_n_val = DATE;
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست", Pop1, Pop1Text1, Pop_Border1);
                    Date_Is_Valid = false;
                }
                else
                {
                    //if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    //{
                    //    universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                    //    Date_Is_Valid = false;
                    //}
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                Date_Is_Valid = false;
            }
            return Date_Is_Valid;
        }

        private void DT1_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!NowIsReady) { return; }

            if (!DATE_IS_VALID(DT1.Text))
            {
                DT1.Text = null;
                e.Handled = true; //Cancel Leaving Focus
            }
        }
        private void DT2_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!DATE_IS_VALID(DT2.Text))
            {
                DT2.Text = null;
                e.Handled = true; //Cancel Leaving Focus
            }
        }

        private bool IsFuzzyNull(string _TEXT_)
        {
            if (string.IsNullOrEmpty(_TEXT_) || string.IsNullOrWhiteSpace(_TEXT_))
            {
                return true;
            }

            return false;
        }

        private bool HeaderIsValid(bool _DisplayErrors = true)
        {
            List<MsgModel> ErrosMessages = new List<MsgModel>();

            //تبدیل از حساب
            if (IsFuzzyNull(AZHES.Text) || IsFuzzyNull(AZHES2.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"قسمت تبدیل از حساب خالی است" });
            }
            else
            {
                if (CL_HESABDARI.ISTAF(AZHES.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب \"تبدیل از حساب\" داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
                }
                else
                {
                    if (CL_HESABDARI.BLOCKEDCUST(AZHES.Text))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = " حساب \"تبدیل از حساب\" مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
                    }
                    else
                    {
                        if ((bool)Baseknow.SAGHF || (bool)(Baseknow.SAGHF2))
                        {
                            if (Convert.ToBoolean(CL_HESABDARI.Checketebar(AZHES.Text)) == false)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = "اعتبار حساب \"تبدیل از حساب\" تمام شده است و نمي تواند خريد نمايد...!" });
                            }
                        }
                    }

                    var HesRow = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 hes FROM dbo.CUST_HESAB WHERE hes = @HES", new { HES = AZHES.Text }).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(HesRow))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"کد وارد شده در قسمت از حساب , در سیستم وجود ندارد" });
                    }
                }
            }

            //تبدیل از حساب
            if (IsFuzzyNull(TOHES.Text) || IsFuzzyNull(TOHES2.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"قسمت تبدیل بــه حساب خالی است" });
            }
            else
            {
                if (CL_HESABDARI.ISTAF(TOHES.Text))
                {
                    ErrosMessages.Add(new MsgModel { MessageText_U = "  حساب \"تبدیل بــه حساب\" داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد! فیلد معین مالیات پشت فاکتور" });
                }
                else
                {
                    if (CL_HESABDARI.BLOCKEDCUST(TOHES.Text))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = " حساب \"تبدیل بــه حساب\" مسدود گرديده است لطفا با مديريت مالي تماس بگيريد" });
                    }
                    else
                    {
                        if ((bool)Baseknow.SAGHF || (bool)(Baseknow.SAGHF2))
                        {
                            if (Convert.ToBoolean(CL_HESABDARI.Checketebar(TOHES.Text)) == false)
                            {
                                ErrosMessages.Add(new MsgModel { MessageText_U = "اعتبار حساب \"تبدیل بــه حساب\" تمام شده است و نمي تواند خريد نمايد...!" });
                            }
                        }
                    }

                    var HesRow = dbms.DoGetDataSQL<string?>($"SELECT TOP 1 hes FROM dbo.CUST_HESAB WHERE hes = @HES", new { HES = TOHES.Text }).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(HesRow))
                    {
                        ErrosMessages.Add(new MsgModel { MessageText_U = $"کد وارد شده در قسمت بـه حساب , در سیستم وجود ندارد" });
                    }
                }
            }

            if (AZHES.Text.Trim() == TOHES.Text.Trim())
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"حساب مبدا و مقصد نمیتواند یکی باشد" });
            }

            if (!DATE_IS_VALID(DT1.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار از تاریخ صحیح نیست" });
            }

            if (!DATE_IS_VALID(DT2.Text))
            {
                ErrosMessages.Add(new MsgModel { MessageText_U = $"مقدار تا تاریخ صحیح نیست" });
            }

            if (ErrosMessages.Any())
            {
                if (_DisplayErrors)
                {
                    ErrosMessages = ErrosMessages.Select(x => x.MessageText_U).Distinct()
                                 .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, ErrosMessages).ShowDialog();
                }
                return false;
            }


            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        string? HKOL = null;
        string? HMOIN = null;
        string? HTAF = null;
        string? HTAF2 = null;
        string? HTAF3 = null;
        string? HTAF4 = null;

        List<MsgModel> MyMessages;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!HeaderIsValid()) return;

            Msgwin msgwin = new Msgwin(true, "آیا از اجرای جایگذاری حساب مطمئن هستید ؟");
            msgwin.ShowDialog();
            if (msgwin.DialogResult == false) return;

            _ = AuditLogger.LogActionAsync(
                actionType: "REPLACE HESAB",
                tableName: "جایگزینی حساب",
                recordId: null,
                oldValue: $"OLD HESAB : {AZHES.Text}",
                newValue: $"NEW HESAB : {TOHES.Text}",
                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            Process Prc = ProcLoader.Start();

            double? KOL = null, MOIN = null, taf = null, TAF2 = null, taf3 = null, taf4 = null;
            CL_HESABDARI.GETTAF3(TOHES.Text, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);

            if (IsFuzzyNull(SNDNUM1.Text)) SNDNUM1.Text = "0";
            if (IsFuzzyNull(SNDNUM2.Text)) SNDNUM2.Text = "99999999999999999999";

            long.TryParse(DT1.Text.ToRawTarikh(), out long dt1Num);
            long.TryParse(DT2.Text.ToRawTarikh(), out long dt2Num);
            double.TryParse(SNDNUM1.Text, out double snd1);
            double.TryParse(SNDNUM2.Text, out double snd2);

            var parameters = new
            {
                AzHes = AZHES.Text,
                ToHes = TOHES.Text,
                HKol = KOL,
                HMoin = MOIN,
                HTaf = taf,
                HTaf2 = TAF2,
                HTaf3 = taf3,
                HTaf4 = taf4,
                Dt1 = dt1Num,
                Dt2 = dt2Num,
                Snd1 = snd1,
                Snd2 = snd2
            };

            string sqlBatch = @"
            SET NOCOUNT ON;
            BEGIN TRY
                SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
                BEGIN TRAN;
                DECLARE @Results TABLE (Msg NVARCHAR(500));
                DECLARE @Rows INT;
                DECLARE @PgetRows INT = 0;

                -- 1. اسناد حسابداری
                UPDATE dbo.DEED_DTL
                SET HES = @ToHes, HES_K = @HKol, HES_M = @HMoin, HES_T = @HTaf, HES_T2 = @HTaf2, HES_T3 = @HTaf3, HES_T4 = @HTaf4
                WHERE (HES = @AzHes) AND n_s IN (SELECT n_s FROM dbo.deed_hed WHERE date_s BETWEEN @Dt1 AND @Dt2) AND n_s BETWEEN @Snd1 AND @Snd2;
                
                SET @Rows = @@ROWCOUNT;
                IF @Rows > 0 
                BEGIN
                    INSERT INTO @Results (Msg) VALUES (N'تعداد سطر : ' + CAST(@Rows AS NVARCHAR(50)));
                    INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در اسناد حسابداري انجام شد');
                END

                -- 2. خزانه داری (پرداخت)
                UPDATE dbo.PGET_LST
                SET FHES = @ToHes, FHES_K = @HKol, FHES_M = @HMoin, FHES_T = @HTaf, FHES_T2 = @HTaf2, FHES_T3 = @HTaf3, FHES_T4 = @HTaf4
                WHERE (FHES = @AzHes) AND id IN (SELECT id FROM dbo.pget_hed WHERE date BETWEEN @Dt1 AND @Dt2 AND n_s BETWEEN @Snd1 AND @Snd2);

                SET @PgetRows = @PgetRows + @@ROWCOUNT;

                -- 3. خزانه داری (دریافت)
                UPDATE dbo.PGET_LST
                SET THES = @ToHes, THES_K = @HKol, THES_M = @HMoin, THES_T = @HTaf, THES_T2 = @HTaf2, THES_T3 = @HTaf3, THES_T4 = @HTaf4
                WHERE (THES = @AzHes) AND id IN (SELECT id FROM dbo.pget_hed WHERE date BETWEEN @Dt1 AND @Dt2 AND n_s BETWEEN @Snd1 AND @Snd2);
                
                SET @PgetRows = @PgetRows + @@ROWCOUNT;

                IF @PgetRows > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در خزانه داري انجام شد');

                -- 4. سربرگ فاکتور (CUST_NO بر اساس deed_hed)
                UPDATE dbo.HEAD_LST SET CUST_NO = @ToHes
                WHERE (CUST_NO = @AzHes) AND n_s IN (SELECT n_s FROM dbo.deed_hed WHERE date_s BETWEEN @Dt1 AND @Dt2) AND n_s BETWEEN @Snd1 AND @Snd2;

                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در حساب فاکتورها (از طریق سند) انجام شد');

                -- 5. حساب واریزی فاکتورها
                UPDATE dbo.HEAD_LST SET MOIN_VAR = @ToHes
                WHERE (MOIN_VAR = @AzHes) AND n_s IN (SELECT n_s FROM dbo.deed_hed WHERE date_s BETWEEN @Dt1 AND @Dt2) AND n_s BETWEEN @Snd1 AND @Snd2;
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در حساب واريزي فاکتورها انجام شد');

                -- 6. حواله واریزی فاکتورها
                UPDATE dbo.HEAD_LST SET MOIN_HAV = @ToHes
                WHERE (MOIN_HAV = @AzHes) AND n_s IN (SELECT n_s FROM dbo.deed_hed WHERE date_s BETWEEN @Dt1 AND @Dt2) AND n_s BETWEEN @Snd1 AND @Snd2;
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در حساب حواله واريزي فاکتورها انجام شد');

                -- 7. هزینه فاکتورها
                UPDATE dbo.HEAD_LST SET MOIN_HAZ = @ToHes
                WHERE (MOIN_HAZ = @AzHes) AND n_s IN (SELECT n_s FROM dbo.deed_hed WHERE date_s BETWEEN @Dt1 AND @Dt2) AND n_s BETWEEN @Snd1 AND @Snd2;
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در حساب هزينه فاکتورها انجام شد');

                -- 8. ارزش افزوده فاکتورها
                UPDATE dbo.HEAD_LST SET HMBAA = @ToHes
                WHERE (HMBAA = @AzHes) AND n_s IN (SELECT n_s FROM dbo.deed_hed WHERE date_s BETWEEN @Dt1 AND @Dt2) AND n_s BETWEEN @Snd1 AND @Snd2;
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در حساب ماليات بر ارزش افزوده فاکتورها انجام شد');

                -- 9. سربرگ فاکتور/حواله (CUST_NO بر اساس DATE_N)
                -- بعضي فاکتورها/حواله‌ها در HEAD_LST.N_S به سند حسابداري وصل نيستند؛
                -- بنابراين مسير جايگزين DATE_N نبايد به محدوده شماره سند محدود شود.
                UPDATE dbo.HEAD_LST SET CUST_NO = @ToHes
                WHERE (CUST_NO = @AzHes) AND (DATE_N BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در حساب فاکتورها و حواله‌ها انجام شد');

                -- 10. واگذار به حساب (PAY_GETD)
                UPDATE dbo.PAY_GETD SET HES1 = @ToHes, N_KOL = @HKol, N_MOIN = @HMoin, N_TAF = @HTaf
                WHERE (HES1 = @AzHes) AND (date BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در واگذار به حساب چکهاي دريافتي انجام شد');

                -- 11. برگشت به حساب (PAY_GETD)
                UPDATE dbo.PAY_GETD SET HES2 = @ToHes, N_KOL2 = @HKol, N_MOIN2 = @HMoin, N_TAF2 = @HTaf
                WHERE (HES2 = @AzHes) AND (date BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در برگشت به حساب چکهاي دريافتي انجام شد');

                -- 12. وصول به حساب (PAY_GETD)
                UPDATE dbo.PAY_GETD SET HES3 = @ToHes, N_KOL3 = @HKol, N_MOIN3 = @HMoin, N_TAF3 = @HTaf
                WHERE (HES3 = @AzHes) AND (date BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در وصول به حساب چکهاي دريافتي انجام شد');

                -- 13. پرداخت از حساب (PAY_GETP)
                UPDATE dbo.PAY_GETP SET HES1 = @ToHes, N_KOL = @HKol, N_MOIN = @HMoin, N_TAF = @HTaf
                WHERE (HES1 = @AzHes) AND (date BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در پرداخت از حساب چکهاي پرداختي انجام شد');

                -- 14. برگشت به حساب (PAY_GETP)
                UPDATE dbo.PAY_GETP SET HES2 = @ToHes, N_KOL2 = @HKol, N_MOIN2 = @HMoin, N_TAF2 = @HTaf
                WHERE (HES2 = @AzHes) AND (date BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در برگشت به حساب چکهاي پرداختي انجام شد');

                -- 15. وصول از حساب (PAY_GETP)
                UPDATE dbo.PAY_GETP SET HES3 = @ToHes, N_KOL3 = @HKol, N_MOIN3 = @HMoin, N_TAF3 = @HTaf
                WHERE (HES3 = @AzHes) AND (date BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در وصول از حساب چکهاي پرداختي انجام شد');

                -- 16. مصرف حواله خروج
                UPDATE il SET N_RASID = @ToHes
                FROM dbo.HEAD_LST hl INNER JOIN dbo.INVO_LST il ON hl.NUMBER = il.NUMBER AND hl.TAG = il.TAG
                WHERE (hl.DATE_N BETWEEN @Dt1 AND @Dt2) AND (il.N_RASID = @AzHes);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در محل مصرف حواله خروج ساير انجام شد');

                -- 17. مالک دستگاه (PTAMIRAT)
                UPDATE dbo.PTAMIRAT SET POWNER = @ToHes
                WHERE (POWNER = @AzHes) AND (PINDATE BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در نام مالک دستگاه در تعميرات انجام شد');

                -- 18. ویزیتورها (VISITOR_DTL)
                UPDATE vd SET CUST_NO = @ToHes
                FROM dbo.HEAD_LST hl INNER JOIN dbo.VISITOR_DTL vd ON hl.NUMBER = vd.NUMBER AND hl.TAG = vd.TAG
                WHERE (vd.CUST_NO = @AzHes) AND (hl.DATE_N BETWEEN @Dt1 AND @Dt2);
                IF @@ROWCOUNT > 0 INSERT INTO @Results (Msg) VALUES (N'جايگزاري حساب در ویزیتورها انجام شد');

                COMMIT TRAN;
                SELECT Msg FROM @Results;
            END TRY
            BEGIN CATCH
                ROLLBACK TRAN;
                THROW;
            END CATCH
            ";

            bool isSuccess = true;
            try
            {
                // استفاده از توابع مجاز کلاس CL_CCNNMANAGER به جای ایجاد کانکشن ناامن
                var resultMessages = dbms.DoGetDataSQL<string>(sqlBatch, parameters).ToList();

                if (resultMessages.Count > 0)
                {
                    List<MsgModel> MyMessages = resultMessages.Select(m => new MsgModel { MessageText_U = m }).ToList();
                    new MsgListwin(false, MyMessages).ShowDialog();
                }
            }
            catch (Exception)
            {
                isSuccess = false;
                new Msgwin(false, "عملیات جایگذاری حساب با خطا مواجه شد؛ بنابراین، وضعیت به حالت اولیه بازمی‌گردد و هیچ تغییری اعمال نخواهد شد.").ShowDialog();
            }
            finally
            {
                ProcLoader.Stop(Prc);

                if (isSuccess)
                {
                    universControl.PopNotifyShow("جایگذاری حساب کامل انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
                }
            }

        }
    }
}
