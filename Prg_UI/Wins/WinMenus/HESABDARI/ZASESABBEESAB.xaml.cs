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
            if (!HeaderIsValid())
            {
                return;
            }

            Msgwin msgwin = new Msgwin(true, "آیا از اجرای جایگذاری حساب مطمئن هستید ؟"); msgwin.ShowDialog();
            if (msgwin.DialogResult == false)
            {
                return;
            }

            _ = AuditLogger.LogActionAsync(
                actionType: "REPLACE HESAB",
                tableName: "جایگزینی حساب",
                recordId: null,
                oldValue: $"OLD HESAB : {AZHES.Text}",
                newValue: $"NEW HESAB : {TOHES.Text}",
                additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

            //Let's Go ................................
            Process Prc = ProcLoader.Start();

            double? KOL = null, MOIN = null, taf = null;
            double? TAF2 = null;
            double? taf3 = null;
            double? taf4 = null;

            //به حساب
            CL_HESABDARI.GETTAF3(TOHES.Text, ref KOL, ref MOIN, ref taf, ref TAF2, ref taf3, ref taf4);

            HKOL = string.IsNullOrEmpty(KOL?.ToStringNullSafe()) ? "NULL" : KOL?.ToStringNullSafe();
            HMOIN = string.IsNullOrEmpty(MOIN?.ToStringNullSafe()) ? "NULL" : MOIN?.ToStringNullSafe();
            HTAF = string.IsNullOrEmpty(taf?.ToStringNullSafe()) ? "NULL" : taf?.ToStringNullSafe();
            HTAF2 = string.IsNullOrEmpty(TAF2?.ToStringNullSafe()) ? "NULL" : TAF2?.ToStringNullSafe();
            HTAF3 = string.IsNullOrEmpty(taf3?.ToStringNullSafe()) ? "NULL" : taf3?.ToStringNullSafe();
            HTAF4 = string.IsNullOrEmpty(taf4?.ToStringNullSafe()) ? "NULL" : taf4?.ToStringNullSafe();


            if (IsFuzzyNull(SNDNUM1.Text))
            {
                SNDNUM1.Text = "0";
            }
            if (IsFuzzyNull(SNDNUM2.Text))
            {
                SNDNUM2.Text = "99999999999999999999";
            }

            MyMessages = new List<MsgModel>();


            using (SqlConnection db = new SqlConnection(CL_CCNNMANAGER.CONNECTION_STR))
            {
                db.Open();
                using (var transaction = db.BeginTransaction(IsolationLevel.Serializable))
                {
                    //Effectless Query to Lock Table:
                    //db.Execute("UPDATE TOP(1) HEAD_LST SET MOLAH = MOLAH", null, transaction);

                    try
                    {

                        {
                            var rst = db.Query<double?>("SELECT N_S   FROM dbo.DEED_DTL WHERE     (HES = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction).ToList();
                            if (rst.Count > 0)
                            {
                                MyMessages.Add(new MsgModel { MessageText_U = "تعداد سطر : " + rst.Count });

                                db.Execute("UPDATE    DEED_DTL SET   HES = '" + TOHES.Text + "', HES_K = " + HKOL + ", HES_M = " + HMOIN + "  , HES_T = " + HTAF + " , HES_T2 = " + Interaction.IIf(IsFuzzyNull(HTAF2), "NULL", HTAF2) + ", HES_T3 =  " + Interaction.IIf(IsFuzzyNull(HTAF3), "NULL", HTAF3) + ", HES_T4 = " + Interaction.IIf(IsFuzzyNull(HTAF4), "NULL", HTAF4) + "  FROM dbo.DEED_DTL WHERE     (HES = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction);
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT ID   FROM dbo.PGET_LST WHERE     (FHES = N'" + AZHES.Text + "') OR (THES = N'" + AZHES.Text + "') and id in (select id from pget_hed where date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + " and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PGET_LST SET   FHES = '" + TOHES.Text + "', FHES_K = " + HKOL + ", FHES_M = " + HMOIN + "  , FHES_T = " + HTAF + " , FHES_T2 = " + Interaction.IIf(IsFuzzyNull(HTAF2), "NULL", HTAF2) + ", FHES_T3 =  " + Interaction.IIf(IsFuzzyNull(HTAF3), "NULL", HTAF3) + ", FHES_T4 = " + Interaction.IIf(IsFuzzyNull(HTAF4), "NULL", HTAF4) + "  FROM dbo.PGET_LST WHERE     (FHES = N'" + AZHES.Text + "') and id in (select id from pget_hed where date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + " and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text + ")", null, transaction);

                                db.Execute("UPDATE    PGET_LST SET   THES = '" + TOHES.Text + "', THES_K = " + HKOL + ", THES_M = " + HMOIN + "  , THES_T = " + HTAF + " , THES_T2 = " + Interaction.IIf(IsFuzzyNull(HTAF2), "NULL", HTAF2) + ", THES_T3 =  " + Interaction.IIf(IsFuzzyNull(HTAF3), "NULL", HTAF3) + ", THES_T4 = " + Interaction.IIf(IsFuzzyNull(HTAF4), "NULL", HTAF4) + "  FROM dbo.PGET_LST WHERE     (THES = N'" + AZHES.Text + "') and id in (select id from pget_hed where date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + " and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در اسناد حسابداري انجام شد" });
                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در خزانه داري  انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT NUMBER   FROM dbo.HEAD_LST WHERE     (CUST_NO = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    HEAD_LST SET   CUST_NO = '" + TOHES.Text + "' FROM dbo.HEAD_LST WHERE     (CUST_NO = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction);
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT NUMBER   FROM dbo.HEAD_LST WHERE     (MOIN_VAR = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    HEAD_LST SET    MOIN_VAR = '" + TOHES.Text + "' FROM dbo.HEAD_LST WHERE     ( MOIN_VAR = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در حساب واريزي فاکتورها انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT NUMBER   FROM dbo.HEAD_LST WHERE     (MOIN_HAV = N'" + AZHES.Text + "') and n_s in  (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    HEAD_LST SET    MOIN_HAV = '" + TOHES.Text + "' FROM dbo.HEAD_LST WHERE     ( MOIN_HAV = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در حساب حواله واريزي فاکتورها انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT NUMBER   FROM dbo.HEAD_LST WHERE     (MOIN_HAZ = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    HEAD_LST SET    MOIN_HAZ = '" + TOHES.Text + "' FROM dbo.HEAD_LST WHERE     ( MOIN_HAZ = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در حساب هزينه فاکتورها انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT NUMBER   FROM dbo.HEAD_LST WHERE     (HMBAA = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    HEAD_LST SET    HMBAA = '" + TOHES.Text + "' FROM dbo.HEAD_LST WHERE     ( HMBAA = N'" + AZHES.Text + "') and n_s in (select n_s from deed_hed where date_s between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and  n_s between " + SNDNUM1.Text + " and  " + SNDNUM2.Text, null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در حساب ماليات بر ارزش افزوده فاکتورها انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT NUMBER   FROM dbo.HEAD_LST WHERE    (CUST_NO = N'" + AZHES.Text + "') and  (date_n between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    HEAD_LST SET    CUST_NO = '" + TOHES.Text + "' FROM dbo.HEAD_LST WHERE  (CUST_NO = N'" + AZHES.Text + "') and  (date_n between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در حساب  فاکتورها انجام شد" });
                            }
                        }

                        #region CHECKHA
                        {
                            var rst = db.Query<double?>("SELECT N_SERI   FROM dbo.PAY_GETD WHERE     (HES1 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PAY_GETD SET   HES1 = '" + TOHES.Text + "', N_KOL = " + HKOL + ", N_MOIN = " + HMOIN + "  , N_TAF = " + HTAF + "   FROM dbo.PAY_GETD WHERE     (HES1 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در واگذار به حساب چکهاي دريافتي انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT N_SERI   FROM dbo.PAY_GETD WHERE     (HES2 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PAY_GETD SET   HES2 = '" + TOHES.Text + "', N_KOL2 = " + HKOL + ", N_MOIN2 = " + HMOIN + "  , N_TAF2 = " + HTAF + "   FROM dbo.PAY_GETD WHERE     (HES2 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در برگشت به حساب چکهاي دريافتي انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT N_SERI   FROM dbo.PAY_GETD WHERE     (HES3 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PAY_GETD SET   HES3 = '" + TOHES.Text + "', N_KOL3 = " + HKOL + ", N_MOIN3 = " + HMOIN + "  , N_TAF3 = " + HTAF + "   FROM dbo.PAY_GETD WHERE     (HES3 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در وصول به حساب چکهاي دريافتي انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT N_SERI   FROM dbo.PAY_GETP WHERE     (HES1 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ") and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PAY_GETP SET   HES1 = '" + TOHES.Text + "', N_KOL = " + HKOL + ", N_MOIN = " + HMOIN + "  , N_TAF = " + HTAF + "   FROM dbo.PAY_GETP WHERE     (HES1 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در پرداخت از حساب چکهاي پرداختي انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT N_SERI   FROM dbo.PAY_GETP WHERE     (HES2 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PAY_GETP SET   HES2 = '" + TOHES.Text + "', N_KOL2 = " + HKOL + ", N_MOIN2 = " + HMOIN + "  , N_TAF2 = " + HTAF + "   FROM dbo.PAY_GETP WHERE     (HES2 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در برگشت به حساب چکهاي پرداختي انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT N_SERI   FROM dbo.PAY_GETP WHERE     (HES3 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();
                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PAY_GETP SET   HES3 = '" + TOHES.Text + "', N_KOL3 = " + HKOL + ", N_MOIN3 = " + HMOIN + "  , N_TAF3 = " + HTAF + "   FROM dbo.PAY_GETP WHERE     (HES3 = N'" + AZHES.Text + "') and (date between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در وصول از حساب چکهاي پرداختي انجام شد" });
                            }
                        }

                        #endregion

                        {
                            var rst = db.Query<double?>("SELECT     dbo.INVO_LST.N_RASID FROM    dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE     (dbo.HEAD_LST.DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")  and   (N_RASID = N'" + AZHES.Text + "')", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    INVO_LST SET   N_RASID = '" + TOHES.Text + " '  FROM  dbo.HEAD_LST INNER JOIN   dbo.INVO_LST ON dbo.HEAD_LST.NUMBER = dbo.INVO_LST.NUMBER AND dbo.HEAD_LST.TAG = dbo.INVO_LST.TAG WHERE     (dbo.HEAD_LST.DATE_N BETWEEN " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")  and   (N_RASID = N'" + AZHES.Text + "')", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در محل مصرف حواله خروج ساير انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT POWNER   FROM dbo.PTAMIRAT WHERE     (POWNER = N'" + AZHES.Text + "') and  (PINDATE between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    PTAMIRAT SET   POWNER = '" + TOHES.Text + " '  FROM dbo.PTAMIRAT WHERE      (POWNER = N'" + AZHES.Text + "') and  (PINDATE between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در نام مالک دستگاه در تعميرات انجام شد" });
                            }
                        }

                        {
                            var rst = db.Query<double?>("SELECT CUST_NO   FROM dbo.VISITOR_DTL WHERE     (CUST_NO = N'" + AZHES.Text + "')", null, transaction).ToList();

                            if (rst.Count > 0)
                            {
                                db.Execute("UPDATE    VISITOR_DTL SET   CUST_NO = '" + TOHES.Text + " '  FROM          dbo.HEAD_LST INNER JOIN    dbo.VISITOR_DTL ON dbo.HEAD_LST.NUMBER = dbo.VISITOR_DTL.NUMBER AND dbo.HEAD_LST.TAG = dbo.VISITOR_DTL.TAG WHERE      (dbo.VISITOR_DTL.CUST_NO = N'" + AZHES.Text + "') and  (DATE_N between " + DT1.Text.ToRawTarikh() + " and " + DT2.Text.ToRawTarikh() + ")", null, transaction);

                                MyMessages.Add(new MsgModel { MessageText_U = "جايگزاري حساب در نام مالک دستگاه در تعميرات انجام شد" });
                            }
                        }

                        //Everything was Success
                        if (MyMessages.Any())
                        {
                            MyMessages = MyMessages.Select(x => x.MessageText_U).Distinct()
                                .Select(message => new MsgModel { MessageText_U = message }).ToList();
                            new MsgListwin(false, MyMessages).ShowDialog();
                        }
                        transaction.Commit();

                    }
                    catch (Exception)
                    {
                        transaction.Rollback();

                        new Msgwin(false, "عملیات جایگذاری حساب با خطا مواجه شد؛ بنابراین، وضعیت به حالت اولیه بازمی‌گردد و هیچ تغییری اعمال نخواهد شد.").ShowDialog();
                    }

                    db?.Close();
                }
            }

            ProcLoader.Stop(Prc);

            universControl.PopNotifyShow("جایگذاری حساب کامل انجام شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");

        }

    }
}
