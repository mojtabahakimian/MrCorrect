using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic;
using Prg_Proccessy.FUNCTIONS;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using Prg_UI.UiTools;
using Syncfusion.PMML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Prg_Proccessy.SQLMODELS.CTABLES;

namespace Wins.WinMenus.Checkha.NAZDESANDOGH
{
    /// <summary>
    /// Interaction logic for VOSULDIALOG.xaml
    /// </summary>
    public partial class VOSULDIALOG : Window
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
        public VOSULDIALOG(Visual _WIN_, List<NAZDBANK_D_MODEL> _Records_, string _WinName_, string _OpenArgs_, double? n_SERI)
        {
            InitializeComponent();

            this.DataContext = this;

            WIN = _WIN_;
            RowsChecks = _Records_;
            OpenArgs = _OpenArgs_;
            WinName = _WinName_;
            serial.Text = n_SERI.ToStringNullSafe();
        }
        #region LOCALMODEL
        public class VOSUL_MODEL_QRE1
        {
            public long? DATE { get; set; }
            public string? MOLAH { get; set; }
            public double? N_S { get; set; }
            public int? IDH { get; set; }
            public DateTime? CRT { get; set; }
            public int? UID { get; set; }
            public double? N_SERI { get; set; }
            public int? BANK { get; set; }
            public long? DATE_S { get; set; }
            public int? RADIF { get; set; }
            public int? N_MOIN { get; set; }
            public int? N_TAF { get; set; }
            public string? NAMES { get; set; }
            public string? SHOBEH { get; set; }
            public double? MABL { get; set; }
            public int? N_KOL { get; set; }
            public int? KIND { get; set; }
            public string? HES1 { get; set; }
        }

        public Visual WIN { get; }
        #endregion

