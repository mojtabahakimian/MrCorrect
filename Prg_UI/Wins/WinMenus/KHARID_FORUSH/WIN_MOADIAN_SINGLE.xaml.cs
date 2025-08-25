using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Functions;
using ImageMagick;
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
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Stimulsoft.Data.Expressions.NCalc;
using Stimulsoft.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Prg_UI.Wins.WinMenus.KHARID_FORUSH.HEAD_LST_FROOSH22;
using static Wins.WinMenus.ANBAR.HEAD_LST_HAV_OTHER_WIN;

namespace Wins.WinMenus.KHARID_FORUSH
{
    public partial class WIN_MOADIAN_SINGLE : Window
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

        //868
        public WIN_MOADIAN_SINGLE(double? number_to_open = null)
        {
            NUMBER_TO_OPEN = (double)number_to_open;

            InitializeComponent();
        }

        CL_CCNNMANAGER dbms = new CL_CCNNMANAGER();

        UniversControl universControl = new UniversControl();

        public bool NowIsReady { get; private set; }
        public bool NewRecord { get; set; }
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

        bool MoadianHeaderIsOk = false;
        public double NUMBER_TO_OPEN { get; }
        private bool _isExporty;
        /// <summary>
        /// فاکتور فروش صادراتی
        /// </summary>
        public bool IsExporty
        {
            get { return _isExporty; }
            set
            {
                _isExporty = value;
                if (_isExporty)
                {
                }
                else
                {
                }
            }
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            NowIsReady = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FILL_ALL_COMBOBOXES();

            #region LOAD_FACTOR

            NUMBER.Text = NUMBER_TO_OPEN.ToStringNullSafe();
            var HEADER_FAC = dbms.DoGetDataSQL<HEAD_LST>($"SELECT * FROM HEAD_LST WHERE NUMBER = {NUMBER.Text} AND TAG = 13").FirstOrDefault();
            DATE_N.Text = HEADER_FAC.DATE_N.ToStringNullSafe(); //تاریخ فاکتور

            string thevalue = HEADER_FAC.CUST_NO;
            var data = dbms.DoGetDataSQL<CUST_HESAB>("SELECT hes, NAME FROM dbo.CUST_HESAB WHERE hes = N'" + thevalue + "'").FirstOrDefault();
            if (CUST_NO.ItemsSource == null)
            {
                CUST_NO.ItemsSource = new List<Custom_CUST_HESAB>();
            }
            if (!((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Any(item => item?.hes == thevalue))
            {
                ((List<Custom_CUST_HESAB>)CUST_NO.ItemsSource).Add(new Custom_CUST_HESAB { hes = thevalue, NAME = data.NAME });
            }
            CUST_NO2.ItemsSource = CUST_NO.ItemsSource;
            //مشتری
            CUST_NO.SelectedValue = HEADER_FAC.CUST_NO; CUST_NO.Items.Refresh();

            MOLAH.Text = HEADER_FAC.MOLAH; //ملاحظات

            M_NAGHD.Text = HEADER_FAC.M_NAGHD.ToStringNullSafe(); //مبلغ نقد
            MABL_VAR.Text = (string.IsNullOrEmpty(HEADER_FAC.MABL_VAR.ToStringNullSafe()) ? "0" : HEADER_FAC.MABL_VAR.ToStringNullSafe()); //مبلغ کارت بانک
            MABL_HAV.Text = (string.IsNullOrEmpty(HEADER_FAC.MABL_HAV.ToStringNullSafe()) ? "0" : HEADER_FAC.MABL_HAV.ToStringNullSafe()); //مبلغ بن یا حواله
            TAKHFIF.Text = (string.IsNullOrEmpty(HEADER_FAC.TAKHFIF.ToStringNullSafe()) ? "0" : HEADER_FAC.TAKHFIF.ToStringNullSafe()); //مبلغ تخفیف

            //پشت فاکتور
            MABL_HAZ.Text = (string.IsNullOrEmpty(HEADER_FAC.MABL_HAZ.ToStringNullSafe()) ? "0" : HEADER_FAC.MABL_HAZ.ToStringNullSafe()); //مبلغ خدمات
            MBAA.Text = (string.IsNullOrEmpty(HEADER_FAC.MBAA.ToStringNullSafe()) ? "0" : HEADER_FAC.MBAA.ToStringNullSafe()); //مالیات و عوارض مبلغ

            #endregion


            var MoadianHead = dbms.DoGetDataSQL<HEAD_LST_EXTENDED>($"SELECT * FROM dbo.HEAD_LST_EXTENDED WHERE NUMBER = {NUMBER_TO_OPEN} AND TGU = 2").FirstOrDefault();
            if (MoadianHead != null)
            {
                inty.SelectedValue = MoadianHead.inty; inty.Items.Refresh();
                inp.SelectedValue = MoadianHead.inp; inp.Items.Refresh();
                ins.SelectedValue = MoadianHead.ins; ins.Items.Refresh();
                sbc.Text = MoadianHead.sbc;
                bbc.Text = MoadianHead.bbc;
                if (MoadianHead.ft != null)
                {
                    ft.Text = MoadianHead.ft.ToStringNullSafe();
                }
                bpn.Text = MoadianHead.bpn;
                scln.Text = MoadianHead.scln;
                scc.Text = MoadianHead.scc;
                cdcn.Text = MoadianHead.cdcn;
                if (MoadianHead.cdcd != null)
                {
                    cdcd.Text = MoadianHead.cdcd.ToStringNullSafe();
                }
                crn.Text = MoadianHead.crn;
                billid.Text = MoadianHead.billid;
                if (MoadianHead.todam != null)
                {
                    todam.Text = MoadianHead.todam.ToStringNullSafe();
                }
                if (MoadianHead.tonw != null)
                {
                    tonw.Text = MoadianHead.tonw.ToStringNullSafe();
                }
                if (MoadianHead.torv != null)
                {
                    torv.Text = MoadianHead.torv.ToStringNullSafe();
                }
                if (MoadianHead.tocv != null)
                {
                    tocv.Text = MoadianHead.tocv.ToStringNullSafe();
                }
                setm.SelectedValue = MoadianHead.setm; setm.Items.Refresh();
                if (MoadianHead.cap != null && MoadianHead.cap > 0)
                {
                    cap.Text = MoadianHead.cap.ToStringNullSafe();
                }
                if (MoadianHead.insp != null && MoadianHead.insp > 0)
                {
                    insp.Text = MoadianHead.insp.ToStringNullSafe();
                }
                if (MoadianHead.tvop != null)
                {
                    tvop.Text = MoadianHead.tvop.ToStringNullSafe();
                }
                if (MoadianHead.tax17 != null)
                {
                    tax17.Text = MoadianHead.tax17.ToStringNullSafe();
                }
                if (IsExporty)
                {
                    CUT.SelectedValue = MoadianHead.cut; CUT.Items.Refresh();
                }
                if (!string.IsNullOrEmpty(MoadianHead.irtaxid))
                {
                    irtaxid.Text = MoadianHead.irtaxid;
                }

            }


            // جمع ها ***************************************************
            var _Pursant_ = dbms.DoGetDataSQL<double?>($"SELECT SUM(PURSANT) FROM dbo.VISITOR_DTL WHERE NUMBER = {NUMBER_TO_OPEN} AND TAG = 2").FirstOrDefault();
            double sum = _Pursant_ ?? 0.0; //جمع پورسانت Text190

            //مبلغ کالا ها
            var _sum_of_mabl_k_ = dbms.DoGetDataSQL<double?>($"SELECT SUM(MABL_K) FROM dbo.INVO_LST WHERE NUMBER = {NUMBER_TO_OPEN} AND TAG = 2").FirstOrDefault();
            var SUM_OF_MABL_K = _sum_of_mabl_k_ ?? 0.0;

            //مقدار کالا ها
            var _sum_of_megh_k_ = dbms.DoGetDataSQL<double?>($"SELECT SUM(MEGHk) FROM dbo.INVO_LST WHERE NUMBER = {NUMBER_TO_OPEN} AND TAG = 2").FirstOrDefault();
            var SUM_OF_MEGH_K = _sum_of_megh_k_ ?? 0.0; //جمع مقادیر :

            //مبلغ چکها
            var _nchk_ = dbms.DoGetDataSQL<double?>($"SELECT SUM(MABL) FROM dbo.PAY_GETD WHERE NUMBER = {NUMBER_TO_OPEN} AND TAG = 2 AND (N_KOL IS NULL OR N_KOL <> 911)").FirstOrDefault();
            NCHK.Text = (_nchk_ ?? 0).ToString();

            var JJKOL = SUM_OF_MABL_K.ToString(); //SMABLK //جمع فاکتور :
            HKH.Text = string.IsNullOrEmpty(MABL_HAZ.Text) ? "0" : MABL_HAZ.Text; // هزینه خدمات
            NTKHFIF.Text = TAKHFIF.Text; //تخفیفات
            JF.Text = JJKOL; //جمع کل فاکتور برای فسمت روی فاکتور

            //مبلغ قابل پرداخت:
            var rghabel = Convert.ToInt64(JF.Text) + Convert.ToInt64(HKH.Text) - Convert.ToInt64(NTKHFIF.Text) + Convert.ToInt64(MBAA.Text);
            GHABEL.Text = rghabel.ToString();

            //جمع مبالغ پرداختی
            var RMP = Convert.ToInt64(M_NAGHD.Text) + Convert.ToInt64(MABL_VAR.Text) + Convert.ToInt64(MABL_HAV.Text) + Convert.ToInt64(NCHK.Text);
            NPAR.Text = RMP.ToString();

            // مانده روی فاکتور
            MAN.Text = Convert.ToString(Convert.ToInt64(GHABEL.Text) - Convert.ToInt64(NPAR.Text)); //مانده


            ////***************************************
            var _M_NAGHD_ = Convert.ToInt64(M_NAGHD.Text);
            var _MABL_VAR_ = Convert.ToInt64(MABL_VAR.Text);
            var _MABL_HAV_ = Convert.ToInt64(MABL_HAV.Text);
            var _NCHK_ = Convert.ToInt64(NCHK.Text);

            var CC = _M_NAGHD_ + _MABL_VAR_ + _MABL_HAV_ + _NCHK_;

            var _GHABEL_ = Convert.ToInt64(GHABEL.Text);
            var _MBAA_ = Convert.ToInt64(MBAA.Text);

            var _insp_ = _GHABEL_ - _MBAA_ - CC;
            insp.Text = _insp_.ToStringNullSafe();
            cap.Text = CC.ToStringNullSafe();
            ////***************************************

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
            if (IsExporty)
            {
                //نوع ارز در مودیان
                CUT.ItemsSource = dbms.DoGetDataSQL<TCOD_ARZ>($"SELECT ID,Code, Title, ISOCode, (ISOCode+N' - '+Title+N' - '+CountryName) AS ARZCOUNTRY, CRT, UID FROM dbo.[TCOD_ARZ]").ToList();
            }

            //نوع صورتحساب:
            inty.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "نوع اول" },
                new COMBOYMODEL { ID = 2, NAME = "نوع دوم" },
                new COMBOYMODEL { ID = 3, NAME = "نوع سوم" }
            }; inty.SelectedValue = 1; inty.Items.Refresh();

            //الگوی صورتحساب:
            inp.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "فروش" },
                new COMBOYMODEL { ID = 2, NAME = "فروش ارزی" },
                new COMBOYMODEL { ID = 3, NAME = "طلاوجواهر" },
                new COMBOYMODEL { ID = 4, NAME = "پیمانکاری" },
                new COMBOYMODEL { ID = 5, NAME = "قبوض خدماتی" },
                new COMBOYMODEL { ID = 6, NAME = "بلیط هواپیما" },
                new COMBOYMODEL { ID = 7, NAME = "صادرات" }
            }; inp.SelectedValue = 1; inp.Items.Refresh();

            //الگوی صورتحساب:
            ins.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "اصلی" },
                new COMBOYMODEL { ID = 2, NAME = "اصلاحی" },
                new COMBOYMODEL { ID = 3, NAME = "ابطالی" },
                new COMBOYMODEL { ID = 4, NAME = "برگشت فروش" }
            }; ins.SelectedValue = 1; ins.Items.Refresh();

            //روش تسویه:
            setm.ItemsSource = new List<COMBOYMODEL>
            {
                new COMBOYMODEL { ID = 1, NAME = "نقد" },
                new COMBOYMODEL { ID = 2, NAME = "نسیه" },
                new COMBOYMODEL { ID = 3, NAME = "نقد/نسیه" }
            }; setm.SelectedValue = 2; setm.Items.Refresh();

        }

        private bool MoadianIsValid(bool displayErrors = true)
        {
            var errorMessages = new List<MsgModel>();

            try
            {
                if (ins.SelectedValue.ToStringNullSafe() != "1" && string.IsNullOrWhiteSpace(irtaxid.Text))
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "شناسه مالیاتی صورتحساب نباید خالی باشد." });
                }

                if (inty.SelectedItem == null)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً نوع صورتحساب را انتخاب کنید." });
                }
                else
                {
                    if (Convert.ToInt32(inty.SelectedValue) == 1) //نوع اول
                    {
                        if (setm.SelectedItem == null)
                        {
                            errorMessages.Add(new MsgModel { MessageText_U = "روش تسویه انتخاب نشده است." });
                        }
                        else
                        {
                            var selectedSettlementMethod = setm.SelectedItem.ToString();

                            if (selectedSettlementMethod == "نقد/نسیه" || Convert.ToInt32(setm.SelectedValue) == 3)
                            {
                                if (!decimal.TryParse(insp.Text, out decimal inspValue) || inspValue < 0)
                                {
                                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً مقدار صحیحی برای مبلغ نسیه وارد کنید." });
                                }
                            }
                        }
                    }
                }

                if (inp.SelectedItem == null)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "الگوی صورتحساب انتخاب نشده است." });
                }

                if (ins.SelectedItem == null)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "موضوع صورتحساب انتخاب نشده است." });
                }

                if (!decimal.TryParse(torv.Text, out decimal torvValue) || torvValue < 0)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً مقدار صحیحی برای مجموع ارزش وارد کنید." });
                }

                if (!decimal.TryParse(tocv.Text, out decimal tocvValue) || tocvValue < 0)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "لطفاً مقدار صحیحی برای مبلغ پرداختی نقدی وارد کنید." });
                }

                if (!decimal.TryParse(cap.Text, out decimal capValue) || capValue < 0)
                {
                    errorMessages.Add(new MsgModel { MessageText_U = "مبلغ پرداختی نقدی به درستی وارد نشده است." });
                }

            }
            catch (Exception ex)
            {
                errorMessages.Add(new MsgModel { MessageText_U = $"خطا در اعتبارسنجی: {ex.Message}" });
            }

            if (errorMessages.Any())
            {
                if (displayErrors)
                {
                    errorMessages = errorMessages.Select(x => x.MessageText_U).Distinct()
                        .Select(message => new MsgModel { MessageText_U = message }).ToList();
                    new MsgListwin(false, errorMessages).ShowDialog();
                }
                return false;
            }

            return true;
        }


        private void BTN_SAVE_HEXTENDED_Click(object sender, RoutedEventArgs e)
        {
            MoadianHeaderIsOk = false;

            if (!MoadianIsValid())
            {
                return;
            }

            var HLE = dbms.DoGetDataSQL<HEAD_LST_EXTENDED>($"SELECT TOP 1 inty FROM dbo.HEAD_LST_EXTENDED WHERE NUMBER = {NUMBER_TO_OPEN} AND tgu = 2").FirstOrDefault();
            var IsNewMoadian = HLE == null;
            try
            {
                if (IsNewMoadian)
                {
                    dbms.DoExecuteSQL($@"INSERT INTO dbo.HEAD_LST_EXTENDED(NUMBER, tgu, inty, inp, ins, sbc, bbc, ft, bpn, scln, scc, cdcn, cdcd, crn, billid, todam, tonw, torv, tocv, setm, cap, insp, tvop, tax17, cut, irtaxid)
                                     VALUES({NUMBER_TO_OPEN},
                                     2 ,
                                     {inty.SelectedValue} ,
                                     {inp.SelectedValue}   ,
                                     {ins.SelectedValue}   ,
                                     N'{sbc.Text}' ,
                                     N'{bbc.Text}' ,
                                     {ft.Text} ,
                                     N'{bpn.Text}' ,
                                     N'{scln.Text}' ,
                                     N'{scc.Text}' ,
                                     N'{cdcn.Text}' ,
                                     {cdcd.Text}   ,
                                     N'{crn.Text}' ,
                                     N'{billid.Text}' ,
                                     {(string.IsNullOrEmpty(todam.Text) ? "NULL" : todam.Text)},
                                     {(string.IsNullOrEmpty(tonw.Text) ? "NULL" : tonw.Text)},
                                     {(string.IsNullOrEmpty(torv.Text) ? "NULL" : torv.Text)},
                                     {(string.IsNullOrEmpty(tocv.Text) ? "NULL" : tocv.Text)},
                                     {setm.SelectedValue} ,
                                     {(string.IsNullOrEmpty(cap.Text) ? "NULL" : cap.Text)},
                                     {insp.Text},
                                     {tvop.Text},
                                     {tax17.Text},
                                     N'{CUT.SelectedValue}' ,
                                     N'{irtaxid.Text}' )");
                }
                else
                {
                    dbms.DoExecuteSQL($@"UPDATE dbo.HEAD_LST_EXTENDED
                     SET inty = {inty.SelectedValue},
                         inp = {inp.SelectedValue},
                         ins = {ins.SelectedValue},
                         sbc = N'{sbc.Text}',
                         bbc = N'{bbc.Text}',
                         ft = {ft.Text},
                         bpn = N'{bpn.Text}',
                         scln = N'{scln.Text}',
                         scc = N'{scc.Text}',
                         cdcn = N'{cdcn.Text}',
                         cdcd = {cdcd.Text},
                         crn = N'{crn.Text}',
                         billid = N'{billid.Text}',
                         todam = {(string.IsNullOrEmpty(todam.Text) ? "NULL" : todam.Text)},
                         tonw = {(string.IsNullOrEmpty(tonw.Text) ? "NULL" : tonw.Text)},
                         torv = {(string.IsNullOrEmpty(torv.Text) ? "NULL" : torv.Text)},
                         tocv = {(string.IsNullOrEmpty(tocv.Text) ? "NULL" : tocv.Text)},
                         setm = {setm.SelectedValue},
                         cap = {(string.IsNullOrEmpty(cap.Text) ? "NULL" : cap.Text)},
                         insp = {insp.Text},
                         tvop = {tvop.Text},
                         tax17 = {tax17.Text},
                         cut = N'{CUT.SelectedValue}',
                         irtaxid = N'{irtaxid.Text}'
                     WHERE NUMBER = {NUMBER_TO_OPEN} AND tgu = 2");

                }
            }
            catch (Exception)
            {
                new Msgwin(false, "خطا در ذخیره صورت حساب برای مودیان , لطفا مقادیر را بررسی کنید").ShowDialog();
                return;
            }

            MoadianHeaderIsOk = true;

            if (sender != null)
            {
                universControl.PopNotifyShow($"ذخیره شد.", Pop1, Pop1Text1, Pop_Border1, "#FF1AAA2C");
            }
        }
        private void BTN_SEND_INVOICE_Click(object sender, RoutedEventArgs e)
        {
            if (!MoadianHeaderIsOk)
            {
                BTN_SAVE_HEXTENDED_Click(null, null);
            }

            if (MoadianHeaderIsOk)
            {
                try
                {
                    var _NUMBER_ = Convert.ToInt64(NUMBER_TO_OPEN);
                    var _TGU_ = Convert.ToInt32(2);

                    _ = AuditLogger.LogActionAsync(
                            actionType: "MOADIAN SEND BUTTON CALLED",
                            tableName: "ارسال صورت حساب مودیان",
                            recordId: $"NUMBER {_NUMBER_} TGU: {_TGU_}",
                            oldValue: null,
                            newValue: $" inty: {inty.SelectedValue} inp:{inp.SelectedValue} ins:{ins.SelectedValue} irtaxid:{irtaxid.Text} CUST_NO:{CUST_NO.SelectedValue}",
                            additionalInfo: $@"{this.GetType().Name} , EXE PATH : {System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}");

                    //CL_LMethods.DoWriteMyLog($"Baseknow.tindata : {Baseknow.tindata}");
      

                    if (CL_HESABDARI.MoadianLock(_NUMBER_, _TGU_))
                    {
                        string directoryPath = @"C:\CORRECT\";
                        string filePath = Path.Combine(directoryPath, "cnr.udl");

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Create a file and write connection string
                        using (StreamWriter writer = new StreamWriter(filePath, false))
                        {
                            writer.WriteLine(CL_CCNNMANAGER.CONNECTION_STR);
                        }

                        BTN_SEND_INVOICE.IsEnabled = false;

                        // Execute the external program
                        string arguments = $"{_NUMBER_}_{_TGU_}_m";
                        var PRC = Process.Start(new ProcessStartInfo
                        {
                            FileName = Path.Combine(directoryPath, "MOADIAN.EXE"),
                            Arguments = arguments,
                            UseShellExecute = true,
                            //WindowStyle = ProcessWindowStyle.Normal
                        });

                        PRC.WaitForExit();

                        BTN_SEND_INVOICE.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    new Msgwin(false, "خطا در انجام عملیات ارسال").ShowDialog();
                }
            }
        }

        private void moadian_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void setm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!NowIsReady) { return; }
            //new COMBOYMODEL { ID = 1, NAME = "نقد" },
            //if (Convert.ToInt32(setm.SelectedValue) == 1)
            //{

            //}


        }
    }

}