        List<NAZDBANK_D_MODEL> RowsChecks = new List<NAZDBANK_D_MODEL>();

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool IsAlive { get; set; } = true;

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
                    this.Dispatcher.BeginInvoke(new Action(() => {
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

        public string OpenArgs { get; }
        public string WinName { get; }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CL_HESABDARI.AMALIYAT_USER(this.GetType().Name);

            I_AM_VOSULDIALOG = CL_LMethods.GetTheWindow(new WindowInteropHelper(this).Handle);

            if (WinName == "CHECK_PVLIST")
            {
                HHMOIN.IsEnabled = false;
            }
            else
            {
                HHMOIN.IsEnabled = true;
            }

            HHMOIN.ItemsSource = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME,hes FROM CUST_HESAB WHERE (dbo.GETKOL(HES) = " + Baseknow.BANKHA + ")").ToList();

            if (!string.IsNullOrEmpty(OpenArgs))
            {
                HHMOIN.SelectedValue = OpenArgs;
                HHMOIN.Items.Refresh();
            }

            DTS.Text = Tarikh.FullCurrentDate;
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (BTN_SAVE.IsFocused)
                {
                    return;
                }

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
        }

        private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
        {
            if (!BTN_SAVE.IsEnabled) { return; }

            if (!IS_DATE_VALID(DTS.Text.ToRawTarikh()))
            {
                return;
            }

            int max_ns;
            int NB;
            int NNS;
            string HAPA;
            var mab = default(double);
            double? CKOLV = default, CMOINV = default, CTAFV = default, CTAF2V = default, CTAF3V = default, CTAF4V = default, CKOL = default, CMOIN = default, CTAF = default, CTAF2 = default, CTAF3 = default, CTAF4 = default, CKOLD = default, CMOIND = default, CTAFD = default, CTAF2D = default, CTAF3D = default, CTAF4D = default;

            if (HHMOIN.SelectedValue == null)
            {
                new Msgwin(false, "حسابي كه چك بايد به آن وصول شود مشخص نگرديده است...!").ShowDialog();
                return;
            }
            else if (CL_HESABDARI.ISTAF(HHMOIN.SelectedValue.ToStringNullSafe()))
            {
                new Msgwin(false, "حساب مورد نظر داراي تفضيلي ميباشد بايد تفضيلي آن را انتخاب كنيد!").ShowDialog();
                return;
            }
            else if (string.IsNullOrEmpty(serial.Text) || serial.Text == "0")
            {
                new Msgwin(false, "شماره سریالی انتخاب نشده !").ShowDialog();
                return;
            }
            else
            {
                //var rstfirst = dbms.DoGetDataSQL<double?>("SELECT Max(DEED_HED.N_S) AS MaxOfN_S FROM DEED_HED").FirstOrDefault();
                //if (rstfirst == null || rstfirst == 0)
                //{
                //    max_ns = 1;
                //}
                //else
                //{
                //    max_ns = (int)(rstfirst + 1);
                //}
                //if (WinName != "CHECK_PVLIST") //if (!IsLoaded("CHECK_PVLIST"))
                //{
                //    //if (Val(this.serial) == Forms["CHEK_VLISTall"]["N_SERI"] || this.serial == "")
                //    if (Convert.ToDouble(serial.Text) == RowsChecks?.FirstOrDefault()?.N_SERI)
                //    {
                //        //SHRST.Filter = "NO_S = 6 AND DATE_S = " + DTS.Text.ToRawTarikh();
                //        var SHRST = dbms.DoGetDataSQL<DEED_HED>("SELECT * FROM dbo.DEED_HED WHERE NO_S = 6 AND DATE_S = " + DTS.Text.ToRawTarikh()).FirstOrDefault();
                //        if (SHRST == null) //if (SHRST.RecordCount == 0)
                //        {
                //            //OUTPUT INSERTED.IDD
                //            //INSERT
                //            //SHRST.AddNew();
                //            var _N_S_ = max_ns;
                //            var _DATE_S_ = this.DTS.Text.ToRawTarikh();
                //            var _SHARH_S_ = "اعلام وصول چكهاي دريافتي";
                //            var _GHATEI_ = 0;
                //            var _NO_S_ = 6;
                //            var _USER_NAME_ = CL_HESABDARI.UCurrentUser();
                //            var _UID_ = Baseknow.USERCOD;
                //            //SHRST.update();
                //            //NB = SHRST.Fields("base");
                //            var _QRE_base_ = dbms.DoGetDataSQL<int?>($@"INSERT INTO dbo.DEED_HED(N_S, DATE_S, SHARH_S,GHATEI, NO_S, USER_NAME,UID)
                //                                       OUTPUT INSERTED.base
                //                                       VALUES({_N_S_},
                //                                       {_DATE_S_} ,
                //                                       N'{_SHARH_S_}' ,
                //                                       {_GHATEI_},
                //                                       {_NO_S_} ,
                //                                       N'{_USER_NAME_}' ,
                //                                       {_UID_} )").FirstOrDefault();
                //            NB = (int)_QRE_base_;
                //        }
                //        else
                //        {
                //            max_ns = (int)SHRST.N_S;
                //            NB = SHRST.@base;
                //        }
                //        NNS = max_ns;
                //        {
                //            var rst = dbms.DoGetDataSQL<CHKREC_H>("SELECT * FROM CHKREC_H WHERE [DATE] = " + DTS.Text.ToRawTarikh()).FirstOrDefault();
                //            if (rst == null)
                //            {
                //                //rst.AddNew();
                //                var _DATE_ = DTS.Text.ToRawTarikh();
                //                var _MOLAH_ = "وصولي ثبت شده توسط گزارش";
                //                var _N_S_ = max_ns;
                //                //rst.update();
                //                dbms.DoExecuteSQL($@"INSERT INTO dbo.CHKREC_H(DATE, MOLAH, N_S)
                //                                     VALUES({_DATE_}, N'{_MOLAH_}' , {_N_S_} )");
                //            }
                //        }
                //        {
                //            var rst = dbms.DoGetDataSQL<CHRE_LST>("SELECT * FROM CHRE_LST WHERE N_SERI =" + RowsChecks.FirstOrDefault().N_SERI + " AND BANK = " + RowsChecks.FirstOrDefault().BANK + " AND DATE_S = " + RowsChecks.FirstOrDefault().DATE_S).FirstOrDefault();
                //            if (rst == null)
                //            {
                //                //rst.AddNew();
                //                var _N_SERI_ = RowsChecks.FirstOrDefault().N_SERI;
                //                var _BANK_ = RowsChecks.FirstOrDefault().BANK;
                //                var _DATE_S_ = RowsChecks.FirstOrDefault().DATE_S;
                //                var _DATE_ = DTS.Text.ToRawTarikh();

                //                dbms.DoExecuteSQL($@"INSERT INTO dbo.CHRE_LST(N_SERI, BANK, DATE_S, DATE)
                //                                     VALUES({_N_SERI_},
                //                                     {_BANK_} ,
                //                                     {_DATE_S_} ,
                //                                     {_DATE_} )");
                //                //rst.update();
                //            }
                //        }
                //        {
                //            string _WHERE_ = " WHERE N_SERI =" + RowsChecks.FirstOrDefault().N_SERI + " AND BANK = " + RowsChecks.FirstOrDefault().BANK + " AND DATE_S = " + RowsChecks.FirstOrDefault().DATE_S;
                //            var rst = dbms.DoGetDataSQL<PAY_GETD>("SELECT * FROM PAY_GETD " + _WHERE_).FirstOrDefault();
                //            if (rst != null)
                //            {
                //                var _N_KOL3_ = Baseknow.BANKHA;
                //                var _N_MOIN3_ = CL_HESABDARI.GETMOIN(HHMOIN.SelectedValue.ToStringNullSafe());
                //                var _N_TAF3_ = CL_HESABDARI.GETTAF(HHMOIN.SelectedValue.ToStringNullSafe());
                //                var _N_KOL_ = Baseknow.BANKHA;
                //                var _N_MOIN_ = CL_HESABDARI.GETMOIN(HHMOIN.SelectedValue.ToStringNullSafe());
                //                var _N_TAF_ = CL_HESABDARI.GETTAF(HHMOIN.SelectedValue.ToStringNullSafe());
                //                var _HES1_ = HHMOIN.SelectedValue.ToStringNullSafe();
                //                var _HES3_ = HHMOIN.SelectedValue.ToStringNullSafe();
                //                var _N_S_ = NNS;
                //                //rst.update();
                //                dbms.DoExecuteSQL($@"UPDATE dbo.PAY_GETD SET 
                //                                     N_KOL3 = {_N_KOL3_} , N_MOIN3 = {_N_MOIN3_} , N_TAF3 = {_N_TAF3_} ,  N_KOL = {_N_KOL_},
                //                                     N_MOIN = {_N_MOIN_} , N_TAF = {_N_TAF_} , HES1 = {_HES1_} , HES3 = {_HES3_} , N_S = {_N_S_}
                //                                     WHERE {_WHERE_}");
                //            }
                //        }
                //        CL_HESABDARI.GETDLOG(3, RowsChecks.FirstOrDefault().N_SERI.ToString(), (int)RowsChecks.FirstOrDefault().BANK, (long)RowsChecks.FirstOrDefault().DATE_S, (int)RowsChecks.FirstOrDefault().SANDUGH);
                //        dbms.DoExecuteSQL("DELETE FROM DEED_DTL WHERE (((DEED_DTL.N_S)= " + max_ns + " ))");
                //        if (!string.IsNullOrEmpty(Baseknow.ADA))
                //        {
                //            CL_HESABDARI.GETTAF3(Baseknow.ADA, ref CKOL, ref CMOIN, ref CTAF, ref CTAF2, ref CTAF3, ref CTAF4);
                //        }
                //        if (!string.IsNullOrEmpty(Baseknow.ADV))
                //        {
                //            CL_HESABDARI.GETTAF3(Baseknow.ADV, ref CKOLV, ref CMOINV, ref CTAFV, ref CTAF2V, ref CTAF3V, ref CTAF4V);
                //        }
                //        {
                //            var rst = dbms.DoGetDataSQL<VOSUL_MODEL_QRE1>("SELECT dbo.CHKREC_H.*, dbo.CHRE_LST.*, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETD.SHOBEH,dbo.PAY_GETD.N_S, dbo.PAY_GETD.MABL, dbo.PAY_GETD.N_KOL,  dbo.PAY_GETD.N_MOIN, dbo.PAY_GETD.N_TAF, dbo.PAY_GETD.KIND, dbo.PAY_GETD.HES1    FROM         dbo.CHKREC_H INNER JOIN dbo.CHRE_LST ON dbo.CHKREC_H.DATE = dbo.CHRE_LST.DATE INNER JOIN  dbo.PAY_GETD ON dbo.CHRE_LST.N_SERI = dbo.PAY_GETD.N_SERI AND dbo.CHRE_LST.BANK = dbo.PAY_GETD.BANK AND  dbo.CHRE_LST.DATE_S = dbo.PAY_GETD.DATE_S INNER JOIN dbo.TCOD_BANKS ON dbo.PAY_GETD.BANK = dbo.TCOD_BANKS.CODE WHERE     (dbo.CHKREC_H.DATE = " + DTS.Text.ToRawTarikh() + ")").ToList();
                //            foreach (var item in rst) //while (!rst.EOF)
                //            {
                //                //SDRST.AddNew(); //INSERT
                //                var _N_S_ = NNS;

                //                string? HES_K = null;
                //                string? HES_M = null;
                //                string? HES_T = null;
                //                string? HES_T2 = null;
                //                string? HES_T3 = null;
                //                string? HES_T4 = null;
                //                string? hes = null;

                //                if (item.KIND == 1 || item.KIND == null)
                //                {
                //                    HES_K = CKOL.ToStringNullSafe();
                //                    HES_M = CMOIN.ToStringNullSafe();
                //                    HES_T = CTAF.ToStringNullSafe();
                //                    HES_T2 = CTAF2.ToStringNullSafe();
                //                    HES_T3 = CTAF3.ToStringNullSafe();
                //                    HES_T4 = CTAF4.ToStringNullSafe();
                //                    hes = Baseknow.ADA;
                //                }
                //                else
                //                {
                //                    HES_K = CKOLV.ToStringNullSafe();
                //                    HES_M = CMOINV.ToStringNullSafe();
                //                    HES_T = CTAFV.ToStringNullSafe();
                //                    HES_T2 = CTAF2V.ToStringNullSafe();
                //                    HES_T3 = CTAF3V.ToStringNullSafe();
                //                    HES_T4 = CTAF4V.ToStringNullSafe();
                //                    hes = Baseknow.ADV;
                //                }
                //                var N_SERI = item.N_SERI;
                //                var BANK = item.BANK;
                //                var BES = item.MABL;
                //                var SHARH = Strings.Left(" چك " + item.N_SERI + " بانك " + item.NAMES + " " + item.SHOBEH + " مورخ " + Strings.Format(item.DATE_S, "####/##/##"), 255);
                //                //SDRST.update();
                //                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES, HES_T2, HES_T3, HES_T4)
                //                                 VALUES({_N_S_},
                //                                 {HES_K},
                //                                 {HES_M} ,
                //                                 {HES_T} ,
                //                                 N'{SHARH}' ,
                //                                 0 ,
                //                                 {BES} ,
                //                                 {N_SERI} ,
                //                                 {BANK} ,
                //                                 N'{hes}' ,
                //                                 {(string.IsNullOrEmpty(HES_T2) ? "NULL" : HES_T2)} ,
                //                                 {(string.IsNullOrEmpty(HES_T3) ? "NULL" : HES_T3)} ,
                //                                 {(string.IsNullOrEmpty(HES_T4) ? "NULL" : HES_T4)} )");

                //                CL_HESABDARI.GETTAF3(item.HES1, ref CKOLD, ref CMOIND, ref CTAFD, ref CTAF2D, ref CTAF3D, ref CTAF4D);

                //                //SDRST.AddNew();
                //                //SDRST.Fields("N_S") = NNS;
                //                //SDRST.Fields("HES_K") = CKOLD;
                //                //SDRST.Fields("HES_M") = CMOIND;
                //                //SDRST.Fields("HES_T") = CTAFD;
                //                //SDRST.Fields("HES_T2") = CTAF2D;
                //                //SDRST.Fields("HES_T3") = CTAF3D;
                //                //SDRST.Fields("HES_T4") = CTAF4D;
                //                //SDRST.Fields("hes") = item.HES1;
                //                //SDRST.Fields("N_SERI") = item.N_SERI;
                //                //SDRST.Fields("BANK") = item.BANK;
                //                //SDRST.Fields("BED") = item.MABL;

                //                var SHARH_ = Strings.Left(" چك " + item.N_SERI + " بانك " + item.NAMES + " " + item.SHOBEH + " مورخ " + Strings.Format(item.DATE_S, "####/##/##"), 255);
                //                //SDRST.update();

                //                dbms.DoExecuteSQL($@"INSERT INTO dbo.DEED_DTL(N_S, HES_K, HES_M, HES_T, SHARH, BED, BES, N_SERI, BANK, HES, HES_T2, HES_T3, HES_T4)
                //                                 VALUES({NNS},
                //                                 {CKOLD},
                //                                 {CMOIND} ,
                //                                 {CTAFD} ,
                //                                 N'{SHARH_}' ,
                //                                 {item.MABL} ,
                //                                 0 ,
                //                                 {item.N_SERI} ,
                //                                 {item.BANK} ,
                //                                 N'{item.HES1}' ,
                //                                 {(string.IsNullOrEmpty(CTAF2D.ToStringNullSafe()) ? "NULL" : CTAF2D)} ,
                //                                 {(string.IsNullOrEmpty(CTAF3D.ToStringNullSafe()) ? "NULL" : CTAF3D)} ,
                //                                 {(string.IsNullOrEmpty(CTAF4D.ToStringNullSafe()) ? "NULL" : CTAF4D)} )");

                //                item.N_S = NNS;
                //                mab = (double)item.MABL;
                //                //rst.update();
                //            }
                //            new Msgwin(false, "ثبت در سند شماره :" + NNS + "  شماره مبناي سند : " + NB + "  به مبلغ : " + Strings.Format(Convert.ToInt64(mab), "#,###")).ShowDialog();
                //            //this.serial.SetFocus();
                //        }
                //    }
                //    else
                //    {
                //        new Msgwin(false, "پيدا نشد").ShowDialog();
                //        serial.Focus();
                //    }
                //}
                //else if (false/*Strings.Val(this.serial) == Forms["CHECK_PVLIST"]["N_SERI"] || this.serial == ""*/) //#Check:
                //{
                //    //SHRST.Filter = "NO_S = 7 AND DATE_S = " + this.DTS;
                //    //if (SHRST.RecordCount == 0)
                //    //{
                //    //    SHRST.AddNew();
                //    //    SHRST.Fields("N_S") = max_ns;
                //    //    SHRST.Fields("DATE_S") = this.DTS;
                //    //    SHRST.Fields("SHARH_S") = "اعلام وصول چكهاي پرداختي";
                //    //    SHRST.Fields("GHATEI") = 0;
                //    //    SHRST.Fields("NO_S") = 7;
                //    //    SHRST.Fields("USER_NAME") = UCurrentUser();
                //    //    SHRST.Fields("CRT") = DateTime.Now;
                //    //    SHRST.Fields("UID") = Forms["BASEKNOW"]["USERCOD"];
                //    //    SHRST.update();
                //    //    NB = SHRST.Fields("base");
                //    //}
                //    //else
                //    //{
                //    //    max_ns = SHRST.Fields("N_S");
                //    //    NB = SHRST.Fields("base");
                //    //}
                //    //NNS = max_ns;
                //    //rst.Close();
                //    //rst.Open("SELECT * FROM CHREC_HP where [DATE]=" + this.DTS, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //    //if (rst.RecordCount == 0)
                //    //{
                //    //    rst.AddNew();
                //    //    rst.Fields("DATE") = this.DTS;
                //    //    rst.Fields("MOLAH") = "وصولي ثبت شده توسط گزارش";
                //    //    rst.Fields("N_S") = max_ns;
                //    //    rst.update();
                //    //}
                //    //rst.Close();
                //    //rst.Open("SELECT * FROM CHRE_LSP WHERE N_SERI =" + frm.Form.N_SERI + " AND BANK = " + frm.Form.BANK + " AND DATE_S = " + frm.Form.DATE_S, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //    //if (rst.RecordCount == 0)
                //    //{
                //    //    rst.AddNew();
                //    //    rst.Fields("DATE") = this.DTS;
                //    //    rst.Fields("DATE_S") = frm.Form.DATE_S;
                //    //    rst.Fields("N_SERI") = frm.Form.N_SERI;
                //    //    rst.Fields("BANK") = frm.Form.BANK;
                //    //    rst.update();
                //    //}
                //    //rst.Close();
                //    //rst.Open("SELECT * FROM PAY_GETP WHERE N_SERI =" + frm.Form.N_SERI + " AND BANK = " + frm.Form.BANK + " AND DATE_S = " + frm.Form.DATE_S, CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //    //if (rst.RecordCount == 0)
                //    //{
                //    //}
                //    //else
                //    //{
                //    //    rst.Fields("N_KOL3") = GETKOL(this.HHMOIN);
                //    //    rst.Fields("N_MOIN3") = GETMOIN(this.HHMOIN);
                //    //    rst.Fields("N_TAF3") = GETTAF(this.HHMOIN);
                //    //    rst.Fields("HES1") = this.HHMOIN;
                //    //    rst.Fields("HES3") = this.HHMOIN;
                //    //    rst.Fields("N_S") = NNS;
                //    //    rst.update();
                //    //}
                //    //// Call GETDLOG(3, frm.Form.N_SERI, frm.Form.BANK, frm.Form.DATE_S, frm.Form.SANDUGH)
                //    //mab = rst.Fields("MABL");
                //    //if (rst.Fields("KIND") == 1 || IsNull(rst.Fields("KIND")))
                //    //{
                //    //    HAPA = Forms["BASEKNOW"]["APA"];
                //    //}
                //    //else
                //    //{
                //    //    HAPA = Forms["BASEKNOW"]["APV"];
                //    //}
                //    //DoCmd.RunSQL("DELETE  FROM DEED_DTL WHERE (((DEED_DTL.N_S)= " + max_ns + " ))");
                //    //rst.Close();
                //    //if (!IsNull(Forms["BASEKNOW"]["ADA"]))
                //    //{
                //    //    GETTAF3(Forms["BASEKNOW"]["APA"], CKOL, CMOIN, CTAF, CTAF2, CTAF3, CTAF4);
                //    //}
                //    //if (!IsNull(Forms["BASEKNOW"]["ADV"]))
                //    //{
                //    //    GETTAF3(Forms["BASEKNOW"]["APV"], CKOLV, CMOINV, CTAFV, CTAF2V, CTAF3V, CTAF4V);
                //    //}
                //    //rst.Open("SELECT     dbo.CHREC_HP.idh, dbo.CHRE_LSP.*, dbo.TCOD_BANKS.NAMES, dbo.PAY_GETP.SHOBEH, dbo.PAY_GETP.hes1, dbo.PAY_GETP.hes2, dbo.PAY_GETP.hes3, dbo.PAY_GETP.SHOBEH,dbo.PAY_GETP.N_S, dbo.PAY_GETP.MABL, dbo.PAY_GETP.N_KOL,  dbo.PAY_GETP.N_MOIN, dbo.PAY_GETP.N_TAF, dbo.PAY_GETP.KIND  FROM         dbo.CHREC_HP INNER JOIN dbo.CHRE_LSP ON dbo.CHREC_HP.DATE = dbo.CHRE_LSP.DATE INNER JOIN  dbo.PAY_GETP ON dbo.CHRE_LSP.N_SERI = dbo.PAY_GETP.N_SERI AND dbo.CHRE_LSP.BANK = dbo.PAY_GETP.BANK AND  dbo.CHRE_LSP.DATE_S = dbo.PAY_GETP.DATE_S INNER JOIN dbo.TCOD_BANKS ON dbo.PAY_GETP.BANK = dbo.TCOD_BANKS.CODE WHERE     (dbo.CHREC_HP.DATE = " + this.DTS + ")", CurrentProject.Connection, adOpenKeyset, adLockOptimistic);
                //    //while (!rst.EOF)
                //    //{
                //    //    SDRST.AddNew();
                //    //    SDRST.Fields("N_S") = NNS;
                //    //    if (rst.Fields("KIND") == 1 || IsNull(rst.Fields("KIND")))
                //    //    {
                //    //        SDRST.Fields("HES_K") = CKOL;
                //    //        SDRST.Fields("HES_M") = CMOIN;
                //    //        SDRST.Fields("HES_T") = CTAF;
                //    //        SDRST.Fields("HES_T2") = CTAF2;
                //    //        SDRST.Fields("HES_T3") = CTAF3;
                //    //        SDRST.Fields("HES_T4") = CTAF4;
                //    //        SDRST.Fields("hes") = Forms["BASEKNOW"]["APA"];
                //    //    }
                //    //    else
                //    //    {
                //    //        SDRST.Fields("HES_K") = CKOLV;
                //    //        SDRST.Fields("HES_M") = CMOINV;
                //    //        SDRST.Fields("HES_T") = CTAFV;
                //    //        SDRST.Fields("HES_T2") = CTAF2V;
                //    //        SDRST.Fields("HES_T3") = CTAF3V;
                //    //        SDRST.Fields("HES_T4") = CTAF4V;
                //    //        SDRST.Fields("hes") = Forms["BASEKNOW"]["APV"];
                //    //    }
                //    //    SDRST.Fields("N_SERI") = rst.Fields("N_SERI");
                //    //    SDRST.Fields("BANK") = rst.Fields("BANK");
                //    //    SDRST.Fields("BED") = rst.Fields("MABL");
                //    //    SDRST.Fields("SHARH") = Strings.Left(" چك " + rst.Fields("N_SERI") + " بانك " + rst.Fields("NAMES") + " " + rst.Fields("SHOBEH") + " مورخ " + Strings.Format(rst.Fields("DATE_S"), "####/##/##"), 255);
                //    //    SDRST.update();
                //    //    GETTAF3(rst.Fields("hes1"), CKOLD, CMOIND, CTAFD, CTAF2D, CTAF3D, CTAF4D);
                //    //    SDRST.AddNew();
                //    //    SDRST.Fields("N_S") = NNS;
                //    //    SDRST.Fields("HES_K") = CKOLD;
                //    //    SDRST.Fields("HES_M") = CMOIND;
                //    //    SDRST.Fields("HES_T") = CTAFD;
                //    //    SDRST.Fields("HES_T2") = CTAF2D;
                //    //    SDRST.Fields("HES_T3") = CTAF3D;
                //    //    SDRST.Fields("HES_T4") = CTAF4D;
                //    //    SDRST.Fields("hes") = rst.Fields("hes1");
                //    //    SDRST.Fields("N_SERI") = rst.Fields("N_SERI");
                //    //    SDRST.Fields("BANK") = rst.Fields("BANK");
                //    //    SDRST.Fields("BES") = rst.Fields("MABL");
                //    //    SDRST.Fields("SHARH") = Strings.Left(" چك " + rst.Fields("N_SERI") + " بانك " + rst.Fields("NAMES") + " " + rst.Fields("SHOBEH") + " مورخ " + Strings.Format(rst.Fields("DATE_S"), "####/##/##"), 255);
                //    //    SDRST.update();
                //    //    rst.Fields("N_S") = NNS;
                //    //    rst.update();
                //    //    rst.MoveNext();
                //    //}
                //    //DoCmd.OpenForm("MESAG", default, default, default, default, acDialog, "ثبت در سند شماره :" + NNS + "  شماره مبناي سند : " + NB + "  به مبلغ : " + Strings.Format(mab, "#,###"));
                //    //this.serial.SetFocus();
                //    //frm.Requery();
                //}
                //else
                //{
                //    new Msgwin(false, "پيدا نشد").ShowDialog();
                //    serial.Focus();
                //}
            }
            //else #Check
            //{
            //    Forms["BASEKNOW"]["Text44"] = true;
            //    if (IsLoaded("CHECK_PVLIST"))
            //    {
            //        this.HHMOIN = Forms["CHECK_PVLIST"]["HES1"];
            //    }
            //    DoCmd.OpenForm(this.NAME, default, default, default, default, acHidden);
            //}

            if (WIN is CHEK_VLISTALL)
            {
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.DTS_DATE = DTS.Text.ToRawTarikh();
                (WIN as CHEK_VLISTALL).ITEM_SELECTED_VOSUL.HHMOIN_VOSUL = HHMOIN.SelectedValue.ToStringNullSafe();
            }

            if (WIN is CHECK_PVLIST)
            {
                (WIN as CHECK_PVLIST).ITEM_SELECTED_VOSUL.DTS_DATE = DTS.Text.ToRawTarikh();
                (WIN as CHECK_PVLIST).ITEM_SELECTED_VOSUL.HHMOIN_VOSUL = HHMOIN.SelectedValue.ToStringNullSafe();
            }

            this.Close();

            //this.Hide();
        }

        public Visual I_AM_VOSULDIALOG { get; private set; }
        public FULL_HESAB HESAB_FROM_SEARCH { get; set; } = new();
        private void HHMOIN_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (HHMOIN.IsEditable) { if (!(e.OriginalSource is TextBox)) return; }
            TextBox _THE_TEXTBOX_ = (TextBox)HHMOIN.Template.FindName("PART_EditableTextBox", HHMOIN);
            if (HHMOIN.SelectedValue is not null)
            {
                if ((HHMOIN?.SelectedItem as CUST_HESAB)?.NAME == HHMOIN?.Text)
                {
                    return;
                }
            }
            if (string.IsNullOrEmpty(_THE_TEXTBOX_.Text) || string.IsNullOrWhiteSpace(_THE_TEXTBOX_.Text))
            {
                universControl.PopNotifyShow("مقدار وارد شده خالی است", Pop1, Pop1Text1, Pop_Border1);
                return;
            }

            CL_LMethods.GetSearchedValueCustomer(HHMOIN, "VOSULDIALOG", default, dbms, I_AM_VOSULDIALOG, false);

            if (!string.IsNullOrEmpty(HESAB_FROM_SEARCH.FULL_HES))
            {
                HHMOIN.SelectedValue = HESAB_FROM_SEARCH.FULL_HES;
            }
            else
            {
                HHMOIN.SelectedValue = null;

                universControl.PopNotifyShow("حسابی صحیح انتخاب نشده !", Pop1, Pop1Text1, Pop_Border1);
            }

            HHMOIN.Items.Refresh();
            HESAB_FROM_SEARCH.DoClear();
        }

        private bool IS_DATE_VALID(string _DATE_)
        {
            string date_n_val = _DATE_.ToRawTarikh();
            if (!string.IsNullOrEmpty(date_n_val))
            {
                if (!Tarikh.IsValidedDate(date_n_val))
                {
                    universControl.PopNotifyShow("مقدار تاریخ صحیح نیست.", Pop1, Pop1Text1, Pop_Border1);
                    return false;
                }
                else
                {
                    if (!Tarikh.IsSyncedDateNow(date_n_val, (bool)Baseknow.CTL_DT))
                    {
                        universControl.PopNotifyShow(".تاریخ مربوط به سال جاری نیست", Pop1, Pop1Text1, Pop_Border1);
                        return false;
                    }
                }
            }
            else
            {
                universControl.PopNotifyShow("تاریخ نمی تواند خالی باشد.", Pop1, Pop1Text1, Pop_Border1);
                return false;
            }

            return true;
        }

        private void DTS_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IS_DATE_VALID(DTS.Text.ToRawTarikh()))
            {
                e.Handled = true;
            }
        }
        private void serial_NumericLostFocus(object sender, RoutedEventArgs e)
        {
            //RowsChecks
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsAlive = false;
        }
    }
}
